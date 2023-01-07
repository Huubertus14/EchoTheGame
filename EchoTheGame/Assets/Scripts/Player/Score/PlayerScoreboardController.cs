using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Project.Echo.Setting;

public class PlayerScoreboardController : NetworkBehaviour
{
	//When spawn set own name and send rpc to host

	// if host do same but also receive rpc to clients to update score
	[SerializeField]private string _myName;
	//private PlayerList _playerList; //TODO find this object in scene and let that keep the list

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
			//_playerList.gameObject.SetActive(false);
		}
	}

	[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
	private void RPC_SendNameToHost(string name)
	{
		//if local player joined. update host with my new name
		_myName = name;
		PlayerList.Instance.AddName(_myName);
		RPC_ReceiveNewName(PlayerList.Instance.GetCurrentPlayers);
	}

	[Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
	private void RPC_ReceiveNewName(string[] newNames) 
	{
		//Host receives new name and tells everyone to update their list
		//Debug.Log($"name received. Add to collection and send new list to clients {newName}");
		PlayerList.Instance.UpdateList(newNames);
	}


}
