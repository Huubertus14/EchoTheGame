using System;
using System.IO;
using System.Threading.Tasks;
using GameDev.Foundation;
using UnityEngine;

namespace Project.Echo.Setting
{
    public static class EchoFoundation
    {
        public static Foundation Services { get; private set; }
        public static ProgressionRules Rules { get; private set; }
        public static string ConnectionMessage { get; set; }
        public static bool IsConnecting { get; set; }
        public static string MatchId { get; set; }
        public static event Action<PlatformServices> ConfigurePlatforms;
        private static Task initialization;
        public static Task OnlineInitialization { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Services = null; Rules = null; initialization = null; ConfigurePlatforms = null;
            OnlineInitialization = null; MatchId = null;
            IsConnecting = false; ConnectionMessage = null; GamePreferences.InputBlocked = false;
            GamePreferences.MenuOpen = false;
            GamePreferences.MenuInputFrame = -1;
        }
        public static Task InitializeAsync() => initialization ??= InitializeInternalAsync();
        private static Task InitializeInternalAsync()
        {
            Rules = Resources.Load<ProgressionRules>("Echo/EchoProgression");
            if (!Rules) Rules = ScriptableObject.CreateInstance<ProgressionRules>();
            var platforms = new PlatformServices();
            PlatformAdapterRegistry.Install(platforms);
            ConfigurePlatforms?.Invoke(platforms);
            // Separate editor clones from the primary editor's save and Authentication profile.
            string suffix = Application.isEditor ? "editor-" + Path.GetFileName(Path.GetDirectoryName(Application.dataPath)) : "player";
            Services = new Foundation(Path.Combine(Application.persistentDataPath, "Foundation", suffix), "echo_profile_v1", platforms);
            GamePreferences.Apply();
            OnlineInitialization = InitializeOnlineAsync(Services);
            return Task.CompletedTask;
        }
        private static async Task InitializeOnlineAsync(Foundation services)
        {
            try
            {
                await services.InitializeAsync();
                await services.SyncAsync();
                await services.FlushAchievementsAsync();
            }
            catch (Exception exception) { Debug.LogWarning("Online services unavailable: " + exception.Message); }
        }
    }
}
