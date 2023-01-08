using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KillFeedItem : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _feedText;

	public void SetText(string message)
	{
		_feedText.text = message;
	}
}
