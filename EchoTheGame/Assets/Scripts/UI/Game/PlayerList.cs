using UnityEngine;

public class PlayerList : MonoBehaviour
{
	public static PlayerList Instance;

	[SerializeField]private PlayerListItem _listItemTemplate;
	[SerializeField]private Canvas _canvas;

	private void Awake()
	{
		_canvas.worldCamera = Camera.main;
		_listItemTemplate.gameObject.SetActive(false);
	}
}
