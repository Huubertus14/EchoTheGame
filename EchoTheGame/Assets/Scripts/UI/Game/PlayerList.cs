using UnityEngine;
using Fusion;
using Project.Echo.Player;
using Project.Echo.Networking.Handlers;
using System.Collections.Generic;

public class PlayerList : MonoBehaviour
{
    [SerializeField] private PlayerListItem _playerItemTemplate;

	private List<PlayerListItem> _trackedPlayers;
	private NetworkRunner _networkRunner;

	private void Awake()
	{

		_trackedPlayers = new List<PlayerListItem>();
		_playerItemTemplate.gameObject.SetActive(false);
		PlayerNetworkedController.LocalPlayerSpawned += OnSpawned; 
		
	}

	private void OnSpawned(NetworkRunner obj)
	{
		PlayerNetworkedController.LocalPlayerSpawned -= OnSpawned;
		_networkRunner = obj;
		var allActivePlayers = obj.ActivePlayers;

		//foreach (PlayerRef activePlay in allActivePlayers)
		//{
		//	AddPlayer(GetPlayerName(activePlay));
		//}
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
				return q.PlayerName.ToString();
			}
		}
		return "Unknown?";
	}
}
