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

		LoadScreenController.Hide();

		//If hose create values etc

		//If not host. Request values from host and set to

	}

	public override void FixedUpdateNetwork()
	{
		if (HasInputAuthority && HasStateAuthority)
		{
			if (!_gameTimer.ExpiredOrNotRunning(Runner) && _gameTimer.IsRunning)
			{
				//MatchManager.Instance.TimeLeft =_gameTimer.RemainingTime(Runner).Value;
				RPC_SendMatchData(MatchManager.Instance.IsGameStarted, _gameTimer.RemainingTime(Runner).Value);
			}
		}
	}

	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	private void RPC_SendMatchData(NetworkBool isGameStarted, float secondsLeft)
	{
			MatchManager.Instance.IsGameStarted = isGameStarted;
			MatchManager.Instance.TimeLeft = secondsLeft;
	}

	public void SetMatchValues(bool item1, float item2)
	{
		_gameTimer = TickTimer.CreateFromSeconds(Runner, item2);
		MatchManager.Instance.IsGameStarted = item1;
	}
}
