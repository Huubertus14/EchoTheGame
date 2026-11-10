using System;
using System.Threading;
using System.Threading.Tasks;
using GameDev.Foundation;
using Steamworks;
using UnityEngine;

namespace GameDev.Steam
{
    public sealed class SteamAdapter : IIdentityAdapter, IAchievementAdapter
    {
        private readonly SteamPlatformSettings settings;
        private SteamCallbackPump pump;
        private readonly SemaphoreSlim achievementGate = new(1, 1);
        public SteamAdapter(SteamPlatformSettings settings) => this.settings = settings;
        public string Name => "Steam";
        public int Priority => 100;
        public PlatformFeatures Features => PlatformFeatures.Identity | PlatformFeatures.Achievements;
        public bool IsAvailable => settings && settings.enabledForThisGame && settings.appId > 0 &&
            (!Application.isEditor || settings.enableInEditor) && !Application.isMobilePlatform;
        public Task InitializeAsync()
        {
            if (!SteamAPI.Init()) throw new InvalidOperationException("Steam is unavailable. Start Steam and check the App ID.");
            if (SteamUtils.GetAppID().m_AppId != settings.appId)
            {
                SteamAPI.Shutdown(); throw new InvalidOperationException("The active Steam App ID differs from the configured game.");
            }
            var root = new GameObject("Game Dev Steam Callbacks");
            UnityEngine.Object.DontDestroyOnLoad(root); pump = root.AddComponent<SteamCallbackPump>();
            return Task.CompletedTask;
        }
        public Task<PlatformIdentity> GetIdentityAsync() => Task.FromResult(new PlatformIdentity(Name,
            SteamUser.GetSteamID().m_SteamID.ToString(), SteamFriends.GetPersonaName()));
        public async Task UnlockAsync(string achievementId)
        {
            await achievementGate.WaitAsync();
            try
            {
                if (!SteamUserStats.SetAchievement(achievementId)) throw new InvalidOperationException("Steam achievement not defined: " + achievementId);
                var stored = new TaskCompletionSource<bool>();
                using var callback = Callback<UserStatsStored_t>.Create(result =>
                {
                    if (result.m_nGameID != settings.appId) return;
                    if (result.m_eResult == EResult.k_EResultOK) stored.TrySetResult(true);
                    else stored.TrySetException(new InvalidOperationException("Steam StoreStats: " + result.m_eResult));
                });
                if (!SteamUserStats.StoreStats()) throw new InvalidOperationException("Steam could not store achievements.");
                if (await Task.WhenAny(stored.Task, Task.Delay(15000)) != stored.Task) throw new TimeoutException("Steam achievement sync timed out.");
                await stored.Task;
            }
            finally { achievementGate.Release(); }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            var config = Resources.Load<SteamPlatformSettings>("GameDevSteamSettings");
            if (config) PlatformAdapterRegistry.Register(() => new SteamAdapter(config));
        }
    }
}
