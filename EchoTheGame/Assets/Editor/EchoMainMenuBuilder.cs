using System;
using System.IO;
using System.Linq;
using Project.Echo.Menu;
using Project.Echo.Menu.Test;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Authors real scene objects, not a runtime screenshot of a menu.</summary>
public static class EchoMainMenuBuilder
{
    private const string Root = "Assets/UI/Echo";
    private static EchoMenuTheme theme;

    [MenuItem("Echo/Build Main Menu %#m")]
    public static void Build()
    {
        Directory.CreateDirectory(Root + "/Icons");
        AssetDatabase.Refresh();
        theme = AssetDatabase.LoadAssetAtPath<EchoMenuTheme>(Root + "/EchoMenuTheme.asset");
        if (theme == null)
        {
            theme = ScriptableObject.CreateInstance<EchoMenuTheme>();
            AssetDatabase.CreateAsset(theme, Root + "/EchoMenuTheme.asset");
        }
        theme.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        theme.lightTextMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/EchoLightText.mat");
        if (theme.lightTextMaterial == null)
        {
            theme.lightTextMaterial = new Material(theme.font.material) { name = "Echo Light Text" };
            AssetDatabase.CreateAsset(theme.lightTextMaterial, Root + "/EchoLightText.mat");
        }
        theme.lightTextMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, -0.12f);
        EditorUtility.SetDirty(theme.lightTextMaterial);
        theme.sonar = MakeIcon("Sonar", (x, y) =>
        {
            float r = new Vector2(x - 64, y - 64).magnitude;
            return Mathf.Abs(r - 57) < 0.8f || Mathf.Abs(r - 52) < 1.5f || Mathf.Abs(r - 47) < 0.8f || r < 4;
        });
        theme.submarine = MakeIcon("SubmarineBadge", (x, y) =>
        {
            float r = new Vector2(x - 64, y - 64).magnitude;
            bool hull = Mathf.Pow((x - 65) / 37f, 2) + Mathf.Pow((y - 57) / 10f, 2) < 1;
            return Mathf.Abs(r - 59) < 1 || hull || Box(x, y, 53, 64, 70, 77) || Box(x, y, 62, 76, 65, 90) || Box(x, y, 28, 50, 34, 65);
        });
        theme.currency = MakeIcon("Currency", (x, y) =>
        {
            float dx = Mathf.Abs(x - 64), dy = Mathf.Abs(y - 64);
            float edge = Mathf.Max(dx * 0.8660254f + dy * 0.5f, dy);
            return Mathf.Abs(edge - 49) < 1.5f || edge < 22;
        });
        theme.arrow = MakeIcon("Arrow", (x, y) => Line(x, y, 48, 33, 78, 64, 3.5f) || Line(x, y, 78, 64, 48, 95, 3.5f));
        theme.signal = MakeIcon("Signal", (x, y) => Box(x, y, 13, 18, 34, 62) || Box(x, y, 51, 18, 72, 84) || Box(x, y, 89, 18, 110, 108));
        theme.roundedButton = MakeIcon("RoundedButton", (x, y) =>
        {
            Vector2 p = new Vector2(Mathf.Abs(x - 64), Mathf.Abs(y - 64)) - new Vector2(47, 47);
            return new Vector2(Mathf.Max(p.x, 0), Mathf.Max(p.y, 0)).magnitude + Mathf.Min(Mathf.Max(p.x, p.y), 0) < 16;
        }, new Vector4(24, 24, 24, 24));
        EditorUtility.SetDirty(theme);
        AssetDatabase.SaveAssets();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = theme.abyss;
        cameraObject.transform.position = new Vector3(0, 0, -10);
        var events = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var canvasObject = new GameObject("Echo Main Menu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MenuController));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        MenuController controller = canvasObject.GetComponent<MenuController>();

        // Centered 16:9 safe frame. Background independently covers every aspect ratio.
        RectTransform backdrop = Stretch("Backdrop - Replace with live ocean", canvas.transform);
        Image ocean = Picture("Ocean artwork", backdrop, ImportSprite(Root + "/Textures/MenuOcean.png", false), Color.white, 0, 0, 1920, 1080);
        StretchRect(ocean.rectTransform);
        ocean.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        var cover = ocean.gameObject.AddComponent<AspectRatioFitter>();
        cover.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        cover.aspectRatio = (float)ocean.sprite.texture.width / ocean.sprite.texture.height;
        // Full-height fade keeps cropped sonar terrain readable without a visible panel edge.
        Image shade = Picture("Navigation readability fade", backdrop, MakeReadabilityFade(), Color.white, 0, 0, 0, 0);
        StretchRect(shade.rectTransform);
        RectTransform ui = Rect("Interface", canvas.transform, 0, 0, 1920, 1080);
        ui.anchorMin = ui.anchorMax = new Vector2(0.5f, 0.5f);
        ui.pivot = new Vector2(0.5f, 0.5f);
        ui.anchoredPosition = Vector2.zero;
        CanvasGroup navigation = ui.gameObject.AddComponent<CanvasGroup>();

        Label("Title", ui, "E C H", 103, 71, 458, 154, 140, theme.seafoam, 2);
        Picture("Sonar O", ui, theme.sonar, theme.seafoam, 561, 88, 132, 132);
        Label("Subtitle", ui, "LUISTER NAAR DE DIEPTE", 108, 238, 640, 34, 23, theme.muted, 12);
        Button play = MakeButton("Spelen", ui, "SPELEN", 103, 340, 425, 80, true, controller.ShowPlay);
        Picture("Play arrow", play.transform, theme.arrow, theme.abyss, 366, 23, 30, 34);
        Button sub = MakeButton("Onderzeeër", ui, "ONDERZEEËR", 103, 447, 425, 68, false, controller.ShowSubmarine);
        Button log = MakeButton("Logboek", ui, "LOGBOEK", 103, 524, 425, 68, false, controller.ShowLogbook);
        Button settings = MakeButton("Instellingen", ui, "INSTELLINGEN", 103, 601, 425, 68, false, controller.ShowSettings);
        Button quit = MakeButton("Afsluiten", ui, "AFSLUITEN", 103, 678, 425, 68, false, controller.ShowQuit);
        LinkNavigation(new[] { play, sub, log, settings, quit });

        Picture("Captain badge", ui, theme.submarine, theme.seafoam, 1394, 31, 84, 84);
        Label("Captain", ui, "KAPITEIN", 1492, 34, 180, 22, 15, theme.muted, 8);
        TMP_Text level = Label("Level", ui, "LEVEL 01", 1492, 62, 170, 27, 20, theme.seafoam, 6);
        Image xpTrack = Picture("XP track", ui, theme.roundedButton, new Color32(24, 52, 64, 255), 1492, 95, 188, 7);
        xpTrack.type = Image.Type.Sliced;
        Image xp = Picture("XP fill", ui, theme.roundedButton, theme.seafoam, 1492, 95, 188, 7);
        xp.type = Image.Type.Filled;
        xp.fillMethod = Image.FillMethod.Horizontal;
        xp.fillAmount = 0;
        Picture("Profile divider", ui, null, new Color32(82, 112, 119, 170), 1710, 39, 1, 67);
        Picture("Currency icon", ui, theme.currency, theme.amber, 1736, 49, 45, 45);
        TMP_Text money = Label("Currency balance", ui, "0", 1795, 48, 100, 42, 29, new Color32(225, 240, 239, 255), 2);
        Label("Sonar status", ui, "SONAR ACTIEF", 1602, 994, 228, 24, 17, theme.seafoam, 6);
        Picture("Signal icon", ui, theme.signal, theme.seafoam, 1822, 972, 52, 52);

        RectTransform modal = Stretch("Modal layer", canvas.transform);
        Image scrim = Picture("Modal scrim", modal, null, new Color(0, 0.025f, 0.04f, 0.86f), 0, 0, 0, 0);
        StretchRect(scrim.rectTransform);
        scrim.raycastTarget = true;
        RectTransform window = Rect("Window", modal, 0, 0, 760, 560);
        window.anchorMin = window.anchorMax = new Vector2(0.5f, 0.5f);
        window.pivot = new Vector2(0.5f, 0.5f);
        window.anchoredPosition = Vector2.zero;
        Picture("Window surface", window, theme.roundedButton, theme.panel, 0, 0, 760, 560).type = Image.Type.Sliced;
        Picture("Sonar accent", window, null, theme.seafoam, 48, 40, 48, 2);
        string[] names = { "SPELEN", "ONDERZEEËR", "LOGBOEK", "INSTELLINGEN", "AFSLUITEN" };
        var pages = new GameObject[5];
        var firstButtons = new Button[5];
        Slider volume = null;
        TMP_Text volumeText = null, fullscreenText = null;
        for (int i = 0; i < names.Length; i++)
        {
            RectTransform page = Stretch(names[i], window);
            pages[i] = page.gameObject;
            Label("Heading", page, names[i], 48, 65, 660, 56, 34, theme.seafoam, 5);
            Button back = MakeButton("Terug", page, "TERUG", 48, 459, 220, 58, false, controller.ClosePanel);
            if (i == 0)
            {
                Label("Description", page, "Begin een duik of sluit aan bij een sessie.", 48, 143, 660, 42, 22, theme.muted);
                Button host = MakeButton("Host", page, "DUIK STARTEN", 48, 229, 664, 72, true, null);
                host.gameObject.AddComponent<HostGameButton>();
                Button join = MakeButton("Join", page, "AANSLUITEN", 48, 320, 664, 72, false, null);
                join.gameObject.AddComponent<JoinGameButton>();
                firstButtons[i] = host;
                LinkNavigation(new[] { host, join, back });
            }
            else if (i == 1)
            {
                Picture("Submarine image", page, theme.submarine, theme.seafoam, 48, 170, 150, 150);
                Label("Vessel", page, "VERKENNER", 235, 190, 460, 44, 27, theme.seafoam, 4);
                Label("Description", page, "Je onderzeeër is klaar voor de diepte.\nUpgrades volgen in een latere versie.", 235, 247, 450, 120, 22, theme.muted);
                firstButtons[i] = back;
                LinkNavigation(new[] { back });
            }
            else if (i == 2)
            {
                Picture("Sonar image", page, theme.sonar, theme.seafoam, 48, 181, 112, 112);
                Label("Empty heading", page, "DE DIEPTE WACHT", 198, 186, 510, 42, 26, theme.seafoam, 3);
                Label("Description", page, "Hier komen je ontdekkingen en duiken.\nHet logboek wordt later aangesloten.", 198, 246, 510, 120, 22, theme.muted);
                firstButtons[i] = back;
                LinkNavigation(new[] { back });
            }
            else if (i == 3)
            {
                Label("Volume title", page, "MASTERVOLUME", 48, 166, 500, 34, 21, theme.muted, 4);
                volumeText = Label("Volume value", page, "80%", 600, 166, 112, 34, 22, theme.seafoam);
                RectTransform sliderRoot = Rect("Master volume", page, 48, 220, 664, 42);
                volume = sliderRoot.gameObject.AddComponent<Slider>();
                Image track = Picture("Track", sliderRoot, null, new Color32(36, 66, 77, 255), 0, 17, 664, 8);
                RectTransform fillArea = Rect("Fill area", sliderRoot, 10, 17, 644, 8);
                Image fill = Picture("Fill", fillArea, null, theme.seafoam, 0, 0, 644, 8);
                StretchRect(fill.rectTransform);
                RectTransform handleArea = Rect("Handle area", sliderRoot, 10, 0, 644, 42);
                Image handle = Picture("Handle", handleArea, theme.roundedButton, theme.seafoam, 0, 0, 20, 34);
                handle.rectTransform.anchorMin = handle.rectTransform.anchorMax = new Vector2(0, 0.5f);
                handle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                handle.rectTransform.anchoredPosition = Vector2.zero;
                volume.fillRect = fill.rectTransform;
                volume.handleRect = handle.rectTransform;
                handle.rectTransform.sizeDelta = new Vector2(20, -8);
                volume.targetGraphic = handle;
                volume.value = 0.8f;
                UnityEventTools.AddPersistentListener(volume.onValueChanged, controller.ApplyVolume);
                Button screen = MakeButton("Fullscreen", page, "VOLLEDIG SCHERM: UIT", 48, 315, 664, 64, false, controller.ToggleFullscreen);
                fullscreenText = screen.GetComponentInChildren<TMP_Text>();
                firstButtons[i] = screen;
                LinkNavigation(new Selectable[] { volume, screen, back });
            }
            else
            {
                Label("Description", page, "Wil je Echo afsluiten?", 48, 164, 664, 60, 26, theme.muted);
                Button exit = MakeButton("Bevestig afsluiten", page, "AFSLUITEN", 48, 294, 664, 72, true, controller.QuitGame);
                firstButtons[i] = back;
                LinkNavigation(new[] { exit, back });
            }
            page.gameObject.SetActive(false);
        }
        var so = new SerializedObject(controller);
        so.FindProperty("persistentManagersPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers/PersistentManager.prefab");
        so.FindProperty("modal").objectReferenceValue = modal.gameObject;
        SetArray(so, "pages", pages);
        SetArray(so, "firstPageButtons", firstButtons);
        so.FindProperty("playButton").objectReferenceValue = play;
        so.FindProperty("mainNavigation").objectReferenceValue = navigation;
        so.FindProperty("levelLabel").objectReferenceValue = level;
        so.FindProperty("currencyLabel").objectReferenceValue = money;
        so.FindProperty("experienceFill").objectReferenceValue = xp;
        so.FindProperty("volumeSlider").objectReferenceValue = volume;
        so.FindProperty("volumeLabel").objectReferenceValue = volumeText;
        so.FindProperty("fullscreenLabel").objectReferenceValue = fullscreenText;
        so.ApplyModifiedPropertiesWithoutUndo();
        events.GetComponent<EventSystem>().firstSelectedGameObject = play.gameObject;
        modal.gameObject.SetActive(false);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        AssetDatabase.SaveAssets();
        ValidateAndPreview();
        Debug.Log("ECHO_MENU_BUILD_OK");
    }

