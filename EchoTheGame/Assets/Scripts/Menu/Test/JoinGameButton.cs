using Project.Echo.Setting.Session;
using Project.Echo.Setting;
using UnityEngine.SceneManagement;
using UnityEngine;
using Fusion;

namespace Project.Echo.Menu.Test
{
	public class JoinGameButton : BaseGameButton
	{
		AsyncOperation _loadGameScene;

		protected override void OnClick()
		{
			base.OnClick();//
			Settings.Session = new SessionSettings("TestGame", GameMode.Client);
			Settings.Player = new();
			Settings.Player.PlayerName = $"Client {Random.value:00}";

			_loadGameScene = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
			_loadGameScene.completed += OnGameSceneLoaded;
		}

		private void OnGameSceneLoaded(AsyncOperation obj)
		{
			_loadGameScene.completed -= OnGameSceneLoaded;
		}
	}
}
