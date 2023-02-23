using System.Collections;
using Project.Echo.Networking;
using UnityEngine;
using TMPro;
using Fusion;
using System;

public class PreGameCountDown : MonoBehaviour
{
    private TextMeshProUGUI _countDownTime;
	private bool _canFadeOut;
	private void Awake()
	{
		NetworkController.OnHostMigrationDone += HostMigrationDone;
		_countDownTime = GetComponentInChildren<TextMeshProUGUI>();
		_countDownTime.text = "0"; 
	}

	private void HostMigrationDone(NetworkRunner _)
	{
		MatchManager.Instance.IsGameStarted = false;
		enabled = true;
		gameObject.SetActive(true);
	}

	private void Update()
	{
		if (!MatchManager.Instance.IsGameStarted && !MatchManager.Instance.IsGameOver)
		{
			_canFadeOut = true;
			enabled = true;
			SetText(MatchManager.Instance.PreGameTime); //TODO add host migration check here
		}
		else if(_canFadeOut)
		{
			_canFadeOut = true;
			StartCoroutine(StartGame());
		}
	}

	private IEnumerator StartGame()
	{
		_countDownTime.text = "Start!";
		yield return new WaitForSeconds(1.4f); //TODO scale and fade out
		enabled = false;
		gameObject.SetActive(false);
	}

	private void SetText(float secondsLeft)
	{
		int seconds = Mathf.RoundToInt(secondsLeft);
		_countDownTime.text = $"{seconds:00}";
	}

	private void OnDestroy()
	{
		NetworkController.OnHostMigrationDone -= HostMigrationDone;
	}
}
