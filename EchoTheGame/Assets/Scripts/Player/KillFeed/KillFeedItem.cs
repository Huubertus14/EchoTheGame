using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Project.Echo.Tuning;

public class KillFeedItem : MonoBehaviour
{
	[SerializeField] private KillFeedSettings _settings;

	[SerializeField] private TextMeshProUGUI _messageText;

	private IEnumerator _fadeCoroutine;

	private void Awake()
	{
		_messageText = GetComponentInChildren<TextMeshProUGUI>();
	}

	internal void SetText(string v)
	{
		_messageText.text = v;
		enabled = true;

		if (_fadeCoroutine != null)
		{
			StopCoroutine(_fadeCoroutine);
			_messageText.alpha = 0;
		}
	
		if (enabled)
		{
			_fadeCoroutine = FadeTextIn();
			StartCoroutine(_fadeCoroutine);
		}
	}

	private IEnumerator FadeTextIn()
	{
		float duration = _settings.fadeInSeconds;

		float evaluate = 0;
		while (evaluate <1)
		{
			evaluate+= Time.deltaTime / duration;
			var alpha = _settings.fadeInCurve.Evaluate(evaluate);
			_messageText.alpha = alpha;
			yield return 0;
		}
		_messageText.alpha = 1;
		_fadeCoroutine = FadeTextOut();
		StartCoroutine(_fadeCoroutine);
	}

	private IEnumerator FadeTextOut()
	{
		float duration = _settings.fadeOutSeconds;
		float evaluate = 0;

		while (evaluate < 1)
		{
			evaluate += Time.deltaTime / duration;
			var alpha = _settings.fadeOutCurve.Evaluate(evaluate);
			_messageText.alpha = alpha;
			yield return 0;
		}
		_messageText.alpha = 0;
		_fadeCoroutine = null;
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}
}
