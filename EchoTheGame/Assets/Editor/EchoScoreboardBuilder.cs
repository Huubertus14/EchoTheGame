using System;
using System.IO;
using Project.Echo.Menu;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class EchoScoreboardBuilder
{
    private static EchoMenuTheme theme;
    private static readonly float[] X = { 22, 90, 510, 648, 750, 856, 980 };
    private static readonly float[] W = { 56, 404, 120, 86, 90, 108, 128 };

    [MenuItem("Echo/Build Game Scoreboard")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        theme = AssetDatabase.LoadAssetAtPath<EchoMenuTheme>("Assets/UI/Echo/EchoMenuTheme.asset");
        if (!theme) throw new InvalidOperationException("Missing Echo UI theme.");
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
        // Replace only the old screen UI. World-space hull meters stay on the players.
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
            if (canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace) Object.DestroyImmediate(canvas.gameObject);
        foreach (var menu in Object.FindObjectsByType<MenuController>(FindObjectsInactive.Include))
            Object.DestroyImmediate(menu.gameObject);

        var root = new GameObject("Scoreboard", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(PlayerList));
        root.layer = 5;
        var canvasRoot = root.GetComponent<Canvas>();
        canvasRoot.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasRoot.sortingOrder = 20;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        var overlay = Rect("Scoreboard / Hold Tab", root.transform, 0, 0, 1920, 1080);
        Stretch(overlay);
        var shade = overlay.gameObject.AddComponent<Image>();
        shade.color = new Color(theme.abyss.r, theme.abyss.g, theme.abyss.b, 0.62f);
        shade.raycastTarget = false;
        var panel = Rect("Score panel", overlay, 0, 0, 1160, 550);
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;
        Fill(panel, theme.panel);
        Text("Title", panel, "Score", 32, theme.seafoam, 32, 24, 600, 50);
        var match = Text("Match state and timer", panel, "FREE FOR ALL   00:00", 18, theme.seafoam, 798, 32, 330, 36);
        match.alignment = TextAlignmentOptions.MidlineRight;
        var labels = new[] { "#", "Naam", "SCORE", "KILLS", "DEATHS", "K/D", "PING" };
        var header = Rect("Column labels", panel, 24, 96, 1112, 36);
        for (int i = 0; i < labels.Length; i++)
            Text(labels[i], header, labels[i], 15, theme.muted, X[i], 0, W[i], 36).alignment = i == 1 ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;
        Line(panel, 32, 140, 1096, 1, new Color(theme.seafoam.r, theme.seafoam.g, theme.seafoam.b, 0.25f));
        var viewport = Rect("Rows / Clipped viewport", panel, 24, 154, 1112, 368);
        viewport.gameObject.AddComponent<RectMask2D>();
        var content = Rect("Editable player rows", viewport, 0, 0, 1112, 0);
        content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
        content.sizeDelta = Vector2.zero;
        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6; layout.childControlHeight = true; layout.childControlWidth = true;
        layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped; scroll.inertia = false;
        var template = BuildRow(content);
        template.gameObject.SetActive(false);
        var empty = Text("Empty state", viewport, "Geen spelers", 20, theme.muted, 32, 28, 1040, 60);
        var controller = root.GetComponent<PlayerList>();
        Set(controller, "_overlay", overlay.gameObject); Set(controller, "_listItemTemplate", template);
        Set(controller, "_content", content); Set(controller, "_scroll", scroll);
        Set(controller, "_matchText", match); Set(controller, "_emptyText", empty);
        overlay.gameObject.SetActive(false);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        ValidateAndPreview(controller, overlay, template);
        Debug.Log("ECHO_SCOREBOARD_OK: Game scene saved; editable UI, theme bindings, hidden overlay, 16:9 and 5:4 previews.");
    }

    private static PlayerListItem BuildRow(Transform parent)
    {
        var row = Rect("Player row template", parent, 0, 0, 1112, 56);
        var background = Fill(row, theme.panel);
        row.gameObject.AddComponent<LayoutElement>().preferredHeight = 56;
        var stripe = Rect("Local contact stripe", row, 0, 0, 3, 56); Fill(stripe, theme.seafoam);
        var item = row.gameObject.AddComponent<PlayerListItem>();
        Set(item, "_theme", theme); Set(item, "_background", background); Set(item, "_localStripe", stripe.gameObject);
        string[] fields = { "_rankText", "_playerNameText", "_scoreText", "_killsText", "_deathsText", "_ratioText", "_pingText" };
        for (int i = 0; i < fields.Length; i++)
        {
            var label = Text(fields[i], row, i == 1 ? "Connecting..." : "0", i == 1 ? 22 : 21, i == 2 ? theme.seafoam : theme.muted, X[i], 0, W[i], 56);
            label.alignment = i == 1 ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;
            label.overflowMode = TextOverflowModes.Ellipsis;
            Set(item, fields[i], label);
        }
        return item;
    }
    private static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.layer = 5; go.transform.SetParent(parent, false);
        var r = (RectTransform)go.transform; r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); return r;
    }
    private static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
    private static Image Fill(RectTransform r, Color color) { var image = r.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false; return image; }
    private static void Line(Transform parent, float x, float y, float w, float h, Color color) => Fill(Rect("Instrument rule", parent, x, y, w, h), color);
    private static TMP_Text Text(string name, Transform parent, string value, int size, Color color, float x, float y, float w, float h)
    {
        var text = Rect(name, parent, x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value; text.font = theme.font; text.fontSharedMaterial = theme.lightTextMaterial;
        text.fontSize = size; text.color = color; text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.alignment = TextAlignmentOptions.MidlineLeft; return text;
    }
    private static void Set(Object target, string field, Object value)
    {
        var so = new SerializedObject(target); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
    }
    private static void ValidateAndPreview(PlayerList controller, RectTransform overlay, PlayerListItem template)
    {
        if (overlay.gameObject.activeSelf || !theme || controller.GetComponentsInChildren<Button>(true).Length != 0)
            throw new Exception("Scoreboard visibility/theme/input validation failed.");
        foreach (var graphic in controller.GetComponentsInChildren<Graphic>(true))
            if (graphic.raycastTarget) throw new Exception("Scoreboard must not intercept gameplay input.");
        overlay.gameObject.SetActive(true);
        var previews = new System.Collections.Generic.List<GameObject>();
        string[] names = { "Nautilus", "Echo", "Silent Current", "Abyss Walker", "Echo", "Very long callsign that should truncate cleanly" };
        for (int i = 0; i < names.Length; i++)
        {
            var row = Object.Instantiate(template, template.transform.parent); row.gameObject.SetActive(true);
            row.Show(i + 1, names[i], 1200 - i * 150, 12 - i, 2 + i, i == 3 ? 185 : i == 5 ? -1 : 24 + i * 9, i == 1);
            previews.Add(row.gameObject);
        }
        var empty = new SerializedObject(controller).FindProperty("_emptyText").objectReferenceValue as TMP_Text;
        empty.gameObject.SetActive(false);
        Render(controller.GetComponent<Canvas>(), 1920, 1080, "../docs/scoreboard-preview.png");
        Render(controller.GetComponent<Canvas>(), 1280, 1024, "../docs/scoreboard-preview-5x4.png");
        foreach (var row in previews) Object.DestroyImmediate(row);
        empty.gameObject.SetActive(true); overlay.gameObject.SetActive(false);
        // Preview-only sample data must never be saved into the scene.
        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        EditorSceneManager.SaveScene(controller.gameObject.scene);
    }
    private static void Render(Canvas canvas, int width, int height, string path)
    {
        var camera = Camera.main; var oldTarget = camera.targetTexture;
        var target = new RenderTexture(width, height, 24); target.Create(); camera.targetTexture = target;
        canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
        Canvas.ForceUpdateCanvases();
        foreach (var text in canvas.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate();
        camera.Render(); var previous = RenderTexture.active; RenderTexture.active = target;
        var capture = new Texture2D(width, height, TextureFormat.RGB24, false);
        capture.ReadPixels(new Rect(0, 0, width, height), 0, 0); capture.Apply(); File.WriteAllBytes(path, capture.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = oldTarget;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null;
        Object.DestroyImmediate(capture); target.Release(); Object.DestroyImmediate(target);
    }
}
