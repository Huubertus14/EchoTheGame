using System.Globalization;
using Project.Echo.Menu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField] private EchoMenuTheme _theme;
    [SerializeField] private Image _background;
    [SerializeField] private GameObject _localStripe;
    [SerializeField] private TMP_Text _rankText;
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _killsText;
    [SerializeField] private TMP_Text _deathsText;
    [SerializeField] private TMP_Text _ratioText;
    [SerializeField] private TMP_Text _pingText;

    public void Bind(PlayerScoreboardController player, int rank)
    {
        Show(rank, player.GetPlayerName, player.Score, player.Kills, player.Deaths, player.PingMilliseconds, player.HasInputAuthority);
    }
    public void Show(int rank, string name, int score, int kills, int deaths, int ping, bool local)
    {
        _rankText.text = rank.ToString("00");
        _rankText.color = rank == 1 ? _theme.amber : _theme.muted;
        _playerNameText.richText = false;
        _playerNameText.text = string.IsNullOrWhiteSpace(name) ? "Connecting..." : name;
        _playerNameText.color = local ? _theme.seafoam : _theme.muted;
        _background.color = new Color(_theme.seafoam.r, _theme.seafoam.g, _theme.seafoam.b, local ? 0.13f : rank % 2 == 0 ? 0.035f : 0.015f);
        _localStripe.SetActive(local);
        _scoreText.text = score.ToString();
        _killsText.text = kills.ToString();
        _deathsText.text = deaths.ToString();
        _ratioText.text = ((float)kills / Mathf.Max(1, deaths)).ToString("0.00", CultureInfo.InvariantCulture);
        _pingText.text = ping < 0 ? "--" : $"{ping} ms";
        _pingText.color = ping >= 150 ? _theme.amber : _theme.muted;
    }
}
