using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameDev.Foundation;
using Project.Echo.Localization;
using Project.Echo.Menu;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using static EchoMainMenuBuilder;

/// <summary>Adds editable scene UI without replacing the arena, HUD, scoreboard or artwork.</summary>
public static class EchoFoundationInstaller
{
    [InitializeOnLoadMethod]
    private static void ResumeRequestedInstall()
    {
        EditorApplication.delayCall += () =>
        {
            const string request = "Temp/EchoFoundationInstall.request";
            if (!File.Exists(request) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(request);
            try { Install(); }
            catch (Exception exception) { Debug.LogException(exception); }
        };
    }
    private static EchoMenuTheme theme;
    private static SerializedObject shell;
    private static readonly Dictionary<string, (string nl, string en)> translations = new();

    [MenuItem("Echo/Install Game Foundation")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        LoadTheme();
        theme = AssetDatabase.LoadAssetAtPath<EchoMenuTheme>("Assets/UI/Echo/EchoMenuTheme.asset");
        Directory.CreateDirectory("Assets/Resources");
        if (!AssetDatabase.LoadAssetAtPath<ProgressionRules>("Assets/Settings/Resources/Echo/EchoProgression.asset"))
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ProgressionRules>(), "Assets/Settings/Resources/Echo/EchoProgression.asset");
        var steamSettings = Type.GetType("GameDev.Steam.SteamPlatformSettings, GameDev.Steam");
        if (steamSettings != null && !AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Resources/GameDevSteamSettings.asset"))
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(steamSettings), "Assets/Resources/GameDevSteamSettings.asset");
        InstallGamepadAxes();
        translations.Clear();
        foreach (string sceneName in new[] { "MainMenu", "Game" })
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity");
            var component = ApplyToOpenScene(sceneName == "MainMenu");
            if (sceneName == "Game") RepairHudBindings();
            InstallTranslations();
            Validate(component);
            EditorSceneManager.SaveScene(scene);
            if (sceneName == "MainMenu") RenderPreviews(component);
        }
        AssetDatabase.SaveAssets();
        EchoFoundationVerification.Run();
        Debug.Log("ECHO_FOUNDATION_INSTALL_OK: lobby, pause/settings, controls, results, profile and progression saved.");
    }

    public static EchoGameShell ApplyToOpenScene(bool mainMenu)
    {
        LoadTheme();
        theme = AssetDatabase.LoadAssetAtPath<EchoMenuTheme>("Assets/UI/Echo/EchoMenuTheme.asset");
        foreach (var existing in Object.FindObjectsByType<EchoGameShell>(FindObjectsInactive.Include)) Object.DestroyImmediate(existing.gameObject);
        if (!Object.FindAnyObjectByType<EventSystem>()) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var root = new GameObject("Game Foundation UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(EchoGameShell));
        root.layer = 5;
        var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 60;
        var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        var controller = root.GetComponent<EchoGameShell>();
        shell = new SerializedObject(controller);
        var overlay = Stretch("Modal overlay", root.transform);
        var shade = overlay.gameObject.AddComponent<Image>(); shade.color = new Color(0.008f, 0.047f, 0.075f, 0.93f);
        var panel = Rect("Editable window", overlay, 0, 0, 1000, 880);
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f); panel.anchoredPosition = Vector2.zero;
        Picture("Surface", panel, theme.roundedButton, theme.panel, 0, 0, 1000, 880).type = Image.Type.Sliced;
        Picture("Accent", panel, null, theme.seafoam, 48, 34, 64, 2);
        var views = new GameObject[7]; var first = new Selectable[7];
        string[] titlesNl = { "BEMANNING", "PAUZE", "INSTELLINGEN", "BESTURING", "RESULTATEN", "KAPITEIN", "DUIK VERLATEN" };
        string[] titlesEn = { "CREW", "PAUSE", "SETTINGS", "CONTROLS", "RESULTS", "CAPTAIN", "LEAVE DIVE" };
        for (int i = 0; i < views.Length; i++)
        {
            var page = Stretch("Page " + i + " - " + titlesEn[i], panel); views[i] = page.gameObject;
            Txt("Heading" + i, page, titlesNl[i], titlesEn[i], 48, 54, 904, 54, 32);
        }
        Transform lobby = views[0].transform;
        Bind("lobbyText", Txt("Crew", lobby, "KAMERCODE  ABC123     2/8\n\n› Kapitein  [HOST]   GEREED\n   Verkenner   WACHT", "ROOM CODE  ABC123     2/8\n\n› Captain  [HOST]   READY\n   Explorer   WAITING", 48, 136, 904, 410, 24, false));
        Bind("lobbyStatus", Txt("ReadyHint", lobby, "Iedereen gereed? De host start de wedstrijd.", "Ready up. The host starts the match.", 48, 555, 904, 44, 21, false));
        var ready = Btn("Ready", lobby, "GEREED", "READY", 48, 622, 430, controller.ToggleReady, true);
        var start = Btn("Start", lobby, "START WEDSTRIJD", "START MATCH", 500, 622, 452, controller.StartRound, true);
        var copy = Btn("CopyCode", lobby, "KOPIEER CODE", "COPY CODE", 48, 704, 430, controller.CopyCode);
        var leave = Btn("LeaveLobby", lobby, "VERLATEN", "LEAVE", 500, 704, 452, controller.AskLeave);
        var lobbySettings = Btn("LobbySettings", lobby, "INSTELLINGEN", "SETTINGS", 48, 786, 904, controller.ShowSettings);
        Bind("readyButton", ready); Bind("startButton", start); first[0] = ready;
        LinkNavigation(new Selectable[] { ready, start, copy, leave, lobbySettings });

        Transform pause = views[1].transform;
        Txt("PauseHint", pause, "De online wedstrijd gaat door terwijl dit menu openstaat.", "The online match continues while this menu is open.", 48, 130, 904, 72, 21, false);
        var resume = Btn("Resume", pause, "VERDER SPELEN", "RESUME", 48, 248, 904, controller.Close, true);
        var settings = Btn("PauseSettings", pause, "INSTELLINGEN", "SETTINGS", 48, 344, 904, controller.ShowSettings);
        var controls = Btn("PauseControls", pause, "BESTURING", "CONTROLS", 48, 440, 904, controller.ShowControls);
        var profile = Btn("PauseProfile", pause, "PROFIEL EN VOORTGANG", "PROFILE AND PROGRESS", 48, 536, 904, controller.ShowProfile);
        var exit = Btn("PauseLeave", pause, "WEDSTRIJD VERLATEN", "LEAVE MATCH", 48, 704, 904, controller.AskLeave);
        first[1] = resume; LinkNavigation(new Selectable[] { resume, settings, controls, profile, exit });

        Transform graphics = views[2].transform;
        var mode = Dropdown("ScreenMode", graphics, "SCHERMMODUS", "DISPLAY MODE", 135, new[] { "Venster", "Volledig scherm" }); Bind("screenMode", mode);
        var res = Dropdown("Resolution", graphics, "RESOLUTIE", "RESOLUTION", 198, new[] { "1920 × 1080" }); Bind("resolution", res);
        var q = Dropdown("Quality", graphics, "BEELDKWALITEIT", "QUALITY", 261, QualitySettings.names); Bind("quality", q);
        var fps = Dropdown("FrameRate", graphics, "FPS-LIMIET", "FRAME LIMIT", 324, new[] { "60" }); Bind("frameRate", fps);
        var v = Dropdown("VSync", graphics, "V-SYNC", "V-SYNC", 387, new[] { "Aan", "Uit" }); Bind("vsync", v);
        var lang = Dropdown("Language", graphics, "TAAL", "LANGUAGE", 450, new[] { "Systeemtaal", "Nederlands", "English" }); Bind("language", lang);
        var master = Volume("Master", graphics, "MASTERVOLUME", "MASTER VOLUME", 520, controller.SetMaster); Bind("masterVolume", master);
        var music = Volume("Music", graphics, "MUZIEK", "MUSIC", 573, controller.SetMusic); Bind("musicVolume", music);
        var effects = Volume("Effects", graphics, "EFFECTEN", "EFFECTS", 626, controller.SetEffects); Bind("effectsVolume", effects);
        var mute = Btn("Mute", graphics, "GELUID AAN / UIT", "TOGGLE SOUND", 48, 690, 430, controller.ToggleMute);
        var keys = Btn("OpenControls", graphics, "BESTURING", "CONTROLS", 500, 690, 452, controller.ShowControls);
        var apply = Btn("Apply", graphics, "TOEPASSEN", "APPLY", 48, 786, 430, controller.ApplyGraphics, true);
        var back = Btn("Back", graphics, "TERUG", "BACK", 500, 786, 452, controller.Back);
        first[2] = mode; LinkNavigation(new Selectable[] { mode, res, q, fps, v, lang, master, music, effects, mute, keys, apply, back });

        Transform input = views[3].transform;
        Txt("ControllerHint", input, "Controller: linker stick varen/draaien · A schieten · X sonar\nLB boost · Start pauze · B terug · View scorebord", "Controller: left stick move/turn · A fire · X sonar\nLB boost · Start pause · B back · View scoreboard", 48, 120, 904, 84, 20, false);
        var bindings = new Button[7];
        UnityAction[] callbacks = { controller.BindForward, controller.BindReverse, controller.BindLeft, controller.BindRight, controller.BindBoost, controller.BindFire, controller.BindSonar };
        string[] keyLabels = { "Vooruit    W", "Achteruit    S", "Links    A", "Rechts    D", "Boost    Left Shift", "Schieten    Space", "Sonar    Left Control" };
        for (int i = 0; i < bindings.Length; i++) bindings[i] = Btn("Binding" + i, input, keyLabels[i], keyLabels[i], 48, 220 + i * 65, 904, callbacks[i]);
        var reset = Btn("ResetKeys", input, "HERSTEL TOETSEN", "RESET KEYS", 48, 786, 430, controller.ResetBindings);
        var keyBack = Btn("Back", input, "TERUG", "BACK", 500, 786, 452, controller.Back);
        SetArray(shell, "bindingButtons", bindings); first[3] = bindings[0];
        LinkNavigation(bindings.Cast<Selectable>().Concat(new Selectable[] { reset, keyBack }).ToArray());

        Transform results = views[4].transform;
        Bind("resultsText", Txt("FinalStandings", results, "EINDSTAND", "FINAL STANDINGS", 48, 130, 904, 40, 22, false));
        Txt("NameColumn", results, "NAAM", "NAME", 48, 190, 520, 30, 18, false);
        Txt("ScoreColumn", results, "SCORE", "SCORE", 600, 190, 150, 30, 18, false);
        Txt("KdColumn", results, "K / D", "K / D", 800, 190, 150, 30, 18, false);
        var names = new TMP_Text[8]; var scores = new TMP_Text[8]; var killsDeaths = new TMP_Text[8];
        for (int i = 0; i < 8; i++)
        {
            names[i] = Label("Player " + i, results, i < 2 ? (i + 1) + ". Kapitein" : "", 48, 238 + i * 40, 520, 36, 24, theme.seafoam);
            names[i].richText = false; names[i].overflowMode = TextOverflowModes.Ellipsis;
            scores[i] = Label("Score " + i, results, i < 2 ? "100" : "", 600, 238 + i * 40, 150, 36, 24, theme.seafoam);
            killsDeaths[i] = Label("Kills deaths " + i, results, i < 2 ? "2 / 0" : "", 800, 238 + i * 40, 150, 36, 24, theme.seafoam);
        }
        SetArray(shell, "resultNames", names); SetArray(shell, "resultScores", scores); SetArray(shell, "resultKillsDeaths", killsDeaths);
        Bind("resultProgress", Txt("EarnedXp", results, "+170 XP   ·   LEVEL 1   ·   170 XP totaal", "+170 XP   ·   LEVEL 1   ·   170 XP total", 48, 590, 904, 64, 25));
        var replay = Btn("Rematch", results, "OPNIEUW SPELEN", "PLAY AGAIN", 48, 704, 904, controller.Rematch, true);
        var resultExit = Btn("ResultLeave", results, "HOOFDMENU", "MAIN MENU", 48, 786, 904, controller.Leave);
        Bind("rematchButton", replay); first[4] = replay; LinkNavigation(new Selectable[] { replay, resultExit });

        Transform captain = views[5].transform;
        var name = InputField("ProfileName", captain, 48, 138, 650, 58, 24); Bind("profileName", name);
        var save = Btn("SaveName", captain, "OPSLAAN", "SAVE", 720, 138, 232, controller.SaveName);
        Bind("profileText", Txt("ProfileStats", captain, "LEVEL 1   ·   170 XP\n\nGespeeld  1    ·    Gewonnen  1\nKills  2    ·    Deaths  0", "LEVEL 1   ·   170 XP\n\nPlayed  1    ·    Won  1\nKills  2    ·    Deaths  0", 48, 236, 904, 260, 25, false));
        Bind("cloudStatus", Txt("CloudState", captain, "Voortgang wordt lokaal opgeslagen.", "Progress is saved locally.", 48, 512, 904, 110, 20, false));
        var sync = Btn("CloudSync", captain, "CLOUD SYNCHRONISEREN", "SYNC CLOUD", 48, 634, 904, controller.SyncCloud);
        var local = Btn("KeepLocal", captain, "BEHOUD LOKAAL", "KEEP LOCAL", 48, 706, 430, controller.KeepLocal);
        var remote = Btn("UseCloud", captain, "GEBRUIK CLOUD", "USE CLOUD", 500, 706, 452, controller.UseCloud);
        var profileBack = Btn("Back", captain, "TERUG", "BACK", 48, 786, 904, controller.Back);
        Bind("localCloudButton", local); Bind("remoteCloudButton", remote); first[5] = name;
        LinkNavigation(new Selectable[] { name, save, sync, local, remote, profileBack });

        Transform confirm = views[6].transform;
        Txt("LeaveWarning", confirm, "Wil je deze duik verlaten?\nEen onafgemaakte wedstrijd levert geen XP op.", "Leave this dive?\nAn unfinished match does not award XP.", 48, 190, 904, 160, 26, false);
        var stay = Btn("Stay", confirm, "BLIJVEN", "STAY", 48, 520, 904, controller.Back, true);
        var confirmLeave = Btn("ConfirmLeave", confirm, "VERLATEN", "LEAVE", 48, 622, 904, controller.Leave);
        first[6] = stay; LinkNavigation(new Selectable[] { stay, confirmLeave });

        Bind("overlay", overlay.gameObject); SetArray(shell, "pages", views); SetArray(shell, "firstControls", first);
        shell.ApplyModifiedPropertiesWithoutUndo();
        if (mainMenu) InstallMainMenu(controller);
        // Live values are formatted by the controller; table callbacks must not overwrite them.
        foreach (string field in new[] { "lobbyText", "lobbyStatus", "resultsText", "resultProgress", "profileText", "cloudStatus" })
        {
            var text = (TMP_Text)shell.FindProperty(field).objectReferenceValue;
            var localized = text.GetComponent<EchoLocalizedTmpText>(); if (localized) Object.DestroyImmediate(localized);
        }
        foreach (var button in bindings.Concat(new[] { ready, replay }))
        {
            var localized = button.GetComponentInChildren<EchoLocalizedTmpText>(true); if (localized) Object.DestroyImmediate(localized);
        }
        foreach (var page in views) page.SetActive(false);
        foreach (var selectable in root.GetComponentsInChildren<Selectable>(true))
        {
            var navigation = selectable.navigation; navigation.mode = Navigation.Mode.Automatic; selectable.navigation = navigation;
        }
        overlay.gameObject.SetActive(false);
        return controller;
    }

    private static void InstallMainMenu(EchoGameShell controller)
    {
        var menu = Object.FindAnyObjectByType<MenuController>(); var menuSo = new SerializedObject(menu);
        var playPage = ((GameObject)menuSo.FindProperty("pages").GetArrayElementAtIndex(0).objectReferenceValue).transform;
        for (int i = playPage.childCount - 1; i >= 0; i--) Object.DestroyImmediate(playPage.GetChild(i).gameObject);
        Txt("PlayHeading", playPage, "SAMEN DUIKEN", "DIVE TOGETHER", 48, 50, 664, 54, 32);
        Txt("PlayerNameLabel", playPage, "SPELERSNAAM", "PLAYER NAME", 48, 119, 664, 30, 18, false);
        var name = InputField("PlayerName", playPage, 48, 157, 664, 52, 24);
        Txt("RoomCodeLabel", playPage, "KAMERCODE (OPTIONEEL)", "ROOM CODE (OPTIONAL)", 48, 225, 664, 30, 18, false);
        var code = InputField("RoomCode", playPage, 48, 263, 664, 52, 6); code.characterValidation = TMP_InputField.CharacterValidation.Alphanumeric;
        var host = Btn("Host", playPage, "KAMER MAKEN", "CREATE ROOM", 48, 338, 320, controller.Host, true);
        var join = Btn("Join", playPage, "AANSLUITEN", "JOIN", 392, 338, 320, controller.Join);
        var message = Txt("ConnectionMessage", playPage, "", "", 48, 410, 664, 40, 18, false);
        var back = Btn("Back", playPage, "TERUG", "BACK", 48, 476, 220, menu.ClosePanel);
        LinkNavigation(new Selectable[] { name, code, host, join, back });
        menuSo.FindProperty("firstPageButtons").GetArrayElementAtIndex(0).objectReferenceValue = host;
        var general = ((GameObject)menuSo.FindProperty("pages").GetArrayElementAtIndex(3).objectReferenceValue).transform;
        var extra = general.Find("ExtendedSettings"); if (extra) Object.DestroyImmediate(extra.gameObject);
        Btn("ExtendedSettings", general, "MEER INSTELLINGEN", "MORE SETTINGS", 300, 459, 412, controller.ShowSettings);
        foreach (var selectable in general.GetComponentsInChildren<Selectable>(true))
        {
            var navigation = selectable.navigation; navigation.mode = Navigation.Mode.Automatic; selectable.navigation = navigation;
        }
        var ui = menu.transform.Find("Interface");
        var previous = ui.Find("OpenProfile"); if (previous) Object.DestroyImmediate(previous.gameObject);
        Btn("OpenProfile", ui, "PROFIEL", "PROFILE", 1394, 130, 300, controller.ShowProfile);
        var logbook = ui.Find("Logboek").GetComponent<Button>();
        logbook.onClick = new Button.ButtonClickedEvent(); UnityEventTools.AddPersistentListener(logbook.onClick, controller.ShowProfile);
        menuSo.ApplyModifiedPropertiesWithoutUndo();
        shell.Update(); Bind("playerName", name); Bind("roomCode", code); Bind("connectionMessage", message); shell.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Bind(string field, Object value) => shell.FindProperty(field).objectReferenceValue = value;
    private static TMP_Text Txt(string key, Transform parent, string nl, string en, float x, float y, float w, float h, float size, bool bright = true)
    {
        var text = Label(key, parent, nl, x, y, w, h, size, bright ? theme.seafoam : theme.muted);
        text.richText = false;
        text.enableAutoSizing = true; text.fontSizeMin = size * 0.75f; text.fontSizeMax = size;
        text.alignment = TextAlignmentOptions.TopLeft;
        if (nl.Length > 0)
        {
            string id = "SHELL_" + key;
            translations[id] = (nl, en);
            var localized = text.gameObject.AddComponent<EchoLocalizedTmpText>(); localized.StringReference.SetReference(EchoLocalization.TableName, id);
        }
        return text;
    }
    private static Button Btn(string key, Transform parent, string nl, string en, float x, float y, float width, UnityAction callback, bool primary = false)
    {
        var button = MakeButton(key, parent, nl, x, y, width, 58, primary, callback);
        var label = button.GetComponentInChildren<TMP_Text>(); label.characterSpacing = 1; label.fontSize = 22;
        label.enableAutoSizing = true; label.fontSizeMin = 15; label.fontSizeMax = 22;
        string id = "SHELL_" + key; translations[id] = (nl, en);
        label.gameObject.AddComponent<EchoLocalizedTmpText>().StringReference.SetReference(EchoLocalization.TableName, id);
        // Automatic navigation handles hidden/disabled controls on every platform.
        var nav = button.navigation; nav.mode = Navigation.Mode.Automatic; button.navigation = nav;
        return button;
    }
    private static TMP_InputField InputField(string key, Transform parent, float x, float y, float width, float height, int limit)
    {
        var image = Picture(key, parent, theme.roundedButton, new Color32(16, 47, 59, 255), x, y, width, height);
        image.type = Image.Type.Sliced; image.raycastTarget = true;
        var field = image.gameObject.AddComponent<TMP_InputField>(); field.targetGraphic = image;
        var viewport = Rect("Text Area", image.transform, 16, 6, width - 32, height - 12); viewport.gameObject.AddComponent<RectMask2D>();
        var label = Label("Editable text", viewport, "", 0, 0, width - 32, height - 12, 24, theme.seafoam);
        label.richText = false; field.textViewport = viewport; field.textComponent = (TextMeshProUGUI)label;
        field.characterLimit = limit; field.lineType = TMP_InputField.LineType.SingleLine;
        return field;
    }
    private static TMP_Dropdown Dropdown(string key, Transform parent, string nl, string en, float y, IEnumerable<string> options)
    {
        Txt(key + "Label", parent, nl, en, 48, y + 13, 390, 34, 20, false);
        var image = Picture(key, parent, theme.roundedButton, new Color32(16, 47, 59, 255), 452, y, 500, 50);
        image.type = Image.Type.Sliced; image.raycastTarget = true;
        var dropdown = image.gameObject.AddComponent<TMP_Dropdown>(); dropdown.targetGraphic = image;
        dropdown.captionText = Label("Value", image.transform, "", 18, 0, 440, 50, 22, theme.seafoam);
        var arrow = Picture("Arrow", image.transform, theme.arrow, theme.seafoam, 465, 16, 18, 18);
        arrow.rectTransform.localRotation = Quaternion.Euler(0, 0, -90);
        var template = Rect("Template", image.transform, 0, 50, 500, 240);
        template.gameObject.AddComponent<CanvasGroup>();
        template.gameObject.AddComponent<Image>().color = theme.panel;
        var viewport = Stretch("Viewport", template); viewport.gameObject.AddComponent<RectMask2D>();
        var content = Rect("Content", viewport, 0, 0, 500, 44);
        var item = Rect("Item", content, 0, 0, 500, 44);
        var itemImage = item.gameObject.AddComponent<Image>(); itemImage.color = new Color32(24, 62, 72, 255);
        var toggle = item.gameObject.AddComponent<Toggle>(); toggle.targetGraphic = itemImage;
        var itemText = Label("Item Label", item, "Option", 18, 0, 456, 44, 22, theme.seafoam); itemText.raycastTarget = true;
        var scroll = template.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        dropdown.template = template; dropdown.itemText = itemText; dropdown.AddOptions(options.ToList()); template.gameObject.SetActive(false);
        return dropdown;
    }
    private static Slider Volume(string key, Transform parent, string nl, string en, float y, UnityAction<float> callback)
    {
        Txt(key + "Volume", parent, nl, en, 48, y + 5, 390, 32, 20, false);
        var track = Picture(key + "Slider", parent, null, Color.clear, 452, y, 500, 36);
        track.raycastTarget = true;
        var slider = track.gameObject.AddComponent<Slider>();
        Picture("Track", track.transform, null, new Color32(24, 62, 72, 255), 10, 14, 480, 8);
        var fillArea = Rect("Fill area", track.transform, 10, 14, 480, 8);
        var fill = Picture("Fill", fillArea, null, theme.seafoam, 0, 0, 0, 0);
        StretchRect(fill.rectTransform);
        var handleArea = Rect("Handle area", track.transform, 10, 0, 480, 36);
        var handle = Picture("Handle", handleArea, theme.roundedButton, theme.seafoam, 0, 0, 20, 36);
        handle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        slider.fillRect = fill.rectTransform; slider.handleRect = handle.rectTransform; slider.targetGraphic = handle;
        handle.rectTransform.sizeDelta = new Vector2(20, -8);
        slider.value = 0.8f; UnityEventTools.AddPersistentListener(slider.onValueChanged, callback);
        return slider;
    }
    internal static void InstallTranslations()
    {
        var collection = LocalizationEditorSettings.GetStringTableCollection(EchoLocalization.TableName);
        foreach (string language in new[] { "nl", "en" })
        {
            var table = collection.GetTable(new LocaleIdentifier(language)) as StringTable;
            foreach (var pair in translations) table.AddEntry(pair.Key, language == "nl" ? pair.Value.nl : pair.Value.en);
            EditorUtility.SetDirty(table);
        }
        EditorUtility.SetDirty(collection.SharedData);
    }
    private static void InstallGamepadAxes()
    {
        var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
        var serialized = new SerializedObject(asset); var axes = serialized.FindProperty("m_Axes");
        foreach (string name in new[] { "GamepadHorizontal", "GamepadVertical" })
        {
            bool found = false;
            for (int i = 0; i < axes.arraySize; i++) if (axes.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name").stringValue == name) found = true;
            if (found) continue;
            axes.InsertArrayElementAtIndex(axes.arraySize); var axis = axes.GetArrayElementAtIndex(axes.arraySize - 1);
            axis.FindPropertyRelative("m_Name").stringValue = name;
            foreach (string field in new[] { "negativeButton", "positiveButton", "altNegativeButton", "altPositiveButton" }) axis.FindPropertyRelative(field).stringValue = "";
            axis.FindPropertyRelative("type").intValue = 2;
            axis.FindPropertyRelative("axis").intValue = name.EndsWith("Vertical") ? 1 : 0;
            axis.FindPropertyRelative("invert").boolValue = name.EndsWith("Vertical");
            axis.FindPropertyRelative("gravity").floatValue = 0; axis.FindPropertyRelative("sensitivity").floatValue = 1;
            axis.FindPropertyRelative("dead").floatValue = 0.19f; axis.FindPropertyRelative("joyNum").intValue = 0;
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
    private static void Validate(EchoGameShell controller)
    {
        var data = new SerializedObject(controller);
        foreach (string field in new[] { "overlay", "lobbyText", "readyButton", "startButton", "resultsText", "profileName", "screenMode", "resolution", "quality", "frameRate", "language", "masterVolume" })
            if (!data.FindProperty(field).objectReferenceValue) throw new InvalidOperationException("Unbound shell field: " + field);
        foreach (var button in controller.GetComponentsInChildren<Button>(true))
            if (button.onClick.GetPersistentEventCount() == 0) throw new InvalidOperationException("Unbound shell button: " + button.name);
    }
    private static void RepairHudBindings()
    {
        foreach (var hud in Object.FindObjectsByType<MatchHud>(FindObjectsInactive.Include))
        {
            var data = new SerializedObject(hud);
            string[] fields = { "_killsText", "_timeLabel", "_timeText", "_rivalLabel", "_rivalText" };
            string[] children = { "KILLS value", "RESTEREND label", "RESTEREND value", "KOPLOPER label", "KOPLOPER value" };
            var text = hud.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < fields.Length; i++)
            {
                var property = data.FindProperty(fields[i]);
                if (!property.objectReferenceValue) property.objectReferenceValue = text.FirstOrDefault(t => t.name == children[i]);
                if (!property.objectReferenceValue) throw new InvalidOperationException("Missing HUD text: " + fields[i]);
            }
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
    private static void RenderPreviews(EchoGameShell controller)
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include).Where(c => c.isRootCanvas).ToArray();
        var camera = Camera.main;
        foreach (int page in new[] { 0, 2, 3, 4, 5 })
        {
            controller.Open(page);
            foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 1024) })
            {
                var target = new RenderTexture(size.x, size.y, 24); target.Create(); camera.targetTexture = target;
                foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; }
                Canvas.ForceUpdateCanvases(); camera.Render();
                var image = new Texture2D(size.x, size.y, TextureFormat.RGB24, false); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0); image.Apply();
                File.WriteAllBytes($"../docs/foundation-{page}-{size.x}.png", image.EncodeToPNG());
                RenderTexture.active = null; camera.targetTexture = null; target.Release(); Object.DestroyImmediate(target); Object.DestroyImmediate(image);
            }
        }
        foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
        controller.Close();
    }
}

