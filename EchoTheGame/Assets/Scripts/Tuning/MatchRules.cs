using UnityEngine;

namespace Project.Echo.Tuning
{
    [CreateAssetMenu(menuName = "Echo/Settings/Match Rules")]
    public sealed class MatchRules : ScriptableObject
    {
        // The current lobby and results layouts have eight player slots.
        public const int SupportedPlayerCount = 8;
        private static MatchRules freeForAll;
        public static MatchRules FreeForAll =>
            freeForAll ? freeForAll : freeForAll = Resources.Load<MatchRules>("Echo/FreeForAll");

        [Header("Lobby")]
        [Tooltip("Maximaal aantal spelers in een kamer, inclusief de host. Geldt bij het openen van een nieuwe kamer.")]
        [SerializeField, Range(1, SupportedPlayerCount)] private int maxPlayers = SupportedPlayerCount;
        [Tooltip("Minimum aantal verbonden spelers om de ronde te starten. Alle spelers moeten gereed zijn.")]
        [SerializeField, Range(1, SupportedPlayerCount)] private int minPlayers = 1;
        [Tooltip("Aantal seconden tussen het starten door de host en het begin van de ronde. Bij 0 is er geen zichtbare aftelling.")]
        [Min(0f)] public float countdownSeconds = 5f;

        [Header("Round")]
        [Tooltip("Maximale speeltijd per ronde in seconden. Bij 0 is er geen tijdlimiet.")]
        [Min(0f)] public float timeLimitSeconds;
        [Tooltip("Aantal kills waarmee één speler de ronde beëindigt. Bij 0 staat deze limiet uit.")]
        [Min(0)] public int killLimit = 2;
        [Tooltip("Aantal punten waarmee één speler de ronde beëindigt. Bij 0 staat deze limiet uit.")]
        [Min(0)] public int scoreLimit;
        [Tooltip("Aantal wedstrijdpunten per kill. Dit staat los van de XP-beloning.")]
        [Min(0)] public int pointsPerKill = 50;

        public int MaxPlayers => Mathf.Clamp(maxPlayers, 1, SupportedPlayerCount);
        public int MinPlayers => Mathf.Clamp(minPlayers, 1, MaxPlayers);
        public bool HasReachedLimit(int score, int kills) =>
            (scoreLimit > 0 && score >= scoreLimit) || (killLimit > 0 && kills >= killLimit);
    }
}
