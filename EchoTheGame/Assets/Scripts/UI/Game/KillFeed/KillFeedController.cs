using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFeedController : MonoBehaviour
{
	private static KillFeedController _instance;

	[SerializeField] private KillFeedItem _killfeedTemplate;

	private Queue<KillFeedItem> _killFeedQueue;

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

		_killfeedTemplate.gameObject.SetActive(false);
		_killFeedQueue = new Queue<KillFeedItem>();
	}

	public static void SetKillFeed(string message)
	{
		var newItem = Instantiate(_instance._killfeedTemplate, _instance.transform);

	}

}
