using UnityEngine;

namespace Project.Echo.Tuning
{
    [CreateAssetMenu(menuName = "Echo/Settings/Session Connection")]
    public sealed class SessionConnectionSettings : ScriptableObject
    {
        private static SessionConnectionSettings defaultSettings;
        public static SessionConnectionSettings Default =>
            defaultSettings ? defaultSettings : defaultSettings = Resources.Load<SessionConnectionSettings>("Echo/SessionConnection");

        [Tooltip("Aantal seconden dat Echo wacht op het openen van of verbinden met een kamer.")]
        [Min(1f)] public float connectTimeoutSeconds = 30f;
        [Tooltip("Maakt nieuwe kamers vindbaar via openbaar zoeken. Een speler met de kamercode kan ook zonder deze zichtbaarheid aansluiten.")]
        public bool showInPublicLobby = true;
    }
}
