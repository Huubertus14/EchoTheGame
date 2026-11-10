using UnityEngine;

namespace Project.Echo.Tuning
{
    [CreateAssetMenu(menuName = "Echo/Settings/Impact Sonar")]
    public sealed class ImpactSonarSettings : ScriptableObject
    {
        private static ImpactSonarSettings defaultSettings;
        public static ImpactSonarSettings Default =>
            defaultSettings ? defaultSettings : defaultSettings = Resources.Load<ImpactSonarSettings>("Echo/ImpactSonar");

        [Tooltip("Bereik, timing, weergave en reflecties van de sonarpuls die bij een inslag ontstaat.")]
        public WorldSonarPulse.Settings pulse = new() { range = 9f, duration = 0.6f };
        [Tooltip("Aantal seconden voordat het netwerkobject van de inslag verdwijnt. De sonarpuls heeft zijn eigen duur.")]
        [Min(0.01f)] public float emitterLifetime = 0.6f;
        [Tooltip("Afstand in Unity-eenheden waarmee de oorsprong van de puls van het geraakte oppervlak af ligt.")]
        [Min(0f)] public float surfaceOffset = 0.08f;
    }
}
