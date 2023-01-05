using Project.Echo.Networking;
using Project.Echo.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenuBehaviour : MonoBehaviour
{
	private AsyncOperation _loadMenuScene;

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			NetworkController.Instance.LeaveGame();
			_loadMenuScene = SceneManager.LoadSceneAsync("MainMenu");
			_loadMenuScene.completed += OnLoadDone;
		}
	}

	private void OnLoadDone(AsyncOperation obj)
	{
		_loadMenuScene.completed -= OnLoadDone;
		
	}
}
