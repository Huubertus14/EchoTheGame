using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Project.Echo.Player;

public class GameModeTimerUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _timerText;
	 private FreeForAll _freeForAllGameMode;

	private float _refreshTimer= 0;

	private void Awake()
	{
		_timerText.text = "00:00";
	}

	private void Update()
	{
		if (MatchManager.Instance.IsGameStarted)
		{
			_refreshTimer += Time.deltaTime;
			if (_refreshTimer > 0.5f)
			{
				SetText(MatchManager.Instance.TimeLeft);
			}
		}
	}

	private void SetText(float secondsLeft)
	{
		int minutes = Mathf.FloorToInt(secondsLeft / 60);
		int seconds =Mathf.RoundToInt(secondsLeft % 60);

		_timerText.text = $"{minutes:00}:{seconds:00}";
	}
}
