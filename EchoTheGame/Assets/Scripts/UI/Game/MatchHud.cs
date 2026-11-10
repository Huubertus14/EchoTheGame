using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchHud : MonoBehaviour
{
    [SerializeField] private TMP_Text _killsText;
    [SerializeField] private TMP_Text _timeLabel;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _rivalLabel;
    [SerializeField] private TMP_Text _rivalText;
    private readonly List<PlayerScoreboardController> _players = new();
    private float _nextRefresh;

    private void OnEnable() => _nextRefresh = 0;

    private void Update()
    {
        if (Time.unscaledTime < _nextRefresh) return;
        _nextRefresh = Time.unscaledTime + 0.15f;
        PlayerScoreboardController.GetSortedPlayers(_players);
        var local = _players.Find(player => player.HasInputAuthority);
        _killsText.text = local ? local.Kills.ToString() : "--";
        var match = MatchManager.Instance;
        _timeLabel.text = match && match.IsGameOver ? "AFGELOPEN" :
            match && match.IsGameStarted ? "RESTEREND" : "START OVER";
        int seconds = match && !match.IsGameOver
            ? Mathf.Max(0, Mathf.CeilToInt(match.IsGameStarted ? match.TimeLeft : match.PreGameTime)) : 0;
        bool unlimited = match && match.IsGameStarted && !match.IsGameOver && match.TimeLeft < 0;
        if (unlimited) _timeLabel.text = "GEEN LIMIET";
        _timeText.text = unlimited ? "∞" : match ? $"{seconds / 60:00}:{seconds % 60:00}" : "--:--";
        bool leading = local && _players.Count > 0 && _players[0] == local;
        var rival = _players.Count > (leading ? 1 : 0) ? _players[leading ? 1 : 0] : null;
        _rivalLabel.text = leading ? "NUMMER 2" : "KOPLOPER";
        _rivalText.text = rival ? rival.Score.ToString() : "--";
    }
}
