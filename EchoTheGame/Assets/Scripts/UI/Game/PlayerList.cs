using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerList : MonoBehaviour
{
	public static PlayerList Instance;

	public string[] GetCurrentPlayers => _playerNames.ToArray();

	[SerializeField]private PlayerListItem _listItemTemplate;

	[SerializeField] private List<PlayerListItem> _currentPlayers;
	[SerializeField] private List<string> _playerNames;

	private void Awake()
	{
		if(Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}

		_playerNames = new List<string>();
		_currentPlayers = new List<PlayerListItem>();
		_listItemTemplate.gameObject.SetActive(false);
	}

	public void UpdateList(IReadOnlyCollection<string> nameCollection)
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
		_playerNames.Add(name);

		PlayerListItem newItem = Instantiate(_listItemTemplate, transform);
		newItem.Init(name);
		newItem.gameObject.SetActive(true);
		_currentPlayers.Add(newItem);
	}

	public void RemoveName(string nameToRemove)
	{

	}
}
