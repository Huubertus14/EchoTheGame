using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFeedController : MonoBehaviour
{
	private static KillFeedController _instance;

	private KillFeedItem[] _killFeedItems;
	private int _feedindex;

	private void Awake()
	{
		if(_instance == null)
		{
			_instance = this;
		}
		else
		{
			Destroy(gameObject);
		}

		_killFeedItems = GetComponentsInChildren<KillFeedItem>(true);
		foreach(var item in _killFeedItems)
		{
			item.SetText("");
		}
	}

	public static void SetKillFeed(string message)
	{
		int index = _instance._feedindex++ % _instance._killFeedItems.Length;
		_instance._killFeedItems[index].SetText(message);

	}

}
