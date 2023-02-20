using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Project.Echo.Loading;
using System;

public class NetworkedPlayerGameModeController : NetworkBehaviour
{
	public bool SkipInit;

	[Networked]
	private TickTimer _gameTimer { get; set; }

	[Networked(OnChanged =nameof(TimeChanged))]
	private float _TimeLeft { get; set; }

	public override void Spawned()
	{
		if (HasInputAuthority&& !SkipInit)
		{
			if (HasStateAuthority)
			{
				_gameTimer = TickTimer.CreateFromSeconds(Runner, 120);
				MatchManager.Instance.IsGameStarted = true;
				LoadScreenController.Hide();
			}
		}
		MatchManager.Instance.IsGameStarted = true;
		LoadScreenController.Hide();
	}

	private static void TimeChanged(Changed<NetworkedPlayerGameModeController> changed)
	{
		MatchManager.Instance.TimeLeft = changed.Behaviour._TimeLeft;
	}

	public override void FixedUpdateNetwork()
	{
		if (HasInputAuthority && HasStateAuthority)
		{
			if (!_gameTimer.ExpiredOrNotRunning(Runner) && _gameTimer.IsRunning)
			{
				_TimeLeft = _gameTimer.RemainingTime(Runner).Value;
			}
		}
	}

	public void SetMatchValues(bool item1, float item2)
	{
		_gameTimer = TickTimer.CreateFromSeconds(Runner, item2);
		MatchManager.Instance.IsGameStarted = item1;
	}
}