    private static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(w, h);
        return rect;
    }

    private static RectTransform Stretch(string name, Transform parent)
    {
        RectTransform rect = Rect(name, parent, 0, 0, 0, 0);
        StretchRect(rect);
        return rect;
    }

    private static void StretchRect(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static Image Picture(string name, Transform parent, Sprite sprite, Color color, float x, float y, float w, float h)
    {
        Image image = Rect(name, parent, x, y, w, h).gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text Label(string name, Transform parent, string text, float x, float y, float w, float h, float size, Color color, float tracking = 0)
    {
        var label = Rect(name, parent, x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = theme.font;
        label.fontSharedMaterial = theme.lightTextMaterial;
        label.text = text;
        label.fontSize = size;
        label.characterSpacing = tracking * 4;
        label.color = color;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;
        return label;
    }

    private static Button MakeButton(string name, Transform parent, string text, float x, float y, float w, float h, bool primary, UnityAction action)
    {
        Image surface = Picture(name, parent, theme.roundedButton, Color.white, x, y, w, h);
        surface.type = Image.Type.Sliced;
        surface.raycastTarget = true;
        var button = surface.gameObject.AddComponent<Button>();
        button.targetGraphic = surface;
        var colors = button.colors;
        colors.normalColor = primary ? theme.seafoam : new Color(0.08f, 0.2f, 0.24f, 0);
        colors.highlightedColor = primary ? new Color(0.77f, 1, 0.97f, 1) : new Color(0.19f, 0.45f, 0.49f, 0.45f);
        colors.selectedColor = primary ? new Color(0.67f, 0.99f, 0.95f, 1) : new Color(0.19f, 0.45f, 0.49f, 0.5f);
        colors.pressedColor = primary ? new Color(0.39f, 0.75f, 0.74f, 1) : new Color(0.2f, 0.5f, 0.52f, 0.7f);
        colors.fadeDuration = 0.13f;
        button.colors = colors;
        Label("Label", button.transform, text, 40, 0, w - 70, h, primary ? 29 : 27, primary ? theme.abyss : theme.muted, 6);
        if (action != null) UnityEventTools.AddPersistentListener(button.onClick, action);
        return button;
    }

    private static void LinkNavigation(Selectable[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            Navigation nav = items[i].navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = items[(i + items.Length - 1) % items.Length];
            nav.selectOnDown = items[(i + 1) % items.Length];
            items[i].navigation = nav;
        }
    }

    private static void SetArray(SerializedObject so, string name, UnityEngine.Object[] values)
    {
        var p = so.FindProperty(name);
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static bool Box(float x, float y, float x1, float y1, float x2, float y2) => x >= x1 && x <= x2 && y >= y1 && y <= y2;

    private static bool Line(float x, float y, float ax, float ay, float bx, float by, float thickness)
    {
        Vector2 p = new Vector2(x - ax, y - ay), d = new Vector2(bx - ax, by - ay);
        return (p - d * Mathf.Clamp01(Vector2.Dot(p, d) / d.sqrMagnitude)).magnitude < thickness;
    }

    private static Sprite MakeIcon(string name, Func<float, float, bool> paint, Vector4 border = default)
    {
        // Four samples per axis keep the small PNG silhouettes smooth at UI scale.
        var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        var pixels = new Color32[128 * 128];
        for (int y = 0; y < 128; y++)
        for (int x = 0; x < 128; x++)
        {
            int coverage = 0;
            for (int sy = 0; sy < 4; sy++)
            for (int sx = 0; sx < 4; sx++)
                if (paint(x + (sx + 0.5f) / 4, y + (sy + 0.5f) / 4)) coverage++;
            pixels[y * 128 + x] = new Color32(255, 255, 255, (byte)(255 * coverage / 16));
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        string path = Root + "/Icons/" + name + ".png";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        Sprite sprite = ImportSprite(path, true, border);
        return sprite;
    }

    private static Sprite ImportSprite(string path, bool icon, Vector4 border = default)
    {
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = border;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.maxTextureSize = 2048;
        importer.textureCompression = icon ? TextureImporterCompression.Uncompressed : TextureImporterCompression.CompressedHQ;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite MakeReadabilityFade()
    {
        var texture = new Texture2D(256, 2, TextureFormat.RGBA32, false);
        for (int x = 0; x < 256; x++)
        {
            Color color = new Color(0.008f, 0.047f, 0.075f, 0.65f * Mathf.Pow(Mathf.Clamp01(1 - x / 127.5f), 1.3f));
            texture.SetPixel(x, 0, color);
            texture.SetPixel(x, 1, color);
        }
        texture.Apply();
        string path = Root + "/Textures/NavigationFade.png";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        return ImportSprite(path, true);
    }

    [MenuItem("Echo/Validate Main Menu")]
    public static void ValidateAndPreview()
    {
        var controller = UnityEngine.Object.FindAnyObjectByType<MenuController>();
        if (controller == null) throw new Exception("Main menu controller missing.");
        var so = new SerializedObject(controller);
        foreach (string name in new[] { "persistentManagersPrefab", "modal", "playButton", "mainNavigation", "levelLabel", "currencyLabel", "experienceFill", "volumeSlider", "volumeLabel", "fullscreenLabel" })
            if (so.FindProperty(name).objectReferenceValue == null) throw new Exception("Unbound menu field: " + name);
        Button[] buttons = controller.GetComponentsInChildren<Button>(true);
        if (buttons.Length != 14) throw new Exception("Expected 14 real buttons, got " + buttons.Length);
        foreach (Button button in buttons)
            if (button.onClick.GetPersistentEventCount() == 0 && button.GetComponent<BaseGameButton>() == null)
                throw new Exception("Unbound button: " + button.name);
        var modal = (GameObject)so.FindProperty("modal").objectReferenceValue;
        Action[] open = { controller.ShowPlay, controller.ShowSubmarine, controller.ShowLogbook, controller.ShowSettings, controller.ShowQuit };
        for (int i = 0; i < open.Length; i++)
        {
            open[i]();
            if (!modal.activeSelf) throw new Exception("Modal did not open.");
            for (int j = 0; j < open.Length; j++)
                if (((GameObject)so.FindProperty("pages").GetArrayElementAtIndex(j).objectReferenceValue).activeSelf != (i == j))
                    throw new Exception("Modal page visibility incorrect.");
            controller.ClosePanel();
            if (modal.activeSelf) throw new Exception("Modal did not close.");
        }
        controller.SetProgress(1, 0, 0);
        if (controller.GetComponentsInChildren<TMP_Text>(true).Any(t => t.text.Contains("BESTE SCORE")))
            throw new Exception("Best score should not be present.");
        RenderPreview(1920, 1080, "../docs/main-menu-preview.png");
        RenderPreview(1280, 1024, "../docs/main-menu-preview-5x4.png");
        controller.ShowSettings();
        RenderPreview(1920, 1080, "../docs/main-menu-settings-preview.png");
        controller.ClosePanel();
        Debug.Log("ECHO_MENU_VALIDATION_OK: 14 buttons, 5 panels, bindings, progress, separate assets, 16:9 and 5:4 renders.");
    }

    private static void RenderPreview(int width, int height, string path)
    {
        Canvas canvas = UnityEngine.Object.FindAnyObjectByType<MenuController>().GetComponent<Canvas>();
        Camera camera = Camera.main;
        var target = new RenderTexture(width, height, 24);
        target.Create();
        camera.targetTexture = target;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1;
        Canvas.ForceUpdateCanvases();
        foreach (var text in canvas.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate();
        camera.Render();
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = target;
        var capture = new Texture2D(width, height, TextureFormat.RGB24, false);
        capture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        capture.Apply();
        File.WriteAllBytes(path, capture.EncodeToPNG());
        RenderTexture.active = previous;
        camera.targetTexture = null;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        UnityEngine.Object.DestroyImmediate(capture);
        target.Release();
        UnityEngine.Object.DestroyImmediate(target);
    }
}
