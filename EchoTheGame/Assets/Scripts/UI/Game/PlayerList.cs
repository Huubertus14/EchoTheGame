using Project.Echo.Networking;
using UnityEngine;
using Fusion;
using Project.Echo.Player;
using Cysharp.Threading.Tasks;
using System.Linq;
using Project.Echo.Networking.Handlers;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PlayerList : MonoBehaviour, IPlayerJoinedInitialization
{
    [SerializeField] private PlayerListItem _playerItemTemplate;

	private List<PlayerListItem> _trackedPlayers;
	private NetworkRunner _networkRunner;

	public Task Init(NetworkRunner runner)
	{
		_networkRunner = runner;
		_trackedPlayers = new List<PlayerListItem>();

		NetworkEventHandler.PlayerJoined += OnPlayerJoined;
		NetworkEventHandler.PlayerLeft += OnPlayerLeft;

		var allActivePlayers = runner.ActivePlayers;

		foreach (PlayerRef activePlay in allActivePlayers)
		{
			Debug.Log($"{activePlay.PlayerId} -");
			AddPlayer(GetPlayerName(activePlay));
		}
		return Task.CompletedTask;
	}

	private void AddPlayer(string name)
	{
		PlayerListItem playerNameItem = Instantiate(_playerItemTemplate,transform).GetComponent<PlayerListItem>();
		playerNameItem.Init(name);
		playerNameItem.gameObject.SetActive(true);
		_trackedPlayers.Add(playerNameItem);
	}

	private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		string playerToRemove = GetPlayerName(player);
		
		for (int i = 0; i < _trackedPlayers.Count; i++)
		{
			if (_trackedPlayers[i].PlayerName == playerToRemove)
			{
				Destroy(_trackedPlayers[i].gameObject);
				_trackedPlayers.RemoveAt(i);
				break;
			}
		}
	}

	private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{
		AddPlayer(GetPlayerName(player));
	}

	private string GetPlayerName(PlayerRef player)
	{
		if (_networkRunner.TryGetPlayerObject(player, out var networkObject))
		{
			PlayerNetworkedController q = _networkRunner.TryGetNetworkedBehaviourFromNetworkedObjectRef<PlayerNetworkedController>(networkObject.Id);
			if (q != null)
			{
				return q.PlayerSettings.PlayerName;
			}
		}
		return "Unknown?";
	}

	private void OnDestroy()
{
		NetworkEventHandler.PlayerJoined -= OnPlayerJoined;
		NetworkEventHandler.PlayerLeft -= OnPlayerLeft; 
	}
}
