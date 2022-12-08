using UnityEngine;
using Fusion;
using System;
using Project.Echo.Setting;
using Project.Echo.Setting.Session;
using UnityEngine.SceneManagement;
using Project.Echo.Networking.Handlers;

namespace Project.Echo.Networking
{
    public class NetworkController : MonoBehaviour
    {
        public static NetworkController Instance { get; private set; }

        public bool NetworkLoaded { get; private set; }

        public NetworkRunner Runner { get; private set; }
        private NetworkEventHandler _eventHandler;

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

                SessionSettings sessionSettings = Settings.Session;
				if (sessionSettings == null)
				{
                    Debug.LogError("Sessions settings not set in menu"); //TODO return to menu here
                    return;
				}

                _eventHandler = gameObject.AddComponent<NetworkEventHandler>();
                _eventHandler.Init();
                Runner = gameObject.AddComponent<NetworkRunner>();
                Runner.ProvideInput = true;

                await Runner.StartGame(new StartGameArgs()
                {
                    GameMode = sessionSettings.Mode,
                    SessionName = sessionSettings.LobbyName,
                    Scene = SceneManager.GetActiveScene().buildIndex,
                    SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
                });

                NetworkLoaded = true;
            }
			catch (Exception e)
			{
                Debug.LogException(e);
				throw;
			}
		}
	} }