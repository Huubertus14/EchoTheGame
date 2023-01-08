using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Project.Echo.Setting;
using System;

public class PlayerScoreboardController : NetworkBehaviour
{
	[Networked(OnChanged =nameof(OnKillsChanged))]
	private int _matchKils { get; set; }

	[Networked(OnChanged = nameof(OnDeathsChanged))]
	private int _matchDeaths { get; set; }

	[Networked(OnChanged = nameof(OnScoreChanged))]
	private int _matchScore { get; set; }

	[SerializeField]private string _myName;

	public string GetPlayerName => _myName;

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
		_myName = name;
		PlayerList.Instance.AddName(_myName);
		RPC_UpdatePlayerList(_myName,PlayerList.Instance.GetCurrentPlayers);
	}

	[Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
	private void RPC_UpdatePlayerList(string newPlayer,string[] newNames) 
	{
		KillFeedController.SetKillFeed($"<b>{newPlayer}</b> Joined the game");
		PlayerList.Instance.UpdateList(newNames);
	}

	private static void OnKillsChanged(Changed<PlayerScoreboardController> changed)
	{
		
	}

	private static void OnDeathsChanged(Changed<PlayerScoreboardController> changed)
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
		KillFeedController.SetKillFeed($"<b>{_myName}</b> left the game");
		PlayerList.Instance.RemoveName(_myName);
	}
}
