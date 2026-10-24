using Project.Echo.Networking;
using Project.Echo.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenuBehaviour : MonoBehaviour
{
	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			ReturnToMenu();
		}
	}

	private async void ReturnToMenu()
	{
		try { await SessionNavigation.ReturnToMenuAsync(); }
		catch (Exception exception) { Debug.LogException(exception); }
	}
}
