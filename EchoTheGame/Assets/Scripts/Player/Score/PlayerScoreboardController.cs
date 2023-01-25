using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Project.Echo.Setting;
using System;

public class PlayerScoreboardController : NetworkBehaviour
{
	private NetworkedKillFeedController _killFeedController;
	public static Action<int,int> ScoreChanged;

	[Networked(OnChanged = nameof(OnKillsChanged))]
	private int _matchKils { get; set; } = 0;

	[Networked(OnChanged = nameof(OnDeathsChanged))]
	private int _matchDeaths { get; set; } = 0;

	[Networked(OnChanged = nameof(OnScoreChanged))]
	private int _matchScore { get; set; } = 0;

	[Networked(OnChanged = nameof(OnNameChanged))]
	private NetworkString<_32> _playerName { get; set; }

	private bool _isNameMessageSend;

	public string GetPlayerName => _playerName.ToString();

	public bool SkipInitialization;

	private void Awake()
	{
		_killFeedController = GetComponent<NetworkedKillFeedController>();
	}

	public override void Spawned()
	{
		if (!SkipInitialization)
		{
			if (HasInputAuthority)
			{
				Debug.Log("Local player spawned");
				RPC_SendNameToHost(Settings.Player.PlayerName);
			}
			else
			{
				Debug.Log("Not local player spawned");
			}
		}
	}

	[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
	private void RPC_SendNameToHost(string name)
	{
		_playerName = name;
		var stringName = _playerName.ToString();
		PlayerList.Instance.AddName(stringName);
		RPC_UpdatePlayerList(PlayerList.Instance.GetCurrentPlayers);

		if (!_isNameMessageSend)
		{
			_killFeedController.SetKillFeed(stringName, "Joined the game");
			_isNameMessageSend = true;
		}
	}

	[Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
	private void RPC_UpdatePlayerList(string[] newNames) 
	{
		PlayerList.Instance.UpdateList(newNames);
	}

	public void AddScore(int score)
	{
		_matchScore += score;
		ScoreChanged?.Invoke(_matchScore, _matchKils);
	}

	public void AddKills(int kills = 1)
	{
		_matchKils += kills;

		if (HasStateAuthority)
		{
			ScoreChanged?.Invoke(_matchScore, _matchKils);
		}
	}

	public void AddDeath()
	{
		_matchDeaths++;	
	}

	private static void OnKillsChanged(Changed<PlayerScoreboardController> changed)
	{
		if (changed.Behaviour.HasStateAuthority)
		{
			changed.Behaviour.RPC_UpdateStatsUI();
		}
	}

	private static void OnDeathsChanged(Changed<PlayerScoreboardController> changed)
	{
		if (changed.Behaviour.HasStateAuthority)
		{
			changed.Behaviour.RPC_UpdateStatsUI();
		}
	}


	private static void OnScoreChanged(Changed<PlayerScoreboardController> changed)
	{
		if (changed.Behaviour.HasStateAuthority)
		{
			changed.Behaviour.RPC_UpdateStatsUI();
		}
	}

	private static void OnNameChanged(Changed<PlayerScoreboardController> changed)
	{
		//TODO can this be a thing
	}

	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	private void RPC_UpdateStatsUI()
	{
		PlayerList.Instance.UpdatePlayerStats(GetPlayerName, _matchScore, _matchKils, _matchDeaths);
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		if (hasState && !MatchManager.Instance.IsGameOver)
		{
			_killFeedController?.SetKillFeed(GetPlayerName, "left the game"); //TODO change this code to something has left?
			PlayerList.Instance.RemoveName(GetPlayerName);
		} 
	}
}
