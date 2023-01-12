using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KillFeedItem : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _messageText;

	private void Awake()
	{
		_messageText = GetComponentInChildren<TextMeshProUGUI>();
	}

	internal void SetText(string v)
	{
		_messageText.text = v;
	}
}
