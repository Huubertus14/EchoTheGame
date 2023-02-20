using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Project.Echo.Networking;
using Project.Echo.Player;
using Project.Echo.Loading;
using System;

public class FreeForAll : MonoBehaviour, IGameMode
{
	private const float _timeLimitInSeconds = 120;

	private int _killLimit = 3;
	private float _matchStartTimer = 2;

	private TickTimer _gameTimer { get; set; }

	private TickTimer _pregameTimer { get; set; }

	//[Networked(OnChanged =nameof(MatchStarted))]
	public NetworkBool IsGameStarted { get; set; }

	//[Networked(OnChanged = nameof(OnMatchDone))]
	private NetworkBool _isGameDone { get; set; }

	public bool IsSpawned { get; internal set; }

	public bool SkipInit;

	public void Spawned()
	{
		NetworkController.OnHostMigrationStarted += HostMigrationStarted;

		//When spawned //Wait until everything is loaded correctly and count down
	//	if (HasStateAuthority)
		{
			if (!SkipInit)
			{
			//	_pregameTimer = TickTimer.CreateFromSeconds(Runner, _matchStartTimer);
				_gameTimer = TickTimer.None;
			}
			PlayerScoreboardController.ScoreChanged += OnScoreChanged;
		}

		LoadScreenController.Hide();
		IsSpawned = true;
	}

	private void HostMigrationStarted(NetworkRunner obj)
	{
		NetworkController.OnHostMigrationStarted -= HostMigrationStarted;
		IsSpawned = false;
	}

	private void OnScoreChanged(int score, int kills)
	{
		EvaluateGameScore(score,kills);
	}

	private static void MatchStarted()//Changed<FreeForAll> changed)
	{
	/*	if (changed.Behaviour.IsGameStarted) //Game starts
		{
			Debug.Log("Game started");
			changed.Behaviour._gameTimer = TickTimer.CreateFromSeconds(changed.Behaviour.Runner, _timeLimitInSeconds);
			MatchManager.Instance.IsGameStarted = true;
		}*/
	}

	private static void OnMatchDone()//Changed<FreeForAll> changed)
	{
		/*if (changed.Behaviour._isGameDone) //Game is done
		{
			MatchManager.Instance.ShowEndScreen(changed.Behaviour.HasWon()); 
			MatchManager.Instance.IsGameStarted = false;
			MatchManager.Instance.IsGameOver = true;
			//changed.Behaviour.DisconnectPlayerFromRoom();
		}*/	
	}

	private bool HasWon()
	{
		var sortedPlayer = PlayerList.Instance.GetSortedPlayerList();

		if (sortedPlayer.Count>0 && PlayerNetworkedController.LocalPlayer.PlayerName == sortedPlayer[0].PlayerName)
		{
			return true;
		}

		return false;
	}

	public void FixedUpdateNetwork()
	{
		/*if (HasStateAuthority)
		{
			if (_pregameTimer.Expired(Runner))
			{
				IsGameStarted = true;
				_pregameTimer = TickTimer.None;
			}

			if (_gameTimer.Expired(Runner))
			{
				GameOver();
			}
		}*/
	}

	private void GameOver()
	{
		IsGameStarted = false;
		_isGameDone = true;
		_gameTimer = TickTimer.None;

		//if (HasStateAuthority)
		{
			PlayerScoreboardController.ScoreChanged -= OnScoreChanged;
		}
	}

	public float GetPreGameSecondsLeft()
	{
		//if (!_pregameTimer.ExpiredOrNotRunning(Runner))
		{
		//	return _pregameTimer.RemainingTime(Runner).Value;
		}
		return _matchStartTimer;
	}

	public float GetGameSecondsLeft()
	{
		//if (!_gameTimer.ExpiredOrNotRunning(Runner))
		{
		//	return _gameTimer.RemainingTime(Runner).Value;
		}
		return _timeLimitInSeconds;
	}

	public void EvaluateGameScore(int score, int kills)
	{
		if (kills >= _killLimit)
		{
			GameOver();
		}
	}
}
