using Fusion;
using Project.Echo.Player;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Project.Echo.Spawner
{
    public class PlayerSpawner : MonoBehaviour
	{
		private static PlayerSpawner _instance;
		private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters;
		[SerializeField]private NetworkPrefabRef _playerPrefab;
		SpawnPoint[] _allSpawnPoints;
		private int _spawnSequence;
		private int[] _lastUsed;

		private void Awake()
		{
			if (_instance == null)
			{
				_instance = this;
			}
			_allSpawnPoints = gameObject.GetComponentsInChildren<SpawnPoint>();
			_spawnedCharacters = new();
			_lastUsed = new int[_allSpawnPoints.Length];
			System.Array.Fill(_lastUsed, -1000);
		}

		public static PlayerNetworkedController SpawnPlayer(NetworkRunner runner, PlayerRef player) //TODO use the callback methods
		{
			Vector3 spawnPosition = GetAvailableSpawnPosition();
			NetworkObject networkPlayerObject = runner.Spawn(_instance._playerPrefab, spawnPosition, Quaternion.identity, player);
			_instance._spawnedCharacters.Add(player, networkPlayerObject);
			//runner.SetPlayerObject(player, networkPlayerObject);
			 return networkPlayerObject.GetComponent<PlayerNetworkedController>();
		}

		public static void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
		{
			if (!runner.IsServer || _instance == null) return;
			if (_instance._spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
			{
				runner.Despawn(networkObject);
				_instance._spawnedCharacters.Remove(player);
			}
		}

		private void OnDestroy() { if (_instance == this) _instance = null; }

		public static Vector3 GetAvailableSpawnPosition(Transform respawningPlayer = null)
		{
			if (_instance == null || _instance._allSpawnPoints.Length == 0)
				throw new System.InvalidOperationException("The game scene needs player spawn points.");
			int best = -1;
			float bestScore = float.NegativeInfinity;
			for (int i = 0; i < _instance._allSpawnPoints.Length; i++)
			{
				Vector3 position = _instance._allSpawnPoints[i].transform.position;
				var arena = Project.Echo.Environment.UnderwaterArena.Instance;
				if (arena != null && !arena.IsSpawnClear(position)) continue;
				float nearest = 30f;
				foreach (var character in _instance._spawnedCharacters.Values)
					if (character != null && character.transform != respawningPlayer)
						nearest = Mathf.Min(nearest, Vector3.Distance(position, character.transform.position));
				// Prefer separation; break ties by least recently used, including respawns.
				float score = nearest + Mathf.Min(100, _instance._spawnSequence - _instance._lastUsed[i]) * 0.05f;
				if (score > bestScore) { best = i; bestScore = score; }
			}
			if (best < 0) throw new System.InvalidOperationException("No spawn point lies inside the arena.");
			_instance._lastUsed[best] = ++_instance._spawnSequence;
			return _instance._allSpawnPoints[best].transform.position;
		}
	}
}
