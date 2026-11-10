using System;
using System.IO;
using System.Reflection;
using Fusion;
using GameDev.Foundation;
using Project.Echo.Player;
using Project.Echo.Player.Controls.Data;
using Project.Echo.Networking.Handlers;
using Project.Echo.Projectiles.Behaviours;
using Project.Echo.Setting;
using Project.Echo.Setting.Session;
using Project.Echo.Tuning;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using LogType = UnityEngine.LogType;

/// <summary>Local Fusion Single smoke check; never connects to Photon or real platform accounts.</summary>
public static class EchoFoundationPlayModeCheck
{
    private const string Active = "Echo.FoundationSmoke.Active";
    private const string Stage = "Echo.FoundationSmoke.Stage";
    private const string Started = "Echo.FoundationSmoke.Started";
    private static string runtimeError;
    private static bool reloadLocked;
    private static MatchRules testRules;
    public static void Run()
    {
        SessionState.SetBool(Active, true); SessionState.SetInt(Stage, 0);
        SessionState.SetFloat(Started, (float)EditorApplication.timeSinceStartup);
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.isPlaying = true;
        Resume();
    }
    [InitializeOnLoadMethod]
    private static void Resume()
    {
        if (!SessionState.GetBool(Active, false)) return;
        EditorApplication.update -= Tick; EditorApplication.update += Tick;
        Application.logMessageReceived -= OnLog; Application.logMessageReceived += OnLog;
    }
    private static void OnLog(string condition, string trace, LogType type)
    {
        if ((type == LogType.Exception || type == LogType.Error) &&
            (trace.Contains("Assets/Scripts/") || trace.Contains("Assets\\Scripts\\"))) runtimeError = condition + "\n" + trace;
    }
    private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    private static void Check(bool condition, string description)
    { if (!condition) throw new InvalidOperationException(description); }
    private static void Advance(int stage)
    {
        SessionState.SetInt(Stage, stage); SessionState.SetFloat(Started, (float)EditorApplication.timeSinceStartup);
    }
    private static void Tick()
    {
        try
        {
            if (runtimeError != null) throw new InvalidOperationException(runtimeError);
            float elapsed = (float)EditorApplication.timeSinceStartup - SessionState.GetFloat(Started, 0);
            if (elapsed > 45) throw new TimeoutException("Play Mode check stage " + SessionState.GetInt(Stage, 0));
            if (!EditorApplication.isPlaying) return;
            var shell = UnityEngine.Object.FindAnyObjectByType<EchoGameShell>();
            if (shell == null || !Field<bool>(shell, "initialized")) return;
            int stage = SessionState.GetInt(Stage, 0);
            if (stage == 0)
            {
                // Reimports can finish mid-match; keep the isolated services alive until the check exits.
                EditorApplication.LockReloadAssemblies();
                reloadLocked = true;
                // Replace only the validation player's in-memory services with an isolated temporary store.
                var services = new Foundation(Path.GetFullPath("Temp/FoundationSmoke-" + Guid.NewGuid().ToString("N")), "smoke", new PlatformServices());
                services.InitializeAsync().GetAwaiter().GetResult();
                typeof(EchoFoundation).GetField("<Services>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, services);
                shell.ShowSettings(); Check(GamePreferences.MenuOpen, "Settings opens");
                Advance(10);
            }
            else if (stage == 10)
            {
                // TMP initializes its tween runner in Start after this page is first activated.
                if (elapsed < 0.25f) return;
                var dropdown = Field<TMP_Dropdown>(shell, "resolution"); dropdown.Show();
                Check(dropdown.IsExpanded, "Resolution dropdown has a valid template"); dropdown.Hide();
                shell.ShowControls(); shell.Back(); shell.Back();
                Check(Field<int>(shell, "current") == -1 && !GamePreferences.MenuOpen, "Nested settings navigation returns to menu");
                Settings.Session = new SessionSettings("SMOKE2", GameMode.Single);
                Settings.Player = new PlayerSettings { PlayerName = "Smoke Captain" };
                testRules = UnityEngine.Object.Instantiate(MatchRules.FreeForAll);
                var rulesCache = typeof(MatchRules).GetField("freeForAll", BindingFlags.Static | BindingFlags.NonPublic);
                rulesCache.SetValue(null, testRules);
                typeof(MatchRules).GetField("maxPlayers", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(testRules, 4);
                testRules.killLimit = 3;
                testRules.scoreLimit = 100;
                Advance(1); SceneManager.LoadScene("Game");
            }
            else if (stage == 1)
            {
                var coordinator = NetworkedPlayerGameModeController.Coordinator;
                if (!coordinator || !coordinator.Object || !coordinator.Object.IsValid) return;
                Check(coordinator.MaxPlayers == 4, "Lobby uses the configured maximum");
                Check(coordinator.Phase == 0 && !coordinator.AllReady(), "Lobby waits for readiness");
                coordinator.RPC_StartRound(); Check(coordinator.Phase == 0, "Unready host cannot start");
                coordinator.RPC_SetReady(true); coordinator.RPC_StartRound(); Advance(2);
            }
            else if (stage == 2)
            {
                var coordinator = NetworkedPlayerGameModeController.Coordinator;
                if (!coordinator || coordinator.Phase != 2) return;
                shell.Open(1); Check(GamePreferences.InputBlocked, "Pause blocks local input");
                shell.Close(); Check(!GamePreferences.InputBlocked, "Resume restores input");
                var player = PlayerNetworkedController.LocalPlayer;
                Check(player.GetComponent<PlayerHealthController>().PlayerHealth == player.Tuning.maxHealth, "Jager starts with configured HP");
                var runner = player.Runner;
                var testInput = runner.gameObject.AddComponent<NetworkEvents>();
                var inputEvent = typeof(NetworkEvents).GetField(nameof(NetworkEvents.OnInput));
                inputEvent.SetValue(testInput, Activator.CreateInstance(inputEvent.FieldType));
                testInput.OnInput.AddListener((_, input) => input.Set(new NetworkPlayerMovementData { IsShooting = true, IsPing = true }));
                runner.RemoveCallbacks(runner.GetComponent<NetworkEventHandler>());
                runner.AddCallbacks(testInput);
                Advance(20);
            }
            else if (stage == 20)
            {
                if (!UnityEngine.Object.FindAnyObjectByType<HunterTorpedo>() || !UnityEngine.Object.FindAnyObjectByType<WorldSonarPulse>()) return;
                var player = PlayerNetworkedController.LocalPlayer;
                var runner = player.Runner;
                var testInput = runner.GetComponent<NetworkEvents>();
                runner.RemoveCallbacks(testInput);
                runner.AddCallbacks(runner.GetComponent<NetworkEventHandler>());
                UnityEngine.Object.Destroy(testInput);
                var health = player.GetComponent<PlayerHealthController>();
                health.HitPlayer(health.MaxHealth, PlayerRef.None);
                Advance(21);
            }
            else if (stage == 21)
            {
                if (PlayerNetworkedController.LocalPlayer.GetComponent<PlayerHealthController>().Alive) return;
                Advance(22);
            }
            else if (stage == 22)
            {
                var player = PlayerNetworkedController.LocalPlayer;
                var health = player.GetComponent<PlayerHealthController>();
                if (!health.Alive) return;
                Check(health.PlayerHealth == player.Tuning.maxHealth, "Jager respawns with configured HP");
                var score = player.GetComponent<PlayerScoreboardController>();
                score.AddScore(100); score.AddKills(2); Advance(3);
            }
            else if (stage == 3)
            {
                var coordinator = NetworkedPlayerGameModeController.Coordinator;
                if (!coordinator || coordinator.Phase != 3 || EchoFoundation.Services.Store.Profile.matchesPlayed == 0) return;
                Check(GamePreferences.InputBlocked && Field<int>(shell, "current") == 4, "Match ends on results page");
                Check(EchoFoundation.Services.Store.Profile.matchesPlayed == 1 && EchoFoundation.Services.Store.Profile.totalExperience == 120, "Solo result awards completion/kill XP once without a win bonus");
                Advance(4);
            }
            else if (stage == 4)
            {
                if (elapsed < 1) return;
                Check(EchoFoundation.Services.Store.Profile.matchesPlayed == 1, "Results refresh does not duplicate XP");
                NetworkedPlayerGameModeController.Coordinator.RPC_ReturnToLobby(); Advance(5);
            }
            else if (stage == 5)
            {
                var coordinator = NetworkedPlayerGameModeController.Coordinator;
                if (!coordinator || coordinator.Phase != 0) return;
                var player = PlayerNetworkedController.LocalPlayer;
                var health = player.GetComponent<PlayerHealthController>();
                Check(player.GetComponent<PlayerScoreboardController>().Kills == 0 && !coordinator.Ready &&
                    health.PlayerHealth == health.MaxHealth, "Rematch resets score, readiness and health");
                testRules.countdownSeconds = 0f;
                testRules.timeLimitSeconds = 0.5f;
                testRules.killLimit = testRules.scoreLimit = 0;
                coordinator.RPC_SetReady(true);
                coordinator.RPC_StartRound();
                Advance(6);
            }
            else if (stage == 6)
            {
                var coordinator = NetworkedPlayerGameModeController.Coordinator;
                if (!coordinator || coordinator.Phase != 3) return;
                Check(coordinator.SecondsLeft == 0f, "Round ends at the configured time limit");
                SessionState.SetBool(Active, false); EditorApplication.update -= Tick;
                File.WriteAllText("../docs/foundation-playmode-verification.txt", "PASS: settings/dropdown/nested navigation; configured lobby capacity, readiness and countdown; pause/resume; Jager torpedo/sonar input and configured HP respawn; score and time limits; single XP award; rematch reset.\nThis does not verify two-client networking or native/cloud services.\n");
                Debug.Log("ECHO_FOUNDATION_PLAYMODE_OK");
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += () => Finish(0);
            }
        }
        catch (Exception exception)
        {
            SessionState.SetBool(Active, false); EditorApplication.update -= Tick;
            Debug.LogException(exception);
            File.WriteAllText("../docs/foundation-playmode-verification.txt", "FAIL: " + exception);
            Finish(1);
        }
    }

    private static void Finish(int exitCode)
    {
        if (reloadLocked)
        {
            reloadLocked = false;
            EditorApplication.UnlockReloadAssemblies();
        }
        EditorApplication.Exit(exitCode);
    }
}
