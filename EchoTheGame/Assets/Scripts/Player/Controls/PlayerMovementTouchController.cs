using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Project.Echo.Player.Controls.Data;
using Project.Echo.Networking;
using UnityEngine.EventSystems;

public class PlayerMovementTouchController : MonoBehaviour
{
	private DynamicJoystick _joystick;
	NetworkEvents _networkEvents;

	private Button _gasButton;

	private void Awake()
	{
		_joystick = GetComponentInChildren<DynamicJoystick>();
		NetworkController.OnRunnerSpawned += OnSpawned;
	}

	private void OnSpawned(NetworkRunner obj)
	{
		NetworkController.OnRunnerSpawned += OnSpawned; 
		
		_networkEvents = obj.GetComponent<NetworkEvents>();
		_networkEvents.OnInput.AddListener(OnInput);
	}

	private void OnInput(NetworkRunner arg0, NetworkInput arg1)
	{
		if (MatchManager.Instance.IsGameStarted)
		{
			arg1.Set(GetData());
		}
	}

	private void Update()
	{
		foreach (Touch touch in Input.touches)
		{
			int id = touch.fingerId;
		
			//TODO add touch raycast to custom stuff/ui/controls
		}
	}
	private NetworkPlayerMovementData GetData()
	{
		NetworkPlayerMovementData data = new();

		//Vector3 joyInput = new Vector3(-_joystick.Vertical,0,_joystick.Horizontal);
		data.Speed = 1;
		data.JoystickRotation = _joystick.Direction;
		return data;
	}
}
