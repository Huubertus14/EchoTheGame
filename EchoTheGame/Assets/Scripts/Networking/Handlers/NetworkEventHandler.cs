using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;
using System;
using Project.Echo.Player.Controls;
using Project.Echo.Spawner;
using Project.Echo.Player;
using System.Threading.Tasks;

namespace Project.Echo.Networking.Handlers
{
	public class NetworkEventHandler : MonoBehaviour, INetworkRunnerCallbacks
	{
        //private PlayerMovementController _movementController;
        Dictionary<int, PlayerNetworkedController> _mapTokenIDWithNetworkPlayer;

		private void Awake()
		{
           // _movementController = new PlayerMovementController();
            _mapTokenIDWithNetworkPlayer = new Dictionary<int, PlayerNetworkedController>();
        }

        private int GetPlayerToken(NetworkRunner runner, PlayerRef player)
		{
			if (runner.LocalPlayer == player)
			{
                return ConnectionTokenUtils.HashToken(GameManager.GetConnectionToken());
			}

			byte[] token = runner.GetPlayerConnectionToken(player);
            if (token != null)
			{
                return ConnectionTokenUtils.HashToken(token);
			}

            return -1;//invalid
		}

        public void SetConnectionTokenMapping(int token, PlayerNetworkedController playerController)
		{
            _mapTokenIDWithNetworkPlayer.Add(token, playerController);
		}

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
        {
            if (runner.IsServer) //TODO change this to a spawner script
            {
                int playerToken = GetPlayerToken(runner,player);

				if (_mapTokenIDWithNetworkPlayer.TryGetValue(playerToken, out PlayerNetworkedController playerNetworkController))
				{
                    Loading.LoadScreenController.SetLoadingText($"Found a referenced player. Assign authority over this object {playerToken}");
                    playerNetworkController.GetComponent<NetworkObject>().AssignInputAuthority(player);

					foreach (var item in playerNetworkController.GetComponentsInChildren<NetworkBehaviour>())
					{
                        item.Spawned();
					}
                    return;
				}

                Loading.LoadScreenController.SetLoadingText($"Could not find a current player, spawning a new one {playerToken}");
				PlayerNetworkedController spawnedObject = PlayerSpawner.SpawnPlayer(runner,player);
                spawnedObject.Token = playerToken;
                _mapTokenIDWithNetworkPlayer[playerToken] = spawnedObject;
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            PlayerSpawner.OnPlayerLeft(runner,player);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input) 
        {
            if (MatchManager.Instance.IsGameStarted)
            {
				//Player.Controls.Data.NetworkPlayerMovementData data = _movementController.GetInput();
                //input.Set(data);
            }
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) 
        {
        
        }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public async void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) 
        {
			try
			{
                if (!MatchManager.Instance.IsGameOver)
                {
                    Loading.LoadScreenController.Show();
                    Loading.LoadScreenController.SetLoadingText("Host migration");
                    Debug.Log("OnHost migration Started");
                    await runner.Shutdown(shutdownReason: ShutdownReason.HostMigration);
                    NetworkController.Instance.StartHostMigration(hostMigrationToken);
                }
            }
			catch (Exception e)
			{
                Debug.LogError(e);
				throw;
			}
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }

        public async void OnHostMigrationCleanup()
		{
            await Task.Delay(500);

            int tokenToRemove=0;
			foreach (var entry in _mapTokenIDWithNetworkPlayer)
			{
                NetworkObject networkObjectInDictionary = entry.Value.GetComponent<NetworkObject>();

                if (networkObjectInDictionary.InputAuthority.IsNone)
				{
                    networkObjectInDictionary.Runner.Despawn(networkObjectInDictionary);
                    tokenToRemove = entry.Key;
                    break;
				}
			}
            if (tokenToRemove != 0)
            {
                _mapTokenIDWithNetworkPlayer.Remove(tokenToRemove);
            }
		}
    }
}