using UnityEngine;

namespace Project.Echo.Tuning
{
    [CreateAssetMenu(menuName = "Echo/Settings/Arena")]
    public sealed class ArenaSettings : ScriptableObject
    {
        private static ArenaSettings defaultSettings;
        public static ArenaSettings Default =>
            defaultSettings ? defaultSettings : defaultSettings = Resources.Load<ArenaSettings>("Echo/Arena");

        [Header("Collision and spawn")]
        [Tooltip("Straal in Unity-eenheden waarmee de onderzeeër tegen de kust en obstakels botst.")]
        [Min(0.01f)] public float playerRadius = 0.85f;
        [Tooltip("Afstand in Unity-eenheden die de onderzeeër vrijhoudt van een geraakt obstakel.")]
        [Min(0f)] public float collisionClearance = 0.02f;
        [Tooltip("Vrije ruimte in Unity-eenheden rond een spawnpunt. De straal van de onderzeeër geldt als minimum.")]
        [Min(0.01f)] public float spawnClearance = 2f;
        [Tooltip("Gewenste afstand in Unity-eenheden tot de dichtstbijzijnde speler. Grotere afstanden tellen niet extra mee bij de keuze.")]
        [Min(0f)] public float preferredSpawnSeparation = 30f;
        [Tooltip("Hoe zwaar de tijd sinds het laatste gebruik meetelt bij de keuze van een spawnpunt. Bij 0 telt alleen afstand mee.")]
        [Min(0f)] public float spawnReuseWeight = 0.05f;
        [Header("Camera")]
        [Tooltip("Positie van de camera ten opzichte van de onderzeeër, in Unity-eenheden langs de wereldassen.")]
        public Vector3 cameraOffset = new Vector3(0f, 30f, 0f);
        [Tooltip("Verticale kijkhoek van de camera in graden. Een grotere hoek toont meer van de arena.")]
        [Range(1f, 179f)] public float cameraFieldOfView = 60f;
    }
}
