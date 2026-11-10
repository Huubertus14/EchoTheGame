using UnityEngine;

namespace GameDev.Steam
{
    [CreateAssetMenu(menuName = "Game Dev/Steam Settings")]
    public sealed class SteamPlatformSettings : ScriptableObject
    {
        public bool enabledForThisGame;
        public uint appId;
        public bool enableInEditor;
    }
}
