using System;
using System.Collections.Generic;
using Project.Echo.Environment;
using UnityEngine;

/// <summary>XZ acoustic visibility sampled against static scene colliders, once per emitted wave.</summary>
public static class SonarPropagation
{
    public const int Samples = 1024;
    public const float SurfaceBias = 0.03f;
    private static readonly Dictionary<UnityEngine.Object, int> identities = new();
    private static int nextIdentity = 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetIdentities() { identities.Clear(); nextIdentity = 1; }

    public sealed class Reflection
    {
        public Vector3 origin;
        public Vector3 direction;
        public float distance;
        public bool emitted;
        public float targetId;
        public float priority;
    }

    public sealed class Profile
    {
        public readonly Color[] mask = new Color[Samples];
        public readonly List<Reflection> reflections = new();
        public Vector3 origin;
        public Vector3 direction;
        public float halfAngle = 180;

        public float SurfaceVisibility(Vector3 position, float objectId = 0)
        {
            if (!IsVisible(position, objectId)) return 0;
            Vector3 delta = position - origin;
            delta.y = 0;
            int index = Mathf.FloorToInt(Mathf.Repeat(Mathf.Atan2(delta.z, delta.x) / (Mathf.PI * 2), 1) * Samples + 0.5f);
            float coverage = 1;
            for (int offset = -1; offset <= 1; offset++)
            {
                Color hit = mask[(index + offset + Samples) % Samples];
                if (objectId == 0 || Mathf.Abs(hit.g - objectId) > 0.25f) continue;
                float t = Mathf.Clamp01((delta.magnitude - (hit.b - hit.a)) / Mathf.Max(0.001f, hit.a));
                coverage = Mathf.Min(coverage, 1 - t * t * (3 - 2 * t));
            }
            return coverage;
        }

        public bool IsVisible(Vector3 position, float objectId = 0)
        {
            Vector3 delta = position - origin;
            delta.y = 0;
            if (halfAngle < 180 && Vector3.Dot(delta.normalized, direction) < Mathf.Cos(halfAngle * Mathf.Deg2Rad)) return false;
            float angle = Mathf.Atan2(delta.z, delta.x) / (Mathf.PI * 2);
            int index = Mathf.FloorToInt(Mathf.Repeat(angle, 1) * Samples + 0.5f);
            for (int offset = -1; offset <= 1; offset++)
            {
                Color hit = mask[(index + offset + Samples) % Samples];
                if (delta.magnitude > hit.r + SurfaceBias && (objectId == 0 || Mathf.Abs(hit.g - objectId) > 0.25f)) return false;
            }
            return true;
        }
    }

    public static float ObjectId(Collider collider)
    {
        var marker = collider.GetComponentInParent<SonarObject>();
        UnityEngine.Object key = marker ? (UnityEngine.Object)marker : collider;
        if (!identities.TryGetValue(key, out int identity))
        {
            identity = nextIdentity++;
            identities.Add(key, identity);
        }
        return identity;
    }

    public static void AssignObjectId(Collider collider)
    {
        var marker = collider.GetComponentInParent<SonarObject>();
        var renderers = marker ? marker.GetComponentsInChildren<Renderer>() : collider.GetComponents<Renderer>();
        var block = new MaterialPropertyBlock();
        foreach (var renderer in renderers)
        {
            renderer.GetPropertyBlock(block);
            block.SetFloat("_EchoObjectId", ObjectId(collider));
            renderer.SetPropertyBlock(block);
        }
    }

    public static bool IsObstacle(Collider collider)
    {
        if (!collider.enabled || collider.isTrigger || collider.attachedRigidbody) return false;
        return collider.GetComponentInParent<UnderwaterArena>() || collider.GetComponentInParent<SonarObject>();
    }

    public static Vector3 Reflect(Vector3 incident, Vector3 normal)
    {
        incident.y = normal.y = 0;
        return Vector3.Reflect(incident.normalized, normal.normalized).normalized;
    }

