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
        [SerializeField]private NetworkPrefabRef _playerPrefab;

        private NetworkRunner _runner;
        private NetworkEventHandler _eventHandler;

        private async void Awake()
		{
			try
			{
                SessionSettings sessionSettings = Settings.Session;
				if (sessionSettings == null)
				{
                    Debug.LogError("Sessions settings not set in menu"); //TODO return to menu here
                    return;
				}

                _eventHandler = gameObject.AddComponent<NetworkEventHandler>();
                _eventHandler.Init(_playerPrefab);
                _runner = gameObject.AddComponent<NetworkRunner>();
                _runner.ProvideInput = true;

                await _runner.StartGame(new StartGameArgs()
                {
                    GameMode = sessionSettings.Mode,
                    SessionName = sessionSettings.LobbyName,
                    Scene = SceneManager.GetActiveScene().buildIndex,
                    SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
                });

            }
			catch (Exception e)
			{
                Debug.LogException(e);
				throw;
			}
		}
	} }