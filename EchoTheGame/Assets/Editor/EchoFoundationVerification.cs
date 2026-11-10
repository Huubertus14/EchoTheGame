using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GameDev.Foundation;
using UnityEngine;

public static class EchoFoundationVerification
{
    private static int checks;
    public static void Run()
    {
        checks = 0;
        string root = Path.GetFullPath(Path.Combine("Temp", "FoundationTests-" + Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(root);
        var rules = ScriptableObject.CreateInstance<ProgressionRules>();
        try
        {
            var store = new ProfileStore(Path.Combine(root, "local"));
            store.SetName("  <Captain>\n  ");
            Check(store.Profile.displayName == "Captain", "Names strip markup and controls");
            var match = new MatchRecord { id = "round-1", won = true, kills = 2, deaths = 1 };
            Check(store.RecordMatch(match, rules), "Completed match is recorded");
            Check(!store.RecordMatch(match, rules), "Duplicate round does not award XP");
            Check(store.Profile.totalExperience == 170 && store.Profile.matchesPlayed == 1 && store.Profile.wins == 1, "XP and stats agree");
            Check(rules.Level(1000) == 2 && rules.Progress(1500) == 0.5f, "Level threshold and progress");
            var reloaded = new ProfileStore(Path.Combine(root, "local"));
            Check(reloaded.Profile.totalExperience == 170 && reloaded.Profile.completedMatches.Contains("round-1"), "Stats and idempotency survive restart");
            store.Save();
            File.WriteAllText(Path.Combine(root, "local", "profile.json"), "broken json");
            var recovered = new ProfileStore(Path.Combine(root, "local"));
            Check(recovered.Profile.totalExperience == 170 && recovered.Warning != null, "Corrupt primary recovers from backup");

            var platforms = new PlatformServices();
            var failed = new FailedIdentity(); var fallback = new FakeIdentity();
            platforms.Register(failed); platforms.Register(fallback);
            platforms.InitializeAsync().GetAwaiter().GetResult();
            Check(platforms.Identity == fallback && platforms.Diagnostics.Count == 1, "Failed provider falls back and reports reason");

            var cloud = new FakeCloud(); platforms = new PlatformServices(); platforms.Register(cloud);
            var foundation = new Foundation(Path.Combine(root, "cloud"), "test", platforms);
            foundation.InitializeAsync().GetAwaiter().GetResult();
            foundation.Store.RecordMatch(new MatchRecord { id = "local-round" }, rules);
            cloud.Document = new CloudDocument(JsonUtility.ToJson(new PlayerProfile { cloudOwner = "TestCloud:account", matchesPlayed = 1, totalExperience = 900 }), "v1");
            foundation.SyncAsync().GetAwaiter().GetResult();
            Check(foundation.CloudState == CloudSyncState.Conflict && cloud.Writes == 0, "Conflicting progress never uploads silently");
            foundation.SyncAsync(false).GetAwaiter().GetResult();
            Check(foundation.CloudState == CloudSyncState.Synced && foundation.Store.Profile.totalExperience == 900 && cloud.ReceivedLock == "v1", "Explicit cloud choice and write lock");
            cloud.Account = "other-account";
            foundation.SyncAsync().GetAwaiter().GetResult();
            Check(foundation.CloudState == CloudSyncState.AccountMismatch && cloud.Writes == 1, "Another account cannot overwrite cloud progress");

            var achievements = new FakeAchievements(); platforms = new PlatformServices(); platforms.Register(achievements);
            foundation = new Foundation(Path.Combine(root, "achievements"), "test", platforms);
            foundation.InitializeAsync().GetAwaiter().GetResult();
            foundation.UnlockAchievementAsync("test.first").GetAwaiter().GetResult();
            Check(foundation.Store.Profile.pendingAchievements.Count == 1, "Failed native achievement remains queued");
            achievements.Fail = false;
            foundation.FlushAchievementsAsync().GetAwaiter().GetResult();
            foundation.UnlockAchievementAsync("test.first").GetAwaiter().GetResult();
            Check(achievements.Successes == 1 && foundation.Store.Profile.achievements.Count == 1, "Queued achievement retries once and remains idempotent");
            Debug.Log($"ECHO_FOUNDATION_TESTS_OK: {checks} checks");
            File.WriteAllText("../docs/foundation-verification.txt", $"{checks} foundation checks passed.\nLocal save/recovery, duplicate XP, level thresholds, provider fallback, cloud conflicts/write locks/account isolation and achievement retries.\n");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(rules);
            string allowed = Path.GetFullPath("Temp") + Path.DirectorySeparatorChar;
            if (root.StartsWith(allowed, StringComparison.OrdinalIgnoreCase) && Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
    private static void Check(bool condition, string description)
    { if (!condition) throw new InvalidOperationException("Foundation check failed: " + description); checks++; }
    private class FakeIdentity : IIdentityAdapter
    {
        public string Name => "Fallback";
        public virtual int Priority => 1;
        public PlatformFeatures Features => PlatformFeatures.Identity;
        public bool IsAvailable => true;
        public virtual Task InitializeAsync() => Task.CompletedTask;
        public Task<PlatformIdentity> GetIdentityAsync() => Task.FromResult(new PlatformIdentity(Name, "id", "Captain"));
    }
    private sealed class FailedIdentity : FakeIdentity
    {
        public override int Priority => 100;
        public override Task InitializeAsync() => Task.FromException(new InvalidOperationException("Unavailable"));
    }
    private sealed class FakeCloud : ICloudSaveAdapter
    {
        public string Name => "TestCloud";
        public string Account = "account";
        public string AccountId => Account;
        public int Priority => 1;
        public PlatformFeatures Features => PlatformFeatures.CloudSave;
        public bool IsAvailable => true;
        public CloudDocument Document;
        public int Writes;
        public string ReceivedLock;
        public Task InitializeAsync() => Task.CompletedTask;
        public Task<CloudDocument> LoadAsync(string key) => Task.FromResult(Document);
        public Task<string> SaveAsync(string key, string json, string writeLock)
        { Writes++; ReceivedLock = writeLock; return Task.FromResult("v2"); }
    }
    private sealed class FakeAchievements : IAchievementAdapter
    {
        public string Name => "NativeAchievements";
        public int Priority => 1;
        public PlatformFeatures Features => PlatformFeatures.Achievements;
        public bool IsAvailable => true;
        public bool Fail = true;
        public int Successes;
        public Task InitializeAsync() => Task.CompletedTask;
        public Task UnlockAsync(string id)
        {
            if (Fail) return Task.FromException(new InvalidOperationException("Offline"));
            Successes++; return Task.CompletedTask;
        }
    }
}
