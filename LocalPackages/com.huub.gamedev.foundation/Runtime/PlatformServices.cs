using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameDev.Foundation
{
    [Flags]
    public enum PlatformFeatures { None = 0, Identity = 1, CloudSave = 2, Achievements = 4 }

    public readonly struct PlatformIdentity
    {
        public readonly string Provider, UserId, DisplayName;
        public PlatformIdentity(string provider, string userId, string displayName)
        { Provider = provider; UserId = userId; DisplayName = displayName; }
    }

    // Register adapters before Foundation.InitializeAsync. Each service is selected independently:
    // native identity/achievements can coexist with Unity Cloud Save.
    public interface IPlatformAdapter
    {
        string Name { get; }
        int Priority { get; }
        PlatformFeatures Features { get; }
        bool IsAvailable { get; }
        Task InitializeAsync();
    }
    public interface IIdentityAdapter : IPlatformAdapter
    {
        Task<PlatformIdentity> GetIdentityAsync();
    }
    public readonly struct CloudDocument
    {
        public readonly string Json, WriteLock;
        public CloudDocument(string json, string writeLock) { Json = json; WriteLock = writeLock; }
    }
    public interface ICloudSaveAdapter : IPlatformAdapter
    {
        string AccountId { get; }
        Task<CloudDocument> LoadAsync(string key);
        Task<string> SaveAsync(string key, string json, string writeLock);
    }
    public interface IAchievementAdapter : IPlatformAdapter
    {
        Task UnlockAsync(string achievementId);
    }

    public sealed class PlatformServices
    {
        private readonly List<IPlatformAdapter> adapters = new();
        private readonly HashSet<IPlatformAdapter> initialized = new();
        public IIdentityAdapter Identity { get; private set; }
        public ICloudSaveAdapter Cloud { get; private set; }
        public IAchievementAdapter Achievements { get; private set; }
        public readonly List<string> Diagnostics = new();
        public void Register(IPlatformAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            adapters.Add(adapter);
        }
        public async Task InitializeAsync()
        {
            Identity = null; Cloud = null; Achievements = null; Diagnostics.Clear();
            adapters.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            foreach (var adapter in adapters)
            {
                if (!adapter.IsAvailable) continue;
                try
                {
                    if (!initialized.Contains(adapter))
                    {
                        await adapter.InitializeAsync();
                        initialized.Add(adapter);
                    }
                    if (Identity == null && (adapter.Features & PlatformFeatures.Identity) != 0)
                        Identity = adapter as IIdentityAdapter;
                    if (Cloud == null && (adapter.Features & PlatformFeatures.CloudSave) != 0)
                        Cloud = adapter as ICloudSaveAdapter;
                    if (Achievements == null && (adapter.Features & PlatformFeatures.Achievements) != 0)
                        Achievements = adapter as IAchievementAdapter;
                }
                catch (Exception exception) { Diagnostics.Add(adapter.Name + ": " + exception.Message); }
            }
        }
    }
}
