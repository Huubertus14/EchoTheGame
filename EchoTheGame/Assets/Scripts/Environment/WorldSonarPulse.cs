using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>One radius drives both the visible ring and world-space surface reveal. No UVs or collision events.</summary>
public sealed class WorldSonarPulse : MonoBehaviour
{
    [Serializable]
    public sealed class Settings
    {
        [Min(0.1f)] public float range = 27.75f;
        [Min(0.05f)] public float duration = 1.85f;
        [Min(0.05f)] public float fadeDuration = 1.5f;
        [Min(0)] public float holdDuration = 2.5f;
        [Min(0.01f)] public float frontWidth = 0.35f;
        [Range(0, 12)] public int maxReflections = 8;
        [Range(1, 4)] public int maxBounces = 3;
        [Range(0, 1)] public float reflectionStrength = 0.55f;
        [Range(1, 60)] public float reflectionHalfAngle = 12;
        public AnimationCurve radiusCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public float Radius(float age)
        {
            float t = Mathf.Clamp01(age / Mathf.Max(0.05f, duration));
            return Mathf.Max(0.1f, range) * Mathf.Clamp01(radiusCurve.Evaluate(t));
        }

        public Vector4 Wave(float age, float travelled = 0)
        {
            return new Vector4(Radius(age) - travelled, frontWidth,
                ExtendedRadius(age - holdDuration) - travelled, ExtendedRadius(age - holdDuration - fadeDuration) - travelled);
        }

        private float ExtendedRadius(float age)
        {
            float speed = range / Mathf.Max(0.05f, duration);
            if (age < 0) return age * speed;
            if (age > duration) return range + (age - duration) * speed;
            return Radius(age);
        }
    }

    public const int MaxPulses = 32;
    private const int RingSegments = SonarPropagation.Samples;
    private sealed class Pulse
    {
        public Vector3 origin;
        public Color color;
        public Settings settings;
        public float started;
        public LineRenderer ring;
        public SonarPropagation.Profile propagation;
        public float travelled;
        public bool reflected;
        public int bounces;
        public MaterialPropertyBlock properties = new();
        public Vector3[] points;
    }

    private static WorldSonarPulse instance;
    private readonly List<Pulse> pulses = new();
    private readonly Vector4[] origins = new Vector4[MaxPulses];
    private readonly Vector4[] waves = new Vector4[MaxPulses];
    private readonly Vector4[] colors = new Vector4[MaxPulses];
    private readonly Vector4[] sectors = new Vector4[MaxPulses];
    private Material ringMaterial;
    private Texture2D occlusion;
    private bool maskDirty;
    private readonly List<(Pulse pulse, SonarPropagation.Reflection hit)> pendingReflections = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetGlobals()
    {
        instance = null;
        Shader.SetGlobalInt("_EchoSonarCount", 0);
        Shader.SetGlobalInt("_EchoOcclusionEnabled", 0);
    }

    public static void Emit(Vector3 origin, Color color, Settings settings)
    {
        if (!instance) instance = new GameObject("World sonar pulses").AddComponent<WorldSonarPulse>();
        Physics.SyncTransforms();
        instance.Add(origin, color, settings, Time.time);
        instance.UpdatePulses();
    }

