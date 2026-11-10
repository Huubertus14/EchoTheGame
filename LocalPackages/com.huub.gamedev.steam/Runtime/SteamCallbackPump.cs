using Steamworks;
using UnityEngine;

namespace GameDev.Steam
{
    // This adapter owns SteamAPI. Do not also install a second SteamManager.
    public sealed class SteamCallbackPump : MonoBehaviour
    {
        private void Update() => SteamAPI.RunCallbacks();
        private void OnDestroy() => SteamAPI.Shutdown();
    }
}
