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
    public class FollowMainPlayerCamera : MonoBehaviour, IPlayerJoinedInitialization
    {
        NetworkRunner _runner; //TODO find my runner?
		Transform _playerTransform;

		[SerializeField] private Vector3 _offset = new Vector3(0,30,0);

		private void Awake()
		{
			enabled = false;
		}

		public Task Init(NetworkRunner runner)
		{
			_runner = runner;
			Loading.LoadScreenController.SetLoadingText("Finding own player");
			Loading.LoadScreenController.SetLoadingText("Found own player");
			enabled = true;
			return Task.CompletedTask;
		}

		private Transform GetOwnPlayerTransform()
		{
			Transform myPlayer = null;
			
			NetworkObject localPlayer = _runner.GetPlayerObject(_runner.LocalPlayer);
			if (localPlayer != null)
			{
				myPlayer = localPlayer.transform;
			}
			return myPlayer;
		}

		private void Update()
		{
			if (_playerTransform != null)
			{
				transform.position = _playerTransform.position + _offset;
			}
			else
			{
				_playerTransform = GetOwnPlayerTransform();
			}
		}
	}
}