    public static Profile Trace(PhysicsScene scene, Vector3 origin, float range, int reflectionLimit,
        Vector3 direction = default, float halfAngle = 180, Func<Collider, bool> obstacleFilter = null)
    {
        var profile = new Profile { origin = origin, direction = direction.normalized, halfAngle = halfAngle };
        var hits = new RaycastHit[128];
        var known = new Dictionary<Collider, bool>();
        var candidates = new Dictionary<(float obstacle, int direction), Reflection>();
        float cone = Mathf.Cos(halfAngle * Mathf.Deg2Rad);
        bool IsBlocking(Collider collider)
        {
            if (!known.TryGetValue(collider, out bool obstacle))
            {
                obstacle = obstacleFilter != null ? obstacleFilter(collider) : IsObstacle(collider);
                known.Add(collider, obstacle);
                if (obstacle) AssignObjectId(collider);
            }
            return obstacle;
        }
        RaycastHit FirstHit(Vector3 start, Vector3 ray, float length)
        {
            int count = scene.Raycast(start, ray, hits, length, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            while (count == hits.Length && hits.Length < 4096)
            {
                hits = new RaycastHit[hits.Length * 2];
                count = scene.Raycast(start, ray, hits, length, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            }
            RaycastHit result = default;
            float nearest = length + 1;
            for(int h=0;h<count;h++)
                if(IsBlocking(hits[h].collider) && hits[h].distance < nearest)
                { result = hits[h]; nearest = result.distance; }
            return result;
        }
        for (int i = 0; i < Samples; i++)
        {
            float angle = i * Mathf.PI * 2 / Samples;
            Vector3 ray = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            profile.mask[i] = new Color(range + 1, 0, 0, 0);
            if (halfAngle < 180 && Vector3.Dot(ray, profile.direction) < cone) continue;
            RaycastHit first = FirstHit(origin, ray, range);
            if (!first.collider) continue;
            float distance = first.distance;
            float id = ObjectId(first.collider);
            // Store a spatial penetration limit rather than selecting whole mesh faces.
            // Reverse-casting the collider finds its actual thickness along this ray,
            // including rotated/irregular geometry. The near half receives the echo.
            float beyond = distance + first.collider.bounds.size.magnitude + 1;
            float exit = distance + SurfaceBias;
            if (first.collider.Raycast(new Ray(origin + ray * beyond, -ray), out var reverseHit, beyond))
                exit = Mathf.Max(exit, beyond - reverseHit.distance);
            float thickness = exit - distance;
            float limit = distance + thickness * 0.5f;
            float feather = Mathf.Min(0.2f, thickness * 0.25f);
            profile.mask[i] = new Color(distance, id, limit, feather);
            if (reflectionLimit == 0 || distance >= range - 0.5f) continue;
            Vector3 normal = first.normal;
            normal.y = 0;
            if (normal.sqrMagnitude < 0.001f || Vector3.Dot(ray, normal.normalized) > -0.1f) continue;
            Vector3 reflectedDirection = Reflect(ray, normal);
            Vector3 reflectedOrigin = first.point + normal.normalized * 0.08f;
            var next = FirstHit(reflectedOrigin, reflectedDirection, range - distance);
            float targetId = next.collider ? ObjectId(next.collider) : 0;
            float priority = -distance;
            if (next.collider && targetId != id)
            {
                // Prefer actual reflected paths to another object, especially one hidden directly.
                Vector3 direct = next.point - origin;
                var blocker = FirstHit(origin, direct.normalized, direct.magnitude + SurfaceBias);
                bool indirectOnly = blocker.collider && ObjectId(blocker.collider) != targetId;
                priority = (indirectOnly ? 1000 : 100) - distance - next.distance;
            }
            int sector = Mathf.FloorToInt(Mathf.Repeat(Mathf.Atan2(reflectedDirection.z, reflectedDirection.x) / (Mathf.PI * 2), 1) * 16);
            var key = (id, sector);
            if (candidates.TryGetValue(key, out var previous) && previous.priority >= priority) continue;
            candidates[key] = new Reflection
            {
                origin = reflectedOrigin, direction = reflectedDirection, distance = distance,
                targetId = targetId == id ? 0 : targetId, priority = priority
            };
        }
        var sorted = new List<Reflection>(candidates.Values);
        sorted.Sort((a,b) => b.priority.CompareTo(a.priority));
        var selectedTargets = new HashSet<float>();
        // Spend the finite echo budget on different discovered objects before adding extra arcs.
        foreach(var candidate in sorted)
        {
            if(profile.reflections.Count >= reflectionLimit) break;
            if(candidate.targetId == 0 || !selectedTargets.Add(candidate.targetId)) continue;
            profile.reflections.Add(candidate);
        }
        foreach(var candidate in sorted)
        {
            if(profile.reflections.Count >= reflectionLimit) break;
            if(!profile.reflections.Contains(candidate)) profile.reflections.Add(candidate);
        }
        return profile;
    }
}
