using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerList : MonoBehaviour
{
    public static PlayerList Instance;
    [SerializeField] private GameObject _overlay;
    [SerializeField] private PlayerListItem _listItemTemplate;
    [SerializeField] private RectTransform _content;
    [SerializeField] private ScrollRect _scroll;
    [SerializeField] private TMP_Text _matchText;
    [SerializeField] private TMP_Text _emptyText;
    private readonly List<PlayerScoreboardController> _players = new();
    private readonly List<PlayerListItem> _rows = new();
    private float _nextRefresh;
    private bool _focused = true;

    private void Awake()
    {
        Instance = this;
        _listItemTemplate.gameObject.SetActive(false);
        _overlay.SetActive(false);
    }
    private void Update()
    {
        bool held = false;
#if ENABLE_INPUT_SYSTEM
        held = (Keyboard.current != null && Keyboard.current.tabKey.isPressed) ||
            (Gamepad.current != null && Gamepad.current.selectButton.isPressed);
#elif ENABLE_LEGACY_INPUT_MANAGER
        held = Input.GetKey(KeyCode.Tab) || Input.GetKey(KeyCode.JoystickButton6);
#endif
        bool visible = _focused && held;
        if (_overlay.activeSelf != visible)
        {
            _overlay.SetActive(visible);
            if (visible) { _nextRefresh = 0; _scroll.verticalNormalizedPosition = 1; }
        }
        if (!visible) return;
        if (Time.unscaledTime >= _nextRefresh)
        {
            _nextRefresh = Time.unscaledTime + 0.15f;
            Refresh();
        }
        // Scrolling never takes movement, aiming, cursor or EventSystem focus.
        float axis = 0;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
            axis = (Keyboard.current.downArrowKey.isPressed ? 1 : 0) - (Keyboard.current.upArrowKey.isPressed ? 1 : 0);
        if (Gamepad.current != null) axis += -Gamepad.current.rightStick.ReadValue().y;
        if (Mouse.current != null) axis -= Mouse.current.scroll.ReadValue().y * 0.02f;
#elif ENABLE_LEGACY_INPUT_MANAGER
        axis = (Input.GetKey(KeyCode.DownArrow) ? 1 : 0) - (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - Input.mouseScrollDelta.y;
#endif
        float overflow = _content.rect.height - _scroll.viewport.rect.height;
        if (overflow > 0) _scroll.verticalNormalizedPosition = Mathf.Clamp01(_scroll.verticalNormalizedPosition - axis * 400 * Time.unscaledDeltaTime / overflow);
    }
    public void Refresh()
    {
        PlayerScoreboardController.GetSortedPlayers(_players);
        while (_rows.Count < _players.Count)
            _rows.Add(Instantiate(_listItemTemplate, _content));
        for (int i = 0; i < _rows.Count; i++)
        {
            _rows[i].gameObject.SetActive(i < _players.Count);
            if (i < _players.Count) _rows[i].Bind(_players[i], i + 1);
        }
        _emptyText.gameObject.SetActive(_players.Count == 0);
        var match = MatchManager.Instance;
        int seconds = match ? Mathf.Max(0, Mathf.CeilToInt(match.IsGameStarted ? match.TimeLeft : match.PreGameTime)) : 0;
        _matchText.text = match && match.IsGameOver ? "MATCH COMPLETE" :
            $"{(match && match.IsGameStarted ? "FREE FOR ALL" : "STANDBY")}   {seconds / 60:00}:{seconds % 60:00}";
    }
    private void OnApplicationFocus(bool focused) { _focused = focused; if (!focused) _overlay.SetActive(false); }
    private void OnDisable() { if (_overlay) _overlay.SetActive(false); }
    private void OnDestroy() { if (Instance == this) Instance = null; }
}
