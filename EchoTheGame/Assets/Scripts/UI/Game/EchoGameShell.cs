using System;
using System.Collections.Generic;
using System.Linq;
using GameDev.Foundation;
using Project.Echo.Localization;
using Project.Echo.Networking;
using Project.Echo.Player;
using Project.Echo.Setting;
using Project.Echo.Setting.Session;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EchoGameShell : MonoBehaviour
{
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject[] pages;
    [SerializeField] private Selectable[] firstControls;
    [SerializeField] private TMP_InputField playerName, roomCode, profileName;
    [SerializeField] private TMP_Text connectionMessage, lobbyText, lobbyStatus, resultsText, resultProgress, profileText, cloudStatus;
    [SerializeField] private TMP_Text[] resultNames, resultScores, resultKillsDeaths;
    [SerializeField] private Button readyButton, startButton, rematchButton, localCloudButton, remoteCloudButton;
    [SerializeField] private TMP_Dropdown screenMode, resolution, quality, frameRate, vsync, language;
    [SerializeField] private Slider masterVolume, musicVolume, effectsVolume;
    [SerializeField] private Button[] bindingButtons;
    private readonly List<PlayerScoreboardController> players = new();
    private readonly List<Vector2Int> resolutions = new();
    private readonly int[] frameRates = { 30, 60, 90, 120, 144, 240, -1 };
    private int current = -1, binding = -1;
    private readonly Stack<int> history = new();
    private int previousPhase = -1;
    private bool initialized, leaving;
    private float nextRefresh;
    private GameObject previousSelection;
    private string recordedRound;
    private string suggestedName;
    private bool IsGame => SceneManager.GetActiveScene().name == "Game";
    private static string T(string nl, string en) => EchoLocalization.IsReady &&
        UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code.StartsWith("en") ? en : nl;

    private async void Start()
    {
        overlay.SetActive(false);
        try
        {
            await EchoFoundation.InitializeAsync();
            if (!this) return;
            initialized = true;
            if (playerName) playerName.SetTextWithoutNotify(EchoFoundation.Services.Store.Profile.displayName);
            suggestedName = EchoFoundation.Services.Store.Profile.displayName;
            EchoFoundation.Services.Store.Changed += RefreshSuggestedName;
            if (connectionMessage) connectionMessage.text = ConnectionError(EchoFoundation.ConnectionMessage);
            if (!IsGame && !string.IsNullOrEmpty(EchoFoundation.ConnectionMessage))
            {
                await System.Threading.Tasks.Task.Yield();
                if (!this) return;
                FindAnyObjectByType<Project.Echo.Menu.MenuController>()?.ShowPlay();
            }
            EchoLocalization.Changed += RefreshLanguage;
            AudioPreferences.Changed += RefreshAudio;
            RefreshAudio(); RefreshBindings();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            if (connectionMessage) connectionMessage.text = T("Profiel kon niet worden geladen.", "Could not load profile.");
        }
    }
    private void Update()
    {
        if (!initialized || leaving) return;
        if (IsGame && EchoFoundation.IsConnecting && Input.GetKeyDown(KeyCode.Escape)) { Leave(); return; }
        if (binding >= 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) { binding = -1; RefreshBindings(); return; }
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                if (Input.GetKeyDown(key) && GamePreferences.Bind(binding, key)) { binding = -1; RefreshBindings(); break; }
            return;
        }
        bool cancel = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1);
        bool pause = Input.GetKeyDown(KeyCode.JoystickButton7);
        if (cancel || pause)
        {
            GamePreferences.MenuInputFrame = Time.frameCount;
            var expanded = GetComponentsInChildren<TMP_Dropdown>().FirstOrDefault(d => d.IsExpanded);
            if (expanded) { expanded.Hide(); return; }
            if (current == 2 || current == 3 || current == 5 || current == 6) Back();
            else if (current == 1) Close();
            else if (current < 0 && IsGame) Open(1);
        }
        if (!IsGame || Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + 0.1f;
        var coordinator = NetworkedPlayerGameModeController.Coordinator;
        if (coordinator == null || !coordinator.Object || !coordinator.Object.IsValid) return;
        int phase = coordinator.Phase;
        if (phase != previousPhase)
        {
            previousPhase = phase;
            history.Clear();
            if (phase <= 1) Open(0);
            else if (phase == 2) Close();
            else if (phase == 3) { RecordResult(coordinator.RoundId.ToString()); Open(4); }
        }
        if (current == 0) RefreshLobby(coordinator);
        if (current == 4) RefreshResults();
    }
    private void RefreshLanguage()
    {
        RefreshBindings(); RefreshAudio();
        if (current == 5) RefreshProfile();
    }
    private void RefreshSuggestedName()
    {
        string name = EchoFoundation.Services.Store.Profile.displayName;
        if (playerName && !playerName.isFocused && (playerName.text == suggestedName || string.IsNullOrWhiteSpace(playerName.text)))
            playerName.SetTextWithoutNotify(name);
        suggestedName = name;
    }
    public void Host() => Launch(true);
    public void Join() => Launch(false);
    private async void Launch(bool host)
    {
        if (!initialized || EchoFoundation.IsConnecting) return;
        try { await SessionLauncher.LaunchAsync(host, roomCode.text, playerName.text); }
        catch (Exception exception)
        {
            Debug.LogWarning(exception.Message);
            if (connectionMessage) connectionMessage.text = exception.Message == "ROOM_CODE_INVALID"
                ? T("Vul een geldige kamercode van 6 tekens in.", "Enter a valid 6-character room code.")
                : T("Verbinden mislukt. Probeer opnieuw.", "Connection failed. Please retry.");
        }
    }
    private static string ConnectionError(string reason)
    {
        if (string.IsNullOrEmpty(reason)) return "";
        if (reason == "NO_OPEN_LOBBY") return T("Geen open lobby gevonden.", "No open lobby found.");
        if (reason.IndexOf("Full", StringComparison.OrdinalIgnoreCase) >= 0) return T("Deze kamer is vol.", "This room is full.");
        if (reason.IndexOf("NotFound", StringComparison.OrdinalIgnoreCase) >= 0) return T("Kamer niet gevonden. Controleer code en regio.", "Room not found. Check the code and region.");
        if (reason.IndexOf("Closed", StringComparison.OrdinalIgnoreCase) >= 0) return T("Deze wedstrijd is al begonnen.", "This match has already started.");
        return T("Verbinding verbroken of mislukt. Probeer opnieuw. ", "Connection lost or failed. Please retry. ") + reason;
    }
    public void Open(int page)
    {
        if (current < 0) previousSelection = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
        current = page;
        overlay.SetActive(true);
        for (int i = 0; i < pages.Length; i++) pages[i].SetActive(i == page);
        GamePreferences.InputBlocked = IsGame;
        GamePreferences.MenuOpen = true;
        if (firstControls[page]) EventSystem.current?.SetSelectedGameObject(firstControls[page].gameObject);
    }
    public void Close()
    {
        current = -1; binding = -1; overlay.SetActive(false); GamePreferences.InputBlocked = false;
        GamePreferences.MenuOpen = false; history.Clear();
        if (previousSelection && previousSelection.activeInHierarchy) EventSystem.current?.SetSelectedGameObject(previousSelection);
    }
    private void NavigateTo(int page) { history.Push(current); Open(page); }
    public void ShowSettings() { NavigateTo(2); PopulateGraphics(); }
    public void ShowControls() { NavigateTo(3); RefreshBindings(); }
    public void ShowProfile() { NavigateTo(5); RefreshProfile(); }
    public void AskLeave() { NavigateTo(6); }
    public void Back()
    {
        binding = -1;
        int previous = history.Count > 0 ? history.Pop() : -1;
        if (previous >= 0) Open(previous); else Close();
    }
    public async void Leave()
    {
        if (leaving) return;
        leaving = true;
        try { await SessionNavigation.ReturnToMenuAsync(); }
        catch (Exception exception) { leaving = false; Debug.LogException(exception); }
    }
    private NetworkedPlayerGameModeController LocalLobby => PlayerNetworkedController.LocalPlayer
        ? PlayerNetworkedController.LocalPlayer.GetComponent<NetworkedPlayerGameModeController>() : null;
    public void ToggleReady() { if (LocalLobby) LocalLobby.RPC_SetReady(!LocalLobby.Ready); }
    public void StartRound() { if (LocalLobby) LocalLobby.RPC_StartRound(); }
    public void Rematch() { if (LocalLobby) LocalLobby.RPC_ReturnToLobby(); }
    public void CopyCode() { if (Settings.Session != null) GUIUtility.systemCopyBuffer = Settings.Session.LobbyName; }
    private void RefreshLobby(NetworkedPlayerGameModeController coordinator)
    {
        PlayerScoreboardController.GetSortedPlayers(players);
        string code = Settings.Session?.LobbyName ?? "------";
        lobbyText.text = T("KAMERCODE  ", "ROOM CODE  ") + code + $"     {players.Count}/{coordinator.MaxPlayers}\n\n" +
            string.Join("\n", players.Select(p =>
            {
                var state = p.GetComponent<NetworkedPlayerGameModeController>();
                return (p.HasInputAuthority ? "› " : "   ") + p.GetPlayerName +
                    (state.IsCoordinator ? "  [HOST]" : "") + (state.Ready ? T("   GEREED", "   READY") : T("   WACHT", "   WAITING"));
            }));
        bool waiting = coordinator.Phase == 0;
        readyButton.interactable = waiting && LocalLobby;
        readyButton.GetComponentInChildren<TMP_Text>().text = LocalLobby && LocalLobby.Ready ? T("NIET GEREED", "NOT READY") : T("GEREED", "READY");
        startButton.gameObject.SetActive(LocalLobby && LocalLobby.IsCoordinator);
        startButton.interactable = waiting && coordinator.AllReady();
        var health = LocalLobby ? LocalLobby.GetComponent<PlayerHealthController>() : null;
        lobbyStatus.text = waiting ? T("JAGER", "HUNTER") + (health ? $"  ·  {health.MaxHealth} HP  ·  " : "  ·  ") +
            T("Gereed? De host start.", "Ready up. The host starts.")
            : T("Duik begint over ", "Dive starts in ") + Mathf.CeilToInt(coordinator.CountdownLeft);
    }
    private void RecordResult(string roundId)
    {
        if (recordedRound == roundId || string.IsNullOrEmpty(roundId)) return;
        PlayerScoreboardController.GetSortedPlayers(players);
        var local = players.FirstOrDefault(p => p.HasInputAuthority);
        if (!local) return;
        bool won = players.Count > 1 && local.Score == players[0].Score && local.Kills == players[0].Kills && local.Deaths == players[0].Deaths;
        var result = new MatchRecord { id = roundId, score = local.Score, kills = local.Kills, deaths = local.Deaths, won = won };
        EchoFoundation.Services.Store.RecordMatch(result, EchoFoundation.Rules);
        recordedRound = roundId;
        _ = SyncAfterResult();
    }
    private async System.Threading.Tasks.Task SyncAfterResult()
    {
        try { await EchoFoundation.Services.SyncAsync(); }
        catch (Exception exception) { Debug.LogWarning(exception.Message); }
    }
    private void RefreshResults()
    {
        if (NetworkedPlayerGameModeController.Coordinator) RecordResult(NetworkedPlayerGameModeController.Coordinator.RoundId.ToString());
        PlayerScoreboardController.GetSortedPlayers(players);
        resultsText.text = T("EINDSTAND", "FINAL STANDINGS");
        for (int i = 0; i < resultNames.Length; i++)
        {
            bool visible = i < players.Count;
            resultNames[i].gameObject.SetActive(visible); resultScores[i].gameObject.SetActive(visible); resultKillsDeaths[i].gameObject.SetActive(visible);
            if (!visible) continue;
            resultNames[i].text = $"{i + 1}. {players[i].GetPlayerName}";
            resultScores[i].text = players[i].Score.ToString(); resultKillsDeaths[i].text = $"{players[i].Kills} / {players[i].Deaths}";
        }
        var profile = EchoFoundation.Services.Store.Profile;
        var result = profile.recentMatches.FirstOrDefault(r => r.id == recordedRound);
        resultProgress.text = result == null ? "" : $"+{result.experience} XP   ·   LEVEL {EchoFoundation.Rules.Level(profile.totalExperience)}   ·   {profile.totalExperience} XP " + T("totaal", "total");
        rematchButton.interactable = LocalLobby && LocalLobby.IsCoordinator;
        rematchButton.GetComponentInChildren<TMP_Text>().text = rematchButton.interactable ? T("OPNIEUW SPELEN", "PLAY AGAIN") : T("WACHT OP HOST", "WAIT FOR HOST");
    }
    private void PopulateGraphics()
    {
        SetOptions(screenMode, new[] { T("Venster", "Windowed"), T("Volledig scherm", "Fullscreen") }, GamePreferences.ScreenMode);
        resolutions.Clear();
        foreach (var r in Screen.resolutions) { var size = new Vector2Int(r.width, r.height); if (!resolutions.Contains(size)) resolutions.Add(size); }
        var currentSize = new Vector2Int(Screen.width, Screen.height);
        if (!resolutions.Contains(currentSize)) resolutions.Add(currentSize);
        SetOptions(resolution, resolutions.Select(r => $"{r.x} × {r.y}"), resolutions.IndexOf(currentSize));
        SetOptions(quality, QualitySettings.names, GamePreferences.Quality);
        SetOptions(frameRate, frameRates.Select(f => f < 0 ? T("Onbegrensd", "Unlimited") : f.ToString()), Math.Max(0, Array.IndexOf(frameRates, GamePreferences.FrameLimit)));
        SetOptions(vsync, new[] { T("Uit", "Off"), T("Aan", "On") }, GamePreferences.VSync ? 1 : 0);
        SetOptions(language, new[] { T("Systeemtaal", "System language"), "Nederlands", "English" }, EchoLocalization.SelectedCode == "nl" ? 1 : EchoLocalization.SelectedCode == "en" ? 2 : 0);
        screenMode.interactable = resolution.interactable = GamePreferences.Desktop;
        RefreshAudio();
    }
    private static void SetOptions(TMP_Dropdown dropdown, IEnumerable<string> options, int value)
    {
        dropdown.ClearOptions(); dropdown.AddOptions(options.ToList()); dropdown.SetValueWithoutNotify(value); dropdown.RefreshShownValue();
    }
    public void ApplyGraphics()
    {
        var size = resolutions[resolution.value];
        GamePreferences.SetGraphics(quality.value, vsync.value == 1, frameRates[frameRate.value], screenMode.value, size.x, size.y);
        EchoLocalization.Select(language.value == 1 ? "nl" : language.value == 2 ? "en" : "");
        Back();
    }
    public void SetMaster(float value) => AudioPreferences.SetMasterVolume(value);
    public void SetMusic(float value) => AudioPreferences.SetMusicVolume(value);
    public void SetEffects(float value) => AudioPreferences.SetEffectsVolume(value);
    public void ToggleMute() => AudioPreferences.SetMuted(!AudioPreferences.Muted);
    private void RefreshAudio()
    {
        if (masterVolume) masterVolume.SetValueWithoutNotify(AudioPreferences.MasterVolume);
        if (musicVolume) musicVolume.SetValueWithoutNotify(AudioPreferences.MusicVolume);
        if (effectsVolume) effectsVolume.SetValueWithoutNotify(AudioPreferences.EffectsVolume);
    }
    public void BindForward() => BeginBinding(0);
    public void BindReverse() => BeginBinding(1);
    public void BindLeft() => BeginBinding(2);
    public void BindRight() => BeginBinding(3);
    public void BindBoost() => BeginBinding(4);
    public void BindFire() => BeginBinding(5);
    public void BindSonar() => BeginBinding(6);
    private void BeginBinding(int index)
    {
        binding = index; bindingButtons[index].GetComponentInChildren<TMP_Text>().text = T("DRUK EEN TOETS · ESC ANNULEERT", "PRESS A KEY · ESC CANCELS");
    }
    public void ResetBindings() { GamePreferences.ResetBindings(); RefreshBindings(); }
    private void RefreshBindings()
    {
        string[] names = { T("Vooruit", "Forward"), T("Achteruit", "Reverse"), T("Links", "Left"), T("Rechts", "Right"), "Boost", T("Torpedo", "Torpedo"), T("Sonarscan", "Sonar scan") };
        for (int i = 0; i < bindingButtons.Length; i++) bindingButtons[i].GetComponentInChildren<TMP_Text>().text = names[i] + "    " + GamePreferences.Key(i);
    }
    public void SaveName()
    {
        EchoFoundation.Services.Store.SetName(profileName.text);
        if (playerName) playerName.SetTextWithoutNotify(EchoFoundation.Services.Store.Profile.displayName);
        RefreshProfile();
    }
    private void RefreshProfile()
    {
        var profile = EchoFoundation.Services.Store.Profile;
        profileName.SetTextWithoutNotify(profile.displayName);
        profileText.text = $"LEVEL {EchoFoundation.Rules.Level(profile.totalExperience)}   ·   {profile.totalExperience} XP\n\n" +
            T("Gespeeld", "Played") + $"  {profile.matchesPlayed}    ·    " + T("Gewonnen", "Won") + $"  {profile.wins}\nKills  {profile.kills}    ·    Deaths  {profile.deaths}\n\n" +
            T("Naamwijzigingen gelden vanaf de volgende verbinding.", "Name changes apply from the next connection.");
        string state = EchoFoundation.Services.CloudState switch
        {
            CloudSyncState.Unavailable => T("Voortgang wordt lokaal opgeslagen. Cloud is nog niet ingesteld.", "Progress is saved locally. Cloud is not configured yet."),
            CloudSyncState.Conflict => T("Lokale en cloudvoortgang verschillen. Kies welke je wilt behouden.", "Local and cloud progress differ. Choose which to keep."),
            CloudSyncState.Synced => T("Voortgang gesynchroniseerd met de cloud.", "Progress synced to the cloud."),
            CloudSyncState.AccountMismatch => T("Dit profiel hoort bij een ander cloudaccount.", "This profile belongs to another cloud account."),
            CloudSyncState.Failed => T("Cloud niet bereikbaar. Je voortgang blijft lokaal bewaard.", "Cloud unavailable. Your progress remains saved locally."),
            _ => T("Cloud gereed om te synchroniseren.", "Cloud ready to sync.")
        };
        cloudStatus.text = state +
            (EchoFoundation.Services.LastError == null ? "" : "\n" + EchoFoundation.Services.LastError);
        bool conflict = EchoFoundation.Services.CloudState == CloudSyncState.Conflict;
        localCloudButton.gameObject.SetActive(conflict); remoteCloudButton.gameObject.SetActive(conflict);
    }
    public void SyncCloud() => SyncCloudInternal(null);
    public void KeepLocal() => SyncCloudInternal(true);
    public void UseCloud() => SyncCloudInternal(false);
    private async void SyncCloudInternal(bool? useLocal)
    {
        if (EchoFoundation.OnlineInitialization != null) await EchoFoundation.OnlineInitialization;
        if (EchoFoundation.Services.Platforms.Cloud == null) await EchoFoundation.Services.InitializeAsync();
        await EchoFoundation.Services.SyncAsync(useLocal);
        if (this) RefreshProfile();
    }
    private void OnDestroy()
    {
        GamePreferences.InputBlocked = false;
        GamePreferences.MenuOpen = false;
        EchoLocalization.Changed -= RefreshLanguage;
        AudioPreferences.Changed -= RefreshAudio;
        if (EchoFoundation.Services != null) EchoFoundation.Services.Store.Changed -= RefreshSuggestedName;
    }
}

