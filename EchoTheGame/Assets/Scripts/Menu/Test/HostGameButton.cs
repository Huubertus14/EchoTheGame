using Project.Echo.Setting.Session;
using Project.Echo.Setting;
using Fusion;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Project.Echo.Menu.Test
{
	public class HostGameButton : BaseGameButton
	{
		AsyncOperation _loadGameScene;

		protected override void OnClick()
		{
			base.OnClick();
			Settings.Session = new SessionSettings("TestGame", GameMode.Host);
			Settings.Player = new();
			Settings.Player.PlayerName = $"Host {Random.value:00}"; 
			
			_loadGameScene = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
			_loadGameScene.completed += OnGameSceneLoaded;
		}

		private void OnGameSceneLoaded(AsyncOperation obj)
		{
			_loadGameScene.completed -= OnGameSceneLoaded;
		}
	}
}