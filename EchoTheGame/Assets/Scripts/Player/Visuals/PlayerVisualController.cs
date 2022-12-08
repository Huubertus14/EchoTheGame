using Cysharp.Threading.Tasks;
using Project.Echo.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Project.Echo.Player.Visuals
{
    public class PlayerVisualController : MonoBehaviour
    {
		PlayerController _playerController;
		
		private async void Awake()
		{
			await UniTask.WaitUntil(() => NetworkController.NetworkLoaded);
			
			_playerController = GetComponentInParent<PlayerController>();
			await UniTask.WaitUntil(()=> _playerController.IsPlayerInitialized);

			if (_playerController.IsLocalPlayer)
			{
				Debug.Log("Im my own");
			}
			else
			{
				Debug.Log("Not my own");
			}
		
		}
	}
}