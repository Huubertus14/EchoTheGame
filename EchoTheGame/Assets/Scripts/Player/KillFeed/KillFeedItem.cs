using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KillFeedItem : MonoBehaviour
{
	[SerializeField]private AnimationCurve _fadeInCurve;
	[SerializeField] private AnimationCurve _fadeOutCurve;

	[SerializeField] private TextMeshProUGUI _messageText;

	private IEnumerator _fadeCoroutine;

	private void Awake()
	{
		_messageText = GetComponentInChildren<TextMeshProUGUI>();
	}

	internal void SetText(string v, float visibleTime = 1.5f)
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
		float duration= 0.2f;

		float evaluate = 0;
		while (evaluate <1)
		{
			evaluate+= Time.deltaTime / duration;
			var alpha = _fadeInCurve.Evaluate(evaluate);
			_messageText.alpha = alpha;
			yield return 0;
		}
		_messageText.alpha = 1;
		_fadeCoroutine = FadeTextOut();
		StartCoroutine(_fadeCoroutine);
	}

	private IEnumerator FadeTextOut()
	{
		float duration = 3.8f;
		float evaluate = 0;

		while (evaluate < 1)
		{
			evaluate += Time.deltaTime / duration;
			var alpha = _fadeOutCurve.Evaluate(evaluate);
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
