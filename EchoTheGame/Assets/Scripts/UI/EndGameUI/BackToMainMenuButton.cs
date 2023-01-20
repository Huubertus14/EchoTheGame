using Project.Echo.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BackToMainMenuButton : MonoBehaviour
{
	private AsyncOperation _loadMenuScene;

	private void Awake()
	{
		GetComponent<Button>().onClick.AddListener(OnClick);
	}

	private void OnClick()
	{
		//NetworkController.Instance.LeaveGame();
		_loadMenuScene = SceneManager.LoadSceneAsync("MainMenu");
		_loadMenuScene.completed += OnLoadDone;
		//Leave the game
	}

	private void OnLoadDone(AsyncOperation obj)
	{
		_loadMenuScene.completed -= OnLoadDone;
	}

	private void OnDestroy()
	{
		GetComponent<Button>().onClick.RemoveListener(OnClick);
	}
}
