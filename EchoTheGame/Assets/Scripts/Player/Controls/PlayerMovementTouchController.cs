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

	[SerializeField] private BaseTouchButton _gasButton;
	[SerializeField] private BaseTouchButton _pingButton;
	[SerializeField] private BaseTouchButton _shootButton;

	private void Awake()
	{
		_joystick = GetComponentInChildren<DynamicJoystick>();

		_gasButton = GetComponentInChildren<GasButton>();
		_pingButton = GetComponentInChildren<PingButton>();
		_shootButton = GetComponentInChildren<ShootButton>();

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

	private NetworkPlayerMovementData GetData()
	{
		NetworkPlayerMovementData data = new();
		data.JoystickRotation = _joystick.Direction; 

		data.Speed = _gasButton.IsPressed ? 1 : 0;
		data.IsShooting = _shootButton.IsPressed;
		data.IsPing = _pingButton.IsPressed;

		return data;
	}
}
