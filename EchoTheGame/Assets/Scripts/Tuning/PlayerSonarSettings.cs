using UnityEngine;

namespace Project.Echo.Tuning
{
    [CreateAssetMenu(menuName = "Echo/Settings/Player Sonar")]
    public sealed class PlayerSonarSettings : ScriptableObject
    {
        [Header("Active scan")]
        [Tooltip("Minimale tijd in seconden tussen twee handmatige sonarpings.")]
        [Min(0.1f)] public float cooldown = 6f;
        [Tooltip("Aantal seconden waarin onderzeeërs binnen bereik zichtbaar worden voor degene die scant. Bij 0 onthult de scan geen onderzeeërs.")]
        [Min(0f)] public float contactRevealDuration = 3f;
        [Tooltip("Bereik, timing, weergave en reflecties van een handmatig geactiveerde sonarping.")]
        public WorldSonarPulse.Settings pulse = new() { range = 32f, duration = 0.8f };

        [Header("Collision pulse")]
        [Tooltip("Minimale tijd in seconden tussen twee sonarpulsen door botsingen.")]
        [Min(0.1f)] public float collisionCooldown = 0.65f;
        [Tooltip("Afstand in Unity-eenheden van het botsingspunt tot de oorsprong van de puls, van het obstakel af.")]
        [Min(0f)] public float collisionSurfaceOffset = 0.08f;
        [Tooltip("Bereik, timing, weergave en reflecties van de automatische puls bij een botsing.")]
        public WorldSonarPulse.Settings collisionPulse = new() { range = 9f, duration = 0.6f };
    }
}
