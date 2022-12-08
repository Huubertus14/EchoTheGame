using Cysharp.Threading.Tasks;
using Fusion;
using Project.Echo.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Echo.Player { 
public class PlayerController : MonoBehaviour
{
		public bool IsLocalPlayer { get; private set; }
		public bool IsPlayerInitialized { get; private set; }
		public NetworkObject _myNetworkObject;

		private async void Awake()
		{
			IsPlayerInitialized = false;
			await UniTask.WaitUntil(() => NetworkController.NetworkLoaded);
			
			var runner = NetworkController.Instance.Runner;

			if (!runner.TryGetPlayerObject(runner.LocalPlayer, out _myNetworkObject))
			{
				Debug.LogError("Something went wrong");
				return;
			}

			IsLocalPlayer = _myNetworkObject.Id == GetComponent<NetworkObject>().Id;
			IsPlayerInitialized = true;
		}
	}
}