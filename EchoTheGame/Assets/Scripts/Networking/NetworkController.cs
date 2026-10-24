using UnityEngine;
using Fusion;
using System;
using Project.Echo.Setting;
using Project.Echo.Spawner;
using Project.Echo.Setting.Session;
using UnityEngine.SceneManagement;
using Project.Echo.Networking.Handlers;
using System.Threading.Tasks;
using Project.Echo.Player.Controls;
using Project.Echo.Loading;
using Project.Echo.Player;

namespace Project.Echo.Networking
{
    public class NetworkController : MonoBehaviour
    {
        [SerializeField] private NetworkRunner _networkRunnerPrefab;
        private NetworkRunner _runner;

        public static NetworkController Instance { get; private set; }

        private NetworkEventHandler _eventHandler;
        private INetworkSceneManager _networkSceneManager;

        [SerializeField] private PlayerSpawner _spawner;

        public static Action<NetworkRunner> OnHostMigrationStarted;
        public static Action<NetworkRunner> OnHostMigrationDone;

        private async void Awake()
		{
			try
			{
                name = "Network Runner";
				if (Instance != null)
				{
                    Debug.LogError($"Multiple instances of {typeof(NetworkController)} found");
                    return;
				}
                Instance = this;
                LoadScreenController.SetLoadingText("Setting up session");
                SessionSettings sessionSettings = Settings.Session;
				if (sessionSettings == null)
				{
                    Debug.LogError("Sessions settings not set in menu"); //TODO return to menu here
                    return;
				}

                _runner = Instantiate(_networkRunnerPrefab);
                _runner.name = "Network Runner";

                _networkSceneManager = GetNetworkSceneManager();

                _runner.ProvideInput = true;
                _eventHandler = _runner.GetComponent<NetworkEventHandler>();
                LoadScreenController.SetLoadingText("Runner created and starting a game");

                var result = await _runner.StartGame(new StartGameArgs()
                {
                    GameMode = sessionSettings.Mode,
                    SessionName = sessionSettings.LobbyName,
                    Scene = SceneManager.GetActiveScene().buildIndex,
                    SceneManager = _networkSceneManager,
                    Initialized = OnRunnerInitialized,
                    ConnectionToken = GameManager.GetConnectionToken()
                });
                if (!result.Ok)
                {
                    LoadScreenController.SetLoadingText($"Could not start session: {result.ShutdownReason}");
                    Debug.LogError($"Could not start session: {result.ShutdownReason}");
                }
            }
			catch (Exception e)
			{
                LoadScreenController.SetLoadingText($"Something went wrong starting the network {e}");
                Debug.LogException(e);
			}
		}

        private INetworkSceneManager GetNetworkSceneManager()
		{
            var sceneManager =_runner.GetComponent<INetworkSceneManager>();

			if (sceneManager == null)
			{
                sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
			}

            return sceneManager;
		}

		private void OnRunnerInitialized(NetworkRunner obj)
		{
            LoadScreenController.SetLoadingText("Runner created and started, Network loaded");
        }

		private async Task InitializeNetworkRunnerHostMigration(NetworkRunner runner, HostMigrationToken token)
		{
            var result = await runner.StartGame(new StartGameArgs()
            {
                SceneManager = _networkSceneManager,
                HostMigrationToken = token,
                HostMigrationResume = OnHostMigrationResumed
            });
            if (!result.Ok) throw new InvalidOperationException($"Host migration failed: {result.ShutdownReason}");
        }

        private void OnHostMigrationResumed(NetworkRunner obj)
        {
            Debug.Log("Start Host migration Resume");

			foreach (NetworkObject resumeObject in _runner.GetResumeSnapshotNetworkObjects())
			{
                Debug.Log(resumeObject.name);
				if (resumeObject.TryGetBehaviour<PlayerMovement>(out var playerNetworked))
				{
                    obj.Spawn(resumeObject, playerNetworked.ReadPosition(), playerNetworked.ReadRotation(), onBeforeSpawned: (obj,newNetworkObject) =>
					{
                        newNetworkObject.CopyStateFrom(resumeObject);

						if (resumeObject.TryGetBehaviour<PlayerMovement>(out var oldMovement))
						{
                            PlayerMovement newMovement = newNetworkObject.GetComponent<PlayerMovement>();
                            newMovement.CopyStateFrom(oldMovement);
						}

                        if (resumeObject.TryGetBehaviour<PlayerNetworkedController>(out var oldNetworkController))
                        {
                            _eventHandler.SetConnectionTokenMapping(oldNetworkController.Token, newNetworkObject.GetComponent<PlayerNetworkedController>());
                        }

						if (resumeObject.TryGetBehaviour<PlayerHealthController>(out var oldHealth))
						{
                            PlayerHealthController newHealth = newNetworkObject.GetComponent<PlayerHealthController>();
                            newHealth.CopyStateFrom(oldHealth);
                            newHealth.SkipInit = true;
                        }

                        if (resumeObject.TryGetBehaviour<PlayerScoreboardController>(out var oldScore))
                        {
                            PlayerScoreboardController newScore = newNetworkObject.GetComponent<PlayerScoreboardController>();
                            newScore.CopyStateFrom(oldScore);
                            newScore.SkipInitialization = true;
                        }

						if (resumeObject.TryGetBehaviour<NetworkedPlayerGameModeController>(out var oldGameMode))
						{
                            NetworkedPlayerGameModeController newGameMode = newNetworkObject.GetComponent<NetworkedPlayerGameModeController>();
							MatchStatus matchData = MatchManager.Instance.GetMatchData();
                            newGameMode.SkipInit = true;
                            newGameMode.SetMatchValues(matchData);
                        }
                    });
				}
			}

            OnHostMigrationDone?.Invoke(_runner);
            _eventHandler.OnHostMigrationCleanup();
            Debug.Log("Done Host migration Resume"); 
            LoadScreenController.Hide();
        }

        public async void StartHostMigration(HostMigrationToken migrationToken)
		{
            OnHostMigrationStarted?.Invoke(_runner);
            _runner =Instantiate(_networkRunnerPrefab);
            _runner.ProvideInput = true;
            _networkSceneManager = GetNetworkSceneManager();
            _eventHandler = _runner.GetComponent<NetworkEventHandler>();

            try { await InitializeNetworkRunnerHostMigration(_runner, migrationToken); }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                LoadScreenController.SetLoadingText("Host migration failed. Return to the menu and reconnect.");
            }
        }

        public async Task LeaveGame()
		{
            if (_runner == null || !_runner.IsRunning) return;
            if (_runner.IsServer)
            {
                await _runner.PushHostMigrationSnapshot();
            }
           await _runner.Shutdown();
		}

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
	} 
}
