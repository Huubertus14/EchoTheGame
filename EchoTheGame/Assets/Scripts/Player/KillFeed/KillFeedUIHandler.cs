using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFeedUIHandler : MonoBehaviour
{
    private KillFeedItem[] _killFeedItems;

	private int _killfeedIndex;

	private void Awake()
	{
		_killFeedItems = GetComponentsInChildren<KillFeedItem>();
		_killfeedIndex = 0;
		foreach (var feedItem in _killFeedItems)
		{
			feedItem.SetText("");
		}
	}

	public void OnMessageReceived(string message)
	{
		if (MatchManager.Instance.IsGameOver || _killFeedItems.Length == 0)
		{
			return;
		}

		var item = _killFeedItems[_killfeedIndex++];
		item.transform.SetSiblingIndex(0);
		item.SetText(message);
		if (_killfeedIndex>= _killFeedItems.Length)
		{
			_killfeedIndex = 0;
		}
	}
}
