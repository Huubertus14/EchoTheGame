using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

public class FreeForAll : NetworkBehaviour, IGameMode
{
	private const float _timeLimitInSeconds = 180;
	private int _killLimit = 10;

	 private float _matchStartTimer = 5;

	[Networked]
	private TickTimer _gameTimer { get; set; }

	[Networked]
	private TickTimer _pregameTimer { get; set; }

	[Networked(OnChanged =nameof(MatchStarted))]
	public NetworkBool IsGameStarted { get; set; }

	[Networked(OnChanged = nameof(OnMatchDone))]
	public NetworkBool IsGameDone { get; set; }


	public bool IsSpawned { get; internal set; }

	public override void Spawned()
	{
		//When spawned //Wait until everything is loaded correctly and count down
		if (HasStateAuthority)
		{
			_pregameTimer = TickTimer.CreateFromSeconds(Runner,3f);
		}

		IsSpawned = true;
	}
	
	private static void MatchStarted(Changed<FreeForAll> changed)
	{
		if (changed.Behaviour.IsGameStarted) //Game starts
		{
			Debug.Log("Game started");
			changed.Behaviour._gameTimer = TickTimer.CreateFromSeconds(changed.Behaviour.Runner, _timeLimitInSeconds);
		}
		else //Game is done
		{

		}
	}

	private static void OnMatchDone(Changed<FreeForAll> changed)
	{
		
	}

	public override void FixedUpdateNetwork()
	{
		if (HasStateAuthority)
		{
			if (_pregameTimer.Expired(Runner))
			{
				IsGameStarted = true;
				_pregameTimer = TickTimer.None;
			}

			if (_gameTimer.Expired(Runner))
			{
				IsGameStarted = false;
				IsGameDone = true;
			}
		}
	}

	public float GetSecondsLeft()
	{
		if (!_gameTimer.ExpiredOrNotRunning(Runner))
		{
			return _gameTimer.RemainingTime(Runner).Value;
		}
		return -1;
	}

	public void EvaluateGameScore()
	{
		
	}
}
