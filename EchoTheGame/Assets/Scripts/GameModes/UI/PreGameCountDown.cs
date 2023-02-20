using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PreGameCountDown : MonoBehaviour
{
    private TextMeshProUGUI _countDownTime;

	private void Awake()
	{
		_countDownTime = GetComponentInChildren<TextMeshProUGUI>();
		_countDownTime.text = "0"; 
		StartCoroutine(StartGame());
	}

	/*private void Update()
	{
		if (!MatchManager.Instance.IsGameStarted)
		{
			if (!_freeForAllGameMode.IsGameStarted)
			{
				SetText(_freeForAllGameMode.GetPreGameSecondsLeft());
			}
			else
			{
				
				StartCoroutine(StartGame());
			}
		}
	}*/

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
}
