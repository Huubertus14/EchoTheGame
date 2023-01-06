using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerList : MonoBehaviour
{
	public static PlayerList Instance;

	[SerializeField]private PlayerListItem _listItemTemplate;
	[SerializeField]private Canvas _canvas;

	private List<PlayerListItem> _currentPlayers;

	private void Awake()
	{
		_currentPlayers = new List<PlayerListItem>();
		_canvas.worldCamera = Camera.main;
		_listItemTemplate.gameObject.SetActive(false);
	}

	public void UpdateList(IReadOnlyList<string> nameCollection)
	{
		foreach (var sendName in nameCollection)
		{
			if (!_currentPlayers.Select(q=>q.PlayerName).Contains(sendName))
			{
				AddName(sendName);
			}
		}
	}

	public void AddName(string name)
	{
		PlayerListItem newItem = Instantiate(_listItemTemplate, transform);
		newItem.Init(name);
		newItem.gameObject.SetActive(true);
		_currentPlayers.Add(newItem);
	}

	public void RemoveName(string nameToRemove)
	{

	}
}
