using Project.Echo.Player;
using Project.Echo.Networking;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BackToMainMenuButton : MonoBehaviour
{
	private void Awake()
	{
		GetComponent<Button>().onClick.AddListener(OnClick);
	}

	private async void OnClick()
	{
		try { await SessionNavigation.ReturnToMenuAsync(); }
		catch (Exception exception) { Debug.LogException(exception); }
	}

	private void OnDestroy()
	{
		GetComponent<Button>().onClick.RemoveListener(OnClick);
	}
}
