using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Project.Echo.Networking;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;

namespace Project.Echo.Player.Cameras
{
    public class FollowMainPlayerCamera : MonoBehaviour
    {
        NetworkRunner _runner; //TODO find my runner?
		Transform _playerTransform;

		[SerializeField] private Vector3 _offset = new Vector3(0,30,0);

		private async void Awake()
		{
			try {
				enabled = false;
				while (!NetworkController.NetworkLoaded)
				{
					await UniTask.Yield();
				}
					
				_runner = NetworkController.Instance.Runner;
				Loading.LoadScreenController.SetLoadingText("Finding own player");
				_playerTransform = await GetOwnPlayerTransform();
				Loading.LoadScreenController.SetLoadingText("Found own player");
				enabled = true;
			}
			catch(Exception e) {
				Debug.LogError(e);
			}
		}

		private async Task<Transform> GetOwnPlayerTransform()
		{
			Transform myPlayer = null;
			while (myPlayer == null)
			{
				await UniTask.NextFrame();

				NetworkObject localPlayer = _runner.GetPlayerObject(_runner.LocalPlayer);
				if (localPlayer != null)
				{
					myPlayer = localPlayer.transform;
				}
			}
			return myPlayer;
		}

		private void Update()
		{
			transform.position = _playerTransform.position + _offset;
		}
	}
}