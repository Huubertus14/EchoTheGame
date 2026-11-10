using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GameDev.Foundation
{
    public enum CloudSyncState { Unavailable, Idle, Synced, Conflict, AccountMismatch, Failed }

    public sealed class Foundation
    {
        public ProfileStore Store { get; }
        public PlatformServices Platforms { get; }
        public PlatformIdentity? Identity { get; private set; }
        public CloudSyncState CloudState { get; private set; }
        public string LastError { get; private set; }
        private readonly SemaphoreSlim syncGate = new(1, 1);
        private readonly SemaphoreSlim achievementGate = new(1, 1);
        private readonly string cloudKey;
        private string cloudLock;
        private bool cloudLoaded;
        private PlayerProfile conflict;
        public Foundation(string directory, string cloudKey, PlatformServices platforms)
        { Store = new ProfileStore(directory); this.cloudKey = cloudKey; Platforms = platforms; }

        public async Task InitializeAsync()
        {
            await Platforms.InitializeAsync();
            if (Platforms.Identity != null)
            {
                try
                {
                    Identity = await Platforms.Identity.GetIdentityAsync();
                    if (!Store.Profile.customName && !string.IsNullOrWhiteSpace(Identity.Value.DisplayName))
                        Store.SetName(Identity.Value.DisplayName, false);
                }
                catch (Exception exception) { LastError = exception.Message; }
            }
            CloudState = Platforms.Cloud == null ? CloudSyncState.Unavailable : CloudSyncState.Idle;
        }

        // Never silently replace progressed local data, or upload into another account.
        // UI chooses local/cloud explicitly for conflicts. SDK write locks protect concurrent writes.
        public async Task SyncAsync(bool? useLocalOnConflict = null)
        {
            if (Platforms.Cloud == null) return;
            await syncGate.WaitAsync();
            try
            {
                string owner = Platforms.Cloud.Name + ":" + Platforms.Cloud.AccountId;
                if (!string.IsNullOrEmpty(Store.Profile.cloudOwner) && Store.Profile.cloudOwner != owner)
                { CloudState = CloudSyncState.AccountMismatch; return; }
                if (!cloudLoaded)
                {
                    var remote = await Platforms.Cloud.LoadAsync(cloudKey);
                    cloudLock = remote.WriteLock;
                    if (!string.IsNullOrEmpty(remote.Json))
                    {
                        conflict = JsonUtility.FromJson<PlayerProfile>(remote.Json);
                        ProfileStore.Validate(conflict);
                        if (conflict.cloudOwner != owner) throw new InvalidOperationException("Cloud profile owner differs from the signed-in account.");
                    }
                    cloudLoaded = true;
                }
                if (conflict != null)
                {
                    bool same = conflict.localId == Store.Profile.localId && conflict.revision == Store.Profile.revision;
                    if (!same && !useLocalOnConflict.HasValue && Store.Profile.matchesPlayed > 0)
                    { CloudState = CloudSyncState.Conflict; return; }
                    if (!same && (useLocalOnConflict == false || (!useLocalOnConflict.HasValue && Store.Profile.matchesPlayed == 0)))
                        Store.Replace(conflict);
                    conflict = null;
                }
                Store.Profile.cloudOwner = owner;
                Store.Save();
                cloudLock = await Platforms.Cloud.SaveAsync(cloudKey, JsonUtility.ToJson(Store.Profile), cloudLock);
                CloudState = CloudSyncState.Synced; LastError = null;
            }
            catch (Exception exception)
            {
                cloudLoaded = false; CloudState = CloudSyncState.Failed; LastError = exception.Message;
            }
            finally { syncGate.Release(); }
        }

        public async Task UnlockAchievementAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            if (!Store.Profile.achievements.Contains(id))
            {
                Store.Profile.achievements.Add(id); Store.Profile.pendingAchievements.Add(id); Store.Save();
            }
            await FlushAchievementsAsync();
        }
        public async Task FlushAchievementsAsync()
        {
            if (Platforms.Achievements == null) return;
            await achievementGate.WaitAsync();
            try
            {
                foreach (string id in new List<string>(Store.Profile.pendingAchievements))
                {
                    try
                    {
                        await Platforms.Achievements.UnlockAsync(id);
                        Store.Profile.pendingAchievements.Remove(id); Store.Save();
                    }
                    catch (Exception exception) { LastError = exception.Message; break; }
                }
            }
            finally { achievementGate.Release(); }
        }
    }
}
