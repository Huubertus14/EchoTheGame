using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerList : MonoBehaviour
{
	public static PlayerList Instance;

	public string[] GetCurrentPlayers => _playerNames.ToArray();

	[SerializeField]private PlayerListItem _listItemTemplate;

	 private Dictionary<string,PlayerListItem> _currentPlayers;
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
		foreach (var sendName in nameCollection)
		{
			if(!_currentPlayers.ContainsKey(sendName))
			{
				AddName(sendName) ;
			}
		}

		foreach(var item in _currentPlayers)
		{
			if(!nameCollection.Contains(item.Key))
			{
				Destroy(item.Value.gameObject);
				_currentPlayers.Remove(item.Key);
			}
		}
	}

	public void AddName(string name)
	{
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
}
