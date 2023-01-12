using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Project.Echo.Player;

public class NetworkedKillFeedController : NetworkBehaviour
{
	[SerializeField] private KillFeedUIHandler _killFeedUIHandler;

	public void SetKillFeed(string userName, string message)
	{
		RPC_SendMessage($"<b>{userName}</b> {message}");
	}

	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	private void RPC_SendMessage(string message, RpcInfo info = default)
	{
		if (_killFeedUIHandler == null)
		{
			_killFeedUIHandler = PlayerNetworkedController.LocalPlayer.GetComponentInChildren<KillFeedUIHandler>();
		}

		if (_killFeedUIHandler != null) 
		{ 
			_killFeedUIHandler.OnMessageReceived(message);
		}
	}
}
