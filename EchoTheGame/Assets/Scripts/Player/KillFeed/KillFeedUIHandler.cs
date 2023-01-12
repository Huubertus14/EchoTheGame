using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFeedUIHandler : MonoBehaviour
{
    private KillFeedItem[] _killFeedItems;

	private Queue<string> _messageQueue = new Queue<string>();

	private void Awake()
	{
		_killFeedItems = GetComponentsInChildren<KillFeedItem>();

		foreach (var feedItem in _killFeedItems)
		{
			feedItem.SetText("");
		}
	}

	public void OnMessageReceived(string message)
	{
		Debug.Log(message);

		_messageQueue.Enqueue(message);

		if (_messageQueue.Count > 4)
		{
			_messageQueue.Dequeue();
		}

		int queueIndex = 0;
		foreach (string messageInQueue in _messageQueue)
		{
			_killFeedItems[queueIndex].SetText(messageInQueue);
			queueIndex++;
		}
	}
}
