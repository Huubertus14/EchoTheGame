using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerListItem : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _playerNameText;

	public string PlayerName { get;private set; }

	public void Init(string playerName)
	{
		PlayerName = playerName;
		_playerNameText.text = PlayerName;
	}
}
