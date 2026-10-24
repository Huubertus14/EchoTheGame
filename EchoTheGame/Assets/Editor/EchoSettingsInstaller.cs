using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Project.Echo.Localization;
using Project.Echo.Menu;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;
using static EchoMainMenuBuilder;

/// <summary>Upgrades existing menu objects in place; keeps artwork, navigation and other pages.</summary>
public static class EchoSettingsInstaller
{
    private const string Folder = "Assets/Localization";
    private const string Request = "Temp/EchoSettingsUpgrade.request";
    private static EchoMenuTheme Theme => AssetDatabase.LoadAssetAtPath<EchoMenuTheme>("Assets/UI/Echo/EchoMenuTheme.asset");

    [InitializeOnLoadMethod]
    private static void ResumeRequestedUpgrade()
    {
        if (!File.Exists(Request)) return;
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(Request);
            try { Install(); }
            catch (Exception exception) { Debug.LogException(exception); }
        };
    }

    [MenuItem("Echo/Upgrade Localization and Settings")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before authoring settings.");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var previous = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            InstallTables();
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            ApplyToOpenScene();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            ValidateAndPreview();
            ValidateLocalizationAndRender();
            Debug.Log("ECHO_SETTINGS_UPGRADE_OK: localized menu, language selector, sound toggle and three volume controls.");
        }
        finally
        {
            if (previous.Any(scene => scene.isLoaded && scene.isActive))
                EditorSceneManager.RestoreSceneManagerSetup(previous);
        }
    }

    public static void InstallTables()
    {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets", "Localization");
        if (LocalizationEditorSettings.ActiveLocalizationSettings == null)
        {
            var settings = ScriptableObject.CreateInstance<LocalizationSettings>();
            AssetDatabase.CreateAsset(settings, Folder + "/Localization Settings.asset");
            LocalizationEditorSettings.ActiveLocalizationSettings = settings;
        }
        var locales = new List<Locale>();
        foreach (var language in new[] { SystemLanguage.Dutch, SystemLanguage.English })
        {
            var locale = LocalizationEditorSettings.GetLocale(language);
            if (locale == null)
            {
                locale = Locale.CreateLocale(language);
                locale.LocaleName = language == SystemLanguage.Dutch ? "Nederlands" : "English";
                AssetDatabase.CreateAsset(locale, $"{Folder}/{language}.asset");
                LocalizationEditorSettings.AddLocale(locale);
            }
            locales.Add(locale);
        }
        LocalizationSettings.ProjectLocale = locales[1];
        var collection = LocalizationEditorSettings.GetStringTableCollection(EchoLocalization.TableName) ??
            LocalizationEditorSettings.CreateStringTableCollection(EchoLocalization.TableName, Folder, locales);
        for (int i = 0; i < locales.Count; i++)
        {
            var table = collection.GetTable(locales[i].Identifier) as StringTable ?? collection.AddNewTable(locales[i].Identifier) as StringTable;
            foreach (var entry in Entries) table.AddEntry(entry.key, i == 0 ? entry.nl : entry.en);
            LocalizationEditorSettings.SetPreloadTableFlag(table, true);
            EditorUtility.SetDirty(table);
        }
        EditorUtility.SetDirty(collection.SharedData);
        EditorUtility.SetDirty(collection);
        EditorUtility.SetDirty(LocalizationEditorSettings.ActiveLocalizationSettings);
        AssetDatabase.SaveAssets();
    }

    public static void ApplyToOpenScene()
    {
        InstallTables();
        LoadTheme();
        var controller = UnityEngine.Object.FindAnyObjectByType<MenuController>();
        if (controller == null) throw new InvalidOperationException("Open MainMenu before applying settings.");
        var serialized = new SerializedObject(controller);
        var pageArray = serialized.FindProperty("pages");
        var pages = Enumerable.Range(0, pageArray.arraySize)
            .Select(i => (GameObject)pageArray.GetArrayElementAtIndex(i).objectReferenceValue).ToList();
        var firstArray = serialized.FindProperty("firstPageButtons");
        var firstButtons = Enumerable.Range(0, firstArray.arraySize)
            .Select(i => (Button)firstArray.GetArrayElementAtIndex(i).objectReferenceValue).ToList();
        var general = pages[3].transform;
        for (int i = general.childCount - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(general.GetChild(i).gameObject);
        Label("Heading", general, "INSTELLINGEN", 48, 65, 660, 56, 34, Theme.seafoam, 5);
        Label("Language title", general, "TAAL", 48, 145, 664, 32, 21, Theme.muted, 4);
        var dropdown = CreateLanguageDropdown(general);
        UnityEventTools.AddPersistentListener(dropdown.onValueChanged, controller.ChangeLanguage);
        var audioButton = MakeButton("Audio settings", general, "GELUID", 48, 263, 664, 64, false, controller.ShowAudio);
        var fullscreen = MakeButton("Fullscreen", general, "VOLLEDIG SCHERM: UIT", 48, 348, 664, 64, false, controller.ToggleFullscreen);
        var generalBack = MakeButton("Terug", general, "TERUG", 48, 459, 220, 58, false, controller.ClosePanel);
        LinkNavigation(new Selectable[] { dropdown, audioButton, fullscreen, generalBack });
        firstButtons[3] = audioButton;
        RectTransform audio;
        if (pages.Count > 5)
        {
            audio = (RectTransform)pages[5].transform;
            for (int i = audio.childCount - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(audio.GetChild(i).gameObject);
        }
        else
        {
            audio = Stretch("Audio settings", general.parent);
            pages.Add(audio.gameObject);
            firstButtons.Add(null);
        }
        Label("Heading", audio, "GELUID", 48, 65, 660, 56, 34, Theme.seafoam, 5);
        var toggleRoot = Rect("Sound enabled", audio, 48, 138, 664, 42);
        var toggle = toggleRoot.gameObject.AddComponent<Toggle>();
        var toggleBox = Picture("Toggle background", toggleRoot, Theme.roundedButton, Theme.muted, 0, 5, 32, 32);
        var check = Picture("Checkmark", toggleBox.transform, Theme.roundedButton, Theme.seafoam, 6, 6, 20, 20);
        toggleBox.raycastTarget = true;
        toggle.targetGraphic = toggleBox;
        toggle.graphic = check;
        toggle.isOn = true;
        Label("Sound label", toggleRoot, "GELUID AAN", 52, 0, 560, 42, 22, Theme.seafoam, 2).raycastTarget = true;
        UnityEventTools.AddPersistentListener(toggle.onValueChanged, controller.SetSoundEnabled);
        var master = CreateVolume(audio, "Master", "MASTERVOLUME", 195, controller.ApplyVolume, out var masterValue);
        var music = CreateVolume(audio, "Music", "ACHTERGRONDMUZIEK", 274, controller.ApplyMusicVolume, out var musicValue);
        var effects = CreateVolume(audio, "Effects", "GELUIDSEFFECTEN", 353, controller.ApplyEffectsVolume, out var effectsValue);
        var audioBack = MakeButton("Terug", audio, "TERUG", 48, 459, 220, 58, false, controller.ShowSettings);
        firstButtons[5] = audioBack;
        LinkNavigation(new Selectable[] { toggle, master, music, effects, audioBack });
        SetArray(serialized, "pages", pages.ToArray());
        SetArray(serialized, "firstPageButtons", firstButtons.ToArray());
        Bind(serialized, "languageDropdown", dropdown);
        Bind(serialized, "soundToggle", toggle);
        Bind(serialized, "volumeSlider", master);
        Bind(serialized, "volumeLabel", masterValue);
        Bind(serialized, "musicSlider", music);
        Bind(serialized, "musicLabel", musicValue);
        Bind(serialized, "effectsSlider", effects);
        Bind(serialized, "effectsLabel", effectsValue);
        Bind(serialized, "fullscreenLabel", fullscreen.GetComponentInChildren<TMP_Text>());
        serialized.ApplyModifiedPropertiesWithoutUndo();
        foreach (var label in controller.GetComponentsInChildren<TMP_Text>(true))
        {
            if (label.transform.IsChildOf(dropdown.transform)) continue;
            var entry = Entries.FirstOrDefault(e => e.nl == label.text);
            if (entry.key == null || entry.key == "FULLSCREEN_OFF") continue;
            var localizer = label.GetComponent<EchoLocalizedTmpText>() ?? label.gameObject.AddComponent<EchoLocalizedTmpText>();
            localizer.StringReference.SetReference(EchoLocalization.TableName, entry.key);
            EditorUtility.SetDirty(localizer);
            label.enableAutoSizing = true;
            label.fontSizeMax = label.fontSize;
            label.fontSizeMin = label.fontSize * 0.7f;
        }
        audio.gameObject.SetActive(false);
        controller.ClosePanel();
        EditorUtility.SetDirty(controller);
    }

    private static void Bind(SerializedObject target, string name, UnityEngine.Object value) => target.FindProperty(name).objectReferenceValue = value;

    private static void ValidateLocalizationAndRender()
    {
        var controller = UnityEngine.Object.FindAnyObjectByType<MenuController>();
        var serialized = new SerializedObject(controller);
        foreach (string field in new[] { "languageDropdown", "soundToggle", "musicSlider", "effectsSlider", "musicLabel", "effectsLabel" })
            if (serialized.FindProperty(field).objectReferenceValue == null)
                throw new InvalidOperationException("Unbound settings field: " + field);
        var labels = controller.GetComponentsInChildren<TMP_Text>(true);
        var original = labels.ToDictionary(label => label, label => label.text);
        var collection = LocalizationEditorSettings.GetStringTableCollection(EchoLocalization.TableName);
        try
        {
            foreach (string code in new[] { "nl", "en" })
            {
                var table = collection.GetTable(new LocaleIdentifier(code)) as StringTable;
                if (table == null || Entries.Any(entry => table.GetEntry(entry.key) == null))
                    throw new InvalidOperationException("Missing menu translations for " + code);
                foreach (var localizer in controller.GetComponentsInChildren<EchoLocalizedTmpText>(true))
                {
                    var key = localizer.StringReference.TableEntryReference.Key;
                    var entry = table.GetEntry(key);
                    if (entry == null) throw new InvalidOperationException("Missing translation: " + key);
                    localizer.GetComponent<TMP_Text>().text = entry.LocalizedValue;
                }
                ((TMP_Text)serialized.FindProperty("fullscreenLabel").objectReferenceValue).text = table.GetEntry("FULLSCREEN_OFF").LocalizedValue;
                ((TMP_Dropdown)serialized.FindProperty("languageDropdown").objectReferenceValue).captionText.text = table.GetEntry("LANGUAGE_AUTO").LocalizedValue;
                controller.ClosePanel();
                RenderPreview(1920, 1080, $"../docs/main-menu-{code}-preview.png");
                controller.ShowSettings();
                RenderPreview(1920, 1080, $"../docs/main-menu-settings-{code}-preview.png");
                controller.ShowAudio();
                RenderPreview(1920, 1080, $"../docs/main-menu-audio-{code}-preview.png");
            }
        }
        finally
        {
            foreach (var pair in original) pair.Key.text = pair.Value;
            controller.ClosePanel();
        }
        Debug.Log($"ECHO_LOCALIZATION_VALIDATION_OK: {Entries.Length} keys per language, NL/EN previews, all settings bindings.");
    }

    private static Slider CreateVolume(Transform parent, string name, string title, float y, UnityAction<float> callback, out TMP_Text value)
    {
        Label(name + " title", parent, title, 48, y, 495, 30, 20, Theme.muted, 2);
        value = Label(name + " value", parent, name == "Master" ? "80%" : "100%", 605, y, 107, 30, 22, Theme.seafoam);
        value.alignment = TextAlignmentOptions.MidlineRight;
        var root = Rect(name + " volume", parent, 48, y + 29, 664, 36);
        var slider = root.gameObject.AddComponent<Slider>();
        Picture("Track", root, null, new Color32(36, 66, 77, 255), 0, 14, 664, 8);
        var fillArea = Rect("Fill area", root, 10, 14, 644, 8);
        var fill = Picture("Fill", fillArea, null, Theme.seafoam, 0, 0, 644, 8);
        StretchRect(fill.rectTransform);
        var handleArea = Rect("Handle area", root, 10, 0, 644, 36);
        var handle = Picture("Handle", handleArea, Theme.roundedButton, Theme.seafoam, 0, 0, 20, 32);
        handle.rectTransform.anchorMin = handle.rectTransform.anchorMax = new Vector2(0, 0.5f);
        handle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        handle.rectTransform.anchoredPosition = Vector2.zero;
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        handle.rectTransform.sizeDelta = new Vector2(20, -4);
        slider.value = name == "Master" ? 0.8f : 1;
        UnityEventTools.AddPersistentListener(slider.onValueChanged, callback);
        return slider;
    }

    private static TMP_Dropdown CreateLanguageDropdown(Transform parent)
    {
        var background = Picture("Language", parent, Theme.roundedButton, new Color32(24, 52, 64, 255), 48, 187, 664, 54);
        background.type = Image.Type.Sliced;
        background.raycastTarget = true;
        var dropdown = background.gameObject.AddComponent<TMP_Dropdown>();
        dropdown.targetGraphic = background;
        dropdown.captionText = Label("Selected language", dropdown.transform, "Automatisch", 20, 0, 590, 54, 23, Theme.seafoam);
        Label("Arrow", dropdown.transform, "+", 620, 0, 40, 54, 24, Theme.seafoam);
        var template = Rect("Template", dropdown.transform, 0, 54, 664, 180);
        template.anchorMin = template.anchorMax = new Vector2(0, 0);
        var templateBackground = template.gameObject.AddComponent<Image>();
        templateBackground.color = Theme.panel;
        var scroll = template.gameObject.AddComponent<ScrollRect>();
        var viewport = Stretch("Viewport", template);
        viewport.gameObject.AddComponent<RectMask2D>();
        var content = Rect("Content", viewport, 0, 0, 664, 46);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.sizeDelta = new Vector2(0, 46);
        var item = Rect("Item", content, 0, 0, 664, 46);
        item.anchorMin = new Vector2(0, 0.5f);
        item.anchorMax = new Vector2(1, 0.5f);
        item.pivot = new Vector2(0.5f, 0.5f);
        item.anchoredPosition = Vector2.zero;
        item.sizeDelta = new Vector2(0, 46);
        var surface = item.gameObject.AddComponent<Image>();
        surface.color = new Color32(24, 52, 64, 255);
        var toggle = item.gameObject.AddComponent<Toggle>();
        toggle.targetGraphic = surface;
        var check = Picture("Check", item, Theme.roundedButton, Theme.seafoam, 9, 16, 12, 12);
        toggle.graphic = check;
        dropdown.itemText = Label("Item label", item, "Nederlands", 36, 0, 600, 46, 23, Theme.seafoam);
        scroll.content = content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        dropdown.template = template;
        dropdown.AddOptions(new List<string> { "Automatisch", "Nederlands", "English" });
        template.gameObject.SetActive(false);
        return dropdown;
    }

    private static readonly (string key, string nl, string en)[] Entries =
    {
        ("SUBTITLE", "LUISTER NAAR DE DIEPTE", "LISTEN TO THE DEPTHS"),
        ("PLAY", "SPELEN", "PLAY"), ("SUBMARINE", "ONDERZEEËR", "SUBMARINE"),
        ("LOGBOOK", "LOGBOEK", "LOGBOOK"), ("SETTINGS", "INSTELLINGEN", "SETTINGS"),
        ("QUIT", "AFSLUITEN", "QUIT"), ("CAPTAIN", "KAPITEIN", "CAPTAIN"),
        ("LEVEL", "LEVEL {0}", "LEVEL {0}"), ("SONAR_ACTIVE", "SONAR ACTIEF", "SONAR ACTIVE"),
        ("BACK", "TERUG", "BACK"),
        ("PLAY_DESCRIPTION", "Begin een duik of sluit aan bij een sessie.", "Start a dive or join a session."),
        ("HOST", "DUIK STARTEN", "START A DIVE"), ("JOIN", "AANSLUITEN", "JOIN SESSION"),
        ("VESSEL", "VERKENNER", "EXPLORER"),
        ("SUBMARINE_DESCRIPTION", "Je onderzeeër is klaar voor de diepte.\nUpgrades volgen in een latere versie.", "Your submarine is ready for the depths.\nUpgrades will arrive in a future update."),
        ("LOGBOOK_EMPTY", "DE DIEPTE WACHT", "THE DEPTHS AWAIT"),
        ("LOGBOOK_DESCRIPTION", "Hier komen je ontdekkingen en duiken.\nHet logboek wordt later aangesloten.", "Your discoveries and dives will appear here.\nThe logbook will be connected in a future update."),
        ("LANGUAGE", "TAAL", "LANGUAGE"), ("LANGUAGE_AUTO", "Automatisch (systeemtaal)", "Automatic (system language)"),
        ("AUDIO", "GELUID", "AUDIO"), ("SOUND_ENABLED", "GELUID AAN", "SOUND ENABLED"),
        ("MASTER_VOLUME", "MASTERVOLUME", "MASTER VOLUME"),
        ("MUSIC_VOLUME", "ACHTERGRONDMUZIEK", "BACKGROUND MUSIC"),
        ("EFFECTS_VOLUME", "GELUIDSEFFECTEN", "SOUND EFFECTS"),
        ("FULLSCREEN_ON", "VOLLEDIG SCHERM: AAN", "FULLSCREEN: ON"),
        ("FULLSCREEN_OFF", "VOLLEDIG SCHERM: UIT", "FULLSCREEN: OFF"),
        ("QUIT_DESCRIPTION", "Wil je Echo afsluiten?", "Would you like to quit Echo?")
    };
}
