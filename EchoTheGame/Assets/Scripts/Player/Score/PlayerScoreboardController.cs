using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Project.Echo.Setting;
using System;

public class PlayerScoreboardController : NetworkBehaviour
{
	private NetworkedKillFeedController _killFeedController;

	[Networked(OnChanged =nameof(OnKillsChanged))]
	private int _matchKils { get; set; }

	[Networked(OnChanged = nameof(OnDeathsChanged))]
	private int _matchDeaths { get; set; }

	[Networked(OnChanged = nameof(OnScoreChanged))]
	private int _matchScore { get; set; }

	[Networked(OnChanged = nameof(OnNameChanged))]
	private NetworkString<_32> _playerName { get; set; }

	private bool _isNameMessageSend;

	public string GetPlayerName => _playerName.ToString();

	private void Awake()
	{
		_killFeedController = GetComponent<NetworkedKillFeedController>();
	}

	public override void Spawned()
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

	[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
	private void RPC_SendNameToHost(string name)
	{
		_playerName = name;
		var stringName = _playerName.ToString();
		PlayerList.Instance.AddName(stringName);
		RPC_UpdatePlayerList( PlayerList.Instance.GetCurrentPlayers);

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

	private static void OnKillsChanged(Changed<PlayerScoreboardController> changed)
	{
		
	}

	private static void OnDeathsChanged(Changed<PlayerScoreboardController> changed)
	{

	}

	private static void OnNameChanged(Changed<PlayerScoreboardController> changed)
	{
		
	}

	private static void OnScoreChanged(Changed<PlayerScoreboardController> changed)
	{

	}

	private void UpdateKillsUI()
	{

	}

	private void UpdateDeathsUI()
	{

	}

	private void UpdateScoreUI()
	{

	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		_killFeedController?.SetKillFeed(GetPlayerName,"left the game"); //TODO change this code to something has left?
		PlayerList.Instance.RemoveName(GetPlayerName);
	}
}
