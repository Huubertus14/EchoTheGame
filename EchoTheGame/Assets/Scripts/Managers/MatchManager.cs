using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using Project.Echo.Player;

public struct MatchStatus
{
	public float TimeLeft;
	public float PreGameTime;
	public bool GameOver;
	public bool GameStarted;
}

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

	[SerializeField] private GameObject _gameOverPanel;
	[SerializeField] private TextMeshProUGUI _winnerText;
	[SerializeField] private GameModeTimerUI _gameModeTimerUI;

	public bool IsGameStarted;
	public bool IsGameOver;

	public float TimeLeft { get; set; }
	public float PreGameTime { get;  set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}

		_gameOverPanel.SetActive(false);
	}

	public MatchStatus GetMatchData()
	{
		var matchStatus = new MatchStatus
		{
			TimeLeft = TimeLeft,
			PreGameTime = PreGameTime,
			GameStarted = IsGameStarted,
			GameOver = IsGameOver
		};
		return matchStatus;
	}

	public  void ShowEndScreen()
	{
		_gameModeTimerUI.enabled = false;
		_winnerText.text = HasWon() ? "Winner!" : "Loser";
		_gameOverPanel.SetActive(true);
	}

	private bool HasWon()
	{
		var sortedPlayer = PlayerList.Instance.GetSortedPlayerList();

		if (sortedPlayer.Count > 0 && PlayerNetworkedController.LocalPlayer.PlayerName == sortedPlayer[0].PlayerName)
		{
			return true;
		}

		return false;
	}
}
