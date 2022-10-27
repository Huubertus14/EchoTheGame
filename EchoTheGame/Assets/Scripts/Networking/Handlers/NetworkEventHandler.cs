using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;
using System;
using Project.Echo.Player.Controls;

namespace Project.Echo.Networking.Handlers
{
	public class NetworkEventHandler : MonoBehaviour, INetworkRunnerCallbacks
	{
        private NetworkPrefabRef _playerPrefab;
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters;
        private PlayerMovementController _movementController;

        internal void Init(NetworkPrefabRef prefab)
        {
            _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
            _playerPrefab = prefab;
            _movementController = new PlayerMovementController();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
        {
            
            if (runner.IsServer) //TODO change this to a spawner script
            {
                Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.DefaultPlayers) * 3, 1, 0);
                NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
                _spawnedCharacters.Add(player, networkPlayerObject);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {  
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))//TODO change this to a spawner script
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input) 
        {
            var data = _movementController.GetInput();
            input.Set(data);
        }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}