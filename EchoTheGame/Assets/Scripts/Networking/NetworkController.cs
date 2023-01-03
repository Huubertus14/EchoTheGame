using UnityEngine;
using Fusion;
using System;
using Project.Echo.Setting;
using Project.Echo.Spawner;
using Project.Echo.Setting.Session;
using UnityEngine.SceneManagement;
using Project.Echo.Networking.Handlers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Echo.Networking
{
    public class NetworkController : MonoBehaviour
    {
        public static NetworkController Instance { get; private set; }
        private NetworkRunner _runner;// { get; private set; }
        private NetworkEventHandler _eventHandler;

        [SerializeField] private PlayerSpawner _spawner;

        private async void Awake()
		{
			try
			{
				if (Instance != null)
				{
                    Debug.LogError($"Multiple instances of {typeof(NetworkController)} found");
                    return;
				}
                Instance = this;
                Loading.LoadScreenController.SetLoadingText("Setting up session");
                SessionSettings sessionSettings = Settings.Session;
				if (sessionSettings == null)
				{
                    Debug.LogError("Sessions settings not set in menu"); //TODO return to menu here
                    return;
				}
                
                Loading.LoadScreenController.SetLoadingText("Session setup");

               
                _eventHandler = gameObject.AddComponent<NetworkEventHandler>();
                _eventHandler.Init(_spawner);

                _runner = gameObject.AddComponent<NetworkRunner>();
                _runner.ProvideInput = true;
                Loading.LoadScreenController.SetLoadingText("Runner created and starting a game");
                await _runner.StartGame(new StartGameArgs()
                {
                    GameMode = sessionSettings.Mode,
                    SessionName = sessionSettings.LobbyName,
                    Scene = SceneManager.GetActiveScene().buildIndex,
                    SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
                });
                
                Loading.LoadScreenController.SetLoadingText("Runner created and started, Network loaded");

                //TODO wait here for player to be spawned

                //get all objects of interface in scene and init them
               
            }
			catch (Exception e)
			{
                Loading.LoadScreenController.SetLoadingText($"Something went wrong starting the network {e}");
                Debug.LogException(e);
				throw;
			}
		}
	} 
}