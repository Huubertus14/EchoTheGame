using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

	[SerializeField] private GameObject _gameOverPanel;
	[SerializeField] private TextMeshProUGUI _winnerText;
	[SerializeField] private GameModeTimerUI _gameModeTimerUI;

	public bool IsGameStarted;
	public bool IsGameOver;

	public float TimeLeft { get; set; }

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

	public (bool, float) GetMatchData()
	{
		return (IsGameStarted, TimeLeft);
	}

	public  void ShowEndScreen(bool hasWon)
	{
		_gameModeTimerUI.enabled = false;
		_winnerText.text = hasWon ? "Winner!" : "Loser";
		_gameOverPanel.SetActive(true);
	}
}