    private void Add(Vector3 origin, Color color, Settings settings, float started,
        float travelled = 0, Vector3 direction = default, bool reflected = false, int bounces = 0)
    {
        if (pulses.Count == MaxPulses)
        {
            // Reflections never evict a primary wave just to add another echo.
            if (reflected) return;
            int oldestEcho = pulses.FindIndex(p => p.reflected);
            int removed = oldestEcho >= 0 ? oldestEcho : 0;
            Destroy(pulses[removed].ring.gameObject);
            pulses.RemoveAt(removed);
        }
        if (!occlusion)
        {
            occlusion = new Texture2D(SonarPropagation.Samples, MaxPulses, TextureFormat.RGBAFloat, false, true)
            {
                name = "Sonar acoustic shadow atlas", filterMode = FilterMode.Point,
                wrapModeU = TextureWrapMode.Repeat, wrapModeV = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
        }
        if (!ringMaterial)
        {
            ringMaterial = new Material(Resources.Load<Material>("SonarRing"));
            ringMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        var ring = new GameObject("Sonar ring").AddComponent<LineRenderer>();
        ring.transform.SetParent(transform, false);
        ring.sharedMaterial = ringMaterial;
        ring.useWorldSpace = true;
        ring.loop = !reflected;
        ring.positionCount = RingSegments;
        ring.widthMultiplier = 0.12f;
        ring.shadowCastingMode = ShadowCastingMode.Off;
        ring.receiveShadows = false;
        float halfAngle = reflected ? settings.reflectionHalfAngle : 180;
        int reflectionLimit = bounces >= settings.maxBounces ? 0
            : (reflected ? Mathf.Min(2, settings.maxReflections) : settings.maxReflections);
        var propagation = SonarPropagation.Trace(gameObject.scene.GetPhysicsScene(), origin, settings.range - travelled,
            reflectionLimit, direction, halfAngle);
        pulses.Add(new Pulse { origin = origin, color = color, settings = settings, started = started,
            ring = ring, propagation = propagation, travelled = travelled, reflected = reflected, bounces = bounces,
            points = new Vector3[reflected ? Mathf.Max(4, Mathf.CeilToInt(halfAngle / 180 * RingSegments) + 1) : RingSegments] });
        maskDirty = true;
    }

    private void LateUpdate() => UpdatePulses();

    private void UpdatePulses()
    {
        for (int i = pulses.Count - 1; i >= 0; i--)
        {
            var p = pulses[i];
            float age = Time.time - p.started;
            if (age >= p.settings.duration + p.settings.holdDuration + p.settings.fadeDuration)
            {
                Destroy(p.ring.gameObject);
                pulses.RemoveAt(i);
                maskDirty = true;
            }
        }
        pendingReflections.Clear();
        foreach (var pulse in pulses)
        {
            if (pulse.bounces >= pulse.settings.maxBounces) continue;
            float radius = pulse.settings.Radius(Time.time - pulse.started) - pulse.travelled;
            foreach (var hit in pulse.propagation.reflections)
            {
                if (hit.emitted || radius < hit.distance) continue;
                hit.emitted = true;
                pendingReflections.Add((pulse, hit));
            }
        }
        foreach (var pending in pendingReflections)
        {
            var source = pending.pulse;
            var hit = pending.hit;
            Color reflectedColor = source.color * source.settings.reflectionStrength;
            reflectedColor.a = source.color.a;
            Add(hit.origin, reflectedColor, source.settings,
                source.started, source.travelled + hit.distance, hit.direction, true, source.bounces + 1);
        }
        for (int i = 0; i < pulses.Count; i++)
        {
            var p = pulses[i];
            float age = Time.time - p.started;
            Vector4 wave = p.settings.Wave(age, p.travelled);
            origins[i] = new Vector4(p.origin.x, p.origin.z, p.settings.range - p.travelled, 0);
            waves[i] = wave;
            colors[i] = p.color;
            var profile = p.propagation;
            sectors[i] = new Vector4(profile.direction.x, profile.direction.z, Mathf.Cos(profile.halfAngle * Mathf.Deg2Rad), 0);
            if (maskDirty) occlusion.SetPixels(0, i, SonarPropagation.Samples, 1, profile.mask);
            p.properties.SetFloat("_EchoPulseSlot", i);
            p.ring.SetPropertyBlock(p.properties);
            p.ring.enabled = age < p.settings.duration && wave.x > 0;
            Color ringColor = p.color;
            ringColor.a *= Mathf.Clamp01((p.settings.duration - age) / 0.15f);
            p.ring.startColor = p.ring.endColor = ringColor;
            int points = p.points.Length;
            p.ring.positionCount = points;
            float heading = Mathf.Atan2(profile.direction.z, profile.direction.x);
            for (int j = 0; j < points; j++)
            {
                float angle = p.reflected
                    ? heading + Mathf.Lerp(-profile.halfAngle, profile.halfAngle, j / (float)(points - 1)) * Mathf.Deg2Rad
                    : j * Mathf.PI * 2 / points;
                p.points[j] = new Vector3(p.origin.x + Mathf.Cos(angle) * wave.x,
                    0.08f, p.origin.z + Mathf.Sin(angle) * wave.x);
            }
            p.ring.SetPositions(p.points);
        }
        if (occlusion && maskDirty) { occlusion.Apply(false, false); maskDirty = false; }
        Shader.SetGlobalVectorArray("_EchoSonarOrigins", origins);
        Shader.SetGlobalVectorArray("_EchoSonarWaves", waves);
        Shader.SetGlobalVectorArray("_EchoSonarColors", colors);
        Shader.SetGlobalVectorArray("_EchoSonarSectors", sectors);
        Shader.SetGlobalTexture("_EchoSonarOcclusion", occlusion);
        Shader.SetGlobalInt("_EchoOcclusionEnabled", 1);
        Shader.SetGlobalInt("_EchoSonarCount", pulses.Count);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            Shader.SetGlobalInt("_EchoSonarCount", 0);
            Shader.SetGlobalInt("_EchoOcclusionEnabled", 0);
        }
        if (ringMaterial) Destroy(ringMaterial);
        if (occlusion) Destroy(occlusion);
    }
}
