using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Project.Echo.Player;

public class PlayerListItem : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _playerNameText;
	[SerializeField] private TextMeshProUGUI _scoreText;
	[SerializeField] private TextMeshProUGUI _killsText;
	[SerializeField] private TextMeshProUGUI _deathsText;

	public string PlayerName { get;private set; }
	public int GetScore { get; private set; }

	private int _kills;
	private int _deaths;

	public void Init(string playerName)
	{
		PlayerName = playerName;
		if (PlayerNetworkedController.LocalPlayer.PlayerName == playerName)
		{
			_playerNameText.text = $"<b>{PlayerName}</b>";
		}
		else
		{
			_playerNameText.text = PlayerName;
		}
		_killsText.text = "0";
		_deathsText.text = "0";
		_scoreText.text = "0";
	}

	public void UpdateScore(int newScore)
	{
		GetScore = newScore;
		_scoreText.text = GetScore.ToString();
	}

	public void UpdateKills(int newKills)
	{
		_kills = newKills;
		_killsText.text = _kills.ToString();
	}

	public void UpdateDeaths(int newDeaths)
	{
		_deaths = newDeaths;
		_deathsText.text = _deaths.ToString();
	}
}
