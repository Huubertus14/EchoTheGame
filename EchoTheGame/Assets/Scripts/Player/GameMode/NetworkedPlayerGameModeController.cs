using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Project.Echo.Loading;
using System;
using Cysharp.Threading.Tasks;

public class NetworkedPlayerGameModeController : NetworkBehaviour
{
	public bool SkipInit;

	private int _killLimit  =2; 
	private float _matchTimeToSet;

	[Networked(OnChanged =nameof(GameOverChanged))]
	private NetworkBool _gameOver { get; set; }

	[Networked(OnChanged = nameof(GameStartedChanged))]
	private NetworkBool _gameStarted { get; set; }

	[Networked]
	private TickTimer _preGameCountdown { get; set; }

	[Networked(OnChanged =nameof(PreGameTimerChanged))]
	public float _networkedPreGameTime { get; set; }

	[Networked]
	private TickTimer _gameTimer { get; set; }

	[Networked(OnChanged =nameof(GameTimeChanged))]
	public float _networkedTime { get; set; }

	public override void Spawned()
	{
		if (HasInputAuthority)
		{
			if (HasStateAuthority)
			{
				if (!SkipInit)
				{
					MatchManager.Instance.IsGameOver = false;
					MatchManager.Instance.IsGameStarted = false;
					_matchTimeToSet = 30;
					PlayerScoreboardController.ScoreChanged += OnScoreChanged;
				}

				_preGameCountdown = TickTimer.CreateFromSeconds(Runner, 5f);//Create pregame countdown
			}
		}
		LoadScreenController.Hide();
	}

	private static void PreGameTimerChanged(Changed<NetworkedPlayerGameModeController> changed)
	{
		MatchManager.Instance.PreGameTime = changed.Behaviour._networkedPreGameTime;
	}

	private static void GameTimeChanged(Changed<NetworkedPlayerGameModeController> changed)
	{
		MatchManager.Instance.TimeLeft = changed.Behaviour._networkedTime;
	}

	private static void GameStartedChanged(Changed<NetworkedPlayerGameModeController> changed)
	{
		MatchManager.Instance.IsGameStarted = changed.Behaviour._gameStarted;
	}

	private static void GameOverChanged(Changed<NetworkedPlayerGameModeController> changed)
	{
		MatchManager.Instance.IsGameOver = changed.Behaviour._gameOver;

		if (MatchManager.Instance.IsGameOver) //Game Over
		{
			MatchManager.Instance.ShowEndScreen();
			//Project.Echo.Player.PlayerNetworkedController.LocalPlayer.Runner.Shutdown();
		}
	}

	private void OnScoreChanged(int score, int kills)
	{
		if (kills >= _killLimit)
		{
			GameOver();
		}
	}

	public override void FixedUpdateNetwork()
	{
		if (HasInputAuthority && HasStateAuthority)
		{
			if (!_gameOver)
			{
				if (!_gameStarted && _preGameCountdown.IsRunning) //Match not started. Do countdown
				{
					_networkedPreGameTime = _preGameCountdown.RemainingTime(Runner).Value;

					if (_preGameCountdown.Expired(Runner))
					{
						StartGame();
					}
					return;
				}

				if (_gameStarted && _gameTimer.IsRunning)
				{
					_networkedTime = _gameTimer.RemainingTime(Runner).Value;

					if (_gameTimer.Expired(Runner))
					{
						GameOver();
					}
				}
			}
		}
	}

	private void StartGame()
	{
		_gameStarted = true; 
		_gameTimer = TickTimer.CreateFromSeconds(Runner, _matchTimeToSet); //TODO change to dynamic gamemode settings
		_preGameCountdown = TickTimer.None;
	}

	public void SetMatchValues(MatchStatus matchData)
	{
		_matchTimeToSet = matchData.TimeLeft;
		_gameStarted = false;
		_gameOver = matchData.GameOver;
		//_preGameCountdown = matchData.PreGameTime;
	}

	private void GameOver()
	{
		_gameStarted = false;
		_gameOver = true;
		_gameTimer = TickTimer.None;
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		if (HasStateAuthority)
		{
			PlayerScoreboardController.ScoreChanged -= OnScoreChanged;
		}
	}
}
