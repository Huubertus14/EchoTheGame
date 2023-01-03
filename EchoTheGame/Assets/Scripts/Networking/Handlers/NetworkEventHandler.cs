using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;
using System;
using Project.Echo.Player.Controls;
using Project.Echo.Spawner;

namespace Project.Echo.Networking.Handlers
{
	public class NetworkEventHandler : MonoBehaviour, INetworkRunnerCallbacks
	{
        private PlayerMovementController _movementController;
        private PlayerSpawner _playerSpawner;

        public static Action<NetworkRunner, PlayerRef> PlayerJoined;
        public static Action<NetworkRunner, PlayerRef> PlayerLeft;

        internal void Init(PlayerSpawner spawner)
        {
            _playerSpawner = spawner;
            _movementController = new PlayerMovementController();
            Loading.LoadScreenController.SetLoadingText("Initialized the networkEventHandler");
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
        {
            if (runner.IsServer) //TODO change this to a spawner script
            {
                _playerSpawner.SpawnPlayer(runner,player);
            }

            PlayerJoined?.Invoke(runner, player);  
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            _playerSpawner.OnPlayerLeft(runner,player);
            PlayerLeft?.Invoke(runner, player);
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