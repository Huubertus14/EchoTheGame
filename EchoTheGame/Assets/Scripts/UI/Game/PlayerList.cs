using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerList : MonoBehaviour
{
	public static PlayerList Instance;

	public string[] GetCurrentPlayers => _playerNames.ToArray();

	[SerializeField]private PlayerListItem _listItemTemplate;

	 private Dictionary<string,PlayerListItem> _currentPlayers;
	private readonly HashSet<string> _incomingNames = new HashSet<string>();
	private readonly List<string> _removedNames = new List<string>();
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
			return;
		}

		_playerNames = new List<string>();
		_currentPlayers = new Dictionary<string, PlayerListItem>();
		_listItemTemplate.gameObject.SetActive(false);
	}

	public void UpdatePlayerStats(string playerName,int score, int kills, int deaths)
	{
		if (_currentPlayers.TryGetValue(playerName, out var item))
		{
			item.UpdateKills(kills);
			item.UpdateDeaths(deaths);
			item.UpdateScore(score);
		}
	}

	public void UpdateList(IReadOnlyCollection<string> nameCollection)
	{
		_incomingNames.Clear();
		foreach (var sendName in nameCollection)
		{
			_incomingNames.Add(sendName);
			if(!_currentPlayers.ContainsKey(sendName))
			{
				AddName(sendName) ;
			}
		}

		_removedNames.Clear();
		foreach(var item in _currentPlayers)
		{
			if(!_incomingNames.Contains(item.Key))
			{
				_removedNames.Add(item.Key);
			}
		}
		foreach (var removedName in _removedNames) RemoveName(removedName);
	}

	public void AddName(string name)
	{
		if (_currentPlayers.ContainsKey(name)) return;
		_playerNames.Add(name);

		PlayerListItem newItem = Instantiate(_listItemTemplate, transform);
		newItem.Init(name);
		newItem.gameObject.SetActive(true);
		_currentPlayers.Add(name,newItem);
	}

	public void RemoveName(string nameToRemove)
	{
		_playerNames.Remove(nameToRemove);

		if(_currentPlayers.TryGetValue(nameToRemove, out var itemToDeleteAndRemove))
		{
			Destroy(itemToDeleteAndRemove.gameObject);
			_currentPlayers.Remove(nameToRemove);
		}
	}

	public List<PlayerListItem> GetSortedPlayerList()
	{
		List<PlayerListItem > unsortedPlayers = _currentPlayers.Values.ToList();
		unsortedPlayers.Sort((a,b)=> b.GetScore.CompareTo(a.GetScore));
		return unsortedPlayers;
	}

	private void OnDestroy()
	{
		if (Instance == this) Instance = null;
	}
}
