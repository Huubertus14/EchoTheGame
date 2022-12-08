using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Project.Echo.Spawner
{
    public class PlayerSpawner : MonoBehaviour
	{
		private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters;
		[SerializeField]private NetworkPrefabRef _playerPrefab;
		SpawnPoint[] _allSpawnPoints;

		private void Awake()
		{
			_allSpawnPoints = gameObject.GetComponentsInChildren<SpawnPoint>();
			_spawnedCharacters = new();
		}

		public void SpawnPlayer(NetworkRunner runner, PlayerRef player) 
		{
			Vector3 spawnPosition = GetAvailableSpawnPosition();
			NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
			_spawnedCharacters.Add(player, networkPlayerObject);
			runner.SetPlayerObject(player, networkPlayerObject);
		}

		public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
		{
			if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
			{
				runner.Despawn(networkObject);
				_spawnedCharacters.Remove(player);
			}
		}

        public void RespawnPlayer(object playerObject)
		{
			SpawnPoint _spawnPoint = _allSpawnPoints[Random.Range(0,_allSpawnPoints.Length-1)];
		}

		public Vector3 GetAvailableSpawnPosition()
		{
			//TODO check if a spawnpoint is available
			return _allSpawnPoints[Random.Range(0, _allSpawnPoints.Length)].transform.position;
		}
	}
}