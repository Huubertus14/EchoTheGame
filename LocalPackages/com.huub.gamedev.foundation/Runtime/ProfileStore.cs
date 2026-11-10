using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GameDev.Foundation
{
    [Serializable]
    public sealed class MatchRecord
    {
        public string id;
        public string completedUtc;
        public int score, kills, deaths, experience;
        public bool won;
    }

    [Serializable]
    public sealed class PlayerProfile
    {
        public int schemaVersion = 1;
        public string localId = Guid.NewGuid().ToString("N");
        public string displayName = "Captain";
        public bool customName;
        public string cloudOwner;
        public long revision;
        public string updatedUtc;
        public int totalExperience, matchesPlayed, wins, kills, deaths;
        public List<string> completedMatches = new();
        public List<string> achievements = new();
        public List<string> pendingAchievements = new();
        public List<MatchRecord> recentMatches = new();
    }

    public sealed class ProfileStore
    {
        private readonly string path;
        public PlayerProfile Profile { get; private set; }
        public event Action Changed;
        public string Warning { get; private set; }
        public ProfileStore(string directory)
        {
            Directory.CreateDirectory(directory);
            path = Path.Combine(directory, "profile.json");
            Profile = Read(path) ?? Read(path + ".bak") ?? new PlayerProfile();
        }
        private PlayerProfile Read(string file)
        {
            if (!File.Exists(file)) return null;
            try
            {
                var profile = JsonUtility.FromJson<PlayerProfile>(File.ReadAllText(file));
                Validate(profile);
                return profile;
            }
            catch (Exception exception) { Warning = "Profile recovery: " + exception.Message; return null; }
        }
        public static void Validate(PlayerProfile profile)
        {
            if (profile == null || profile.schemaVersion != 1 || string.IsNullOrEmpty(profile.localId) ||
                profile.totalExperience < 0 || profile.matchesPlayed < 0)
                throw new InvalidDataException("Unsupported or invalid profile.");
            profile.completedMatches ??= new(); profile.achievements ??= new();
            profile.pendingAchievements ??= new(); profile.recentMatches ??= new();
        }
        public void SetName(string name, bool custom = true)
        {
            name = CleanName(name);
            if (Profile.displayName == name && Profile.customName == custom) return;
            Profile.displayName = name; Profile.customName = custom; Save();
        }
        public static string CleanName(string name)
        {
            var cleaned = new string((name ?? "").Where(c => !char.IsControl(c) && c != '<' && c != '>').ToArray()).Trim();
            return string.IsNullOrEmpty(cleaned) ? "Captain" : cleaned.Substring(0, Math.Min(24, cleaned.Length));
        }
        // Match IDs persist for idempotency even when old result details are pruned.
        public bool RecordMatch(MatchRecord result, ProgressionRules rules)
        {
            if (result == null || string.IsNullOrEmpty(result.id)) throw new ArgumentException("A match ID is required.");
            if (Profile.completedMatches.Contains(result.id)) return false;
            result.kills = Math.Max(0, result.kills); result.deaths = Math.Max(0, result.deaths);
            result.experience = rules.Award(result.won, result.kills);
            result.completedUtc = DateTime.UtcNow.ToString("O");
            Profile.totalExperience = Add(Profile.totalExperience, result.experience);
            Profile.matchesPlayed = Add(Profile.matchesPlayed, 1);
            Profile.wins = Add(Profile.wins, result.won ? 1 : 0);
            Profile.kills = Add(Profile.kills, result.kills); Profile.deaths = Add(Profile.deaths, result.deaths);
            Profile.completedMatches.Add(result.id); Profile.recentMatches.Insert(0, result);
            if (Profile.recentMatches.Count > 20) Profile.recentMatches.RemoveRange(20, Profile.recentMatches.Count - 20);
            Save(); return true;
        }
        private static int Add(int a, int b) => (int)Math.Min(int.MaxValue, (long)a + b);
        public void Replace(PlayerProfile profile) { Validate(profile); Profile = profile; Save(); }
        public void Save()
        {
            Profile.revision++;
            Profile.updatedUtc = DateTime.UtcNow.ToString("O");
            string temp = path + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(Profile, true));
            if (File.Exists(path))
            {
                // Never rotate an unreadable original over the valid recovery copy.
                if (Read(path) != null) File.Copy(path, path + ".bak", true);
                File.Delete(path);
            }
            File.Move(temp, path);
            Changed?.Invoke();
        }
    }
}
