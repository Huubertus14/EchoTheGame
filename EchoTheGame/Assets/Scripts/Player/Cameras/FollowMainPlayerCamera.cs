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
		[SerializeField]Transform _playerTransform;

		[SerializeField] private Vector3 _offset = new Vector3(0,30,0);

		private void Awake()
		{
			enabled = false;
			PlayerNetworkedController.LocalPlayerSpawned += OnLocalPlayerSpawned;
			NetworkController.OnHostMigrationDone += OnHostMigrated;
		}

		private void OnHostMigrated(NetworkRunner _)
		{
			_playerTransform= GetOwnPlayerTransform();
			enabled = true;
		}

		private void OnLocalPlayerSpawned(NetworkRunner _)
		{
			PlayerNetworkedController.LocalPlayerSpawned -= OnLocalPlayerSpawned;
			_playerTransform = GetOwnPlayerTransform();
			enabled = true;
		}

		private Transform GetOwnPlayerTransform()
		{
			Transform myPlayer = null;
			if (PlayerNetworkedController.LocalPlayer!=null)
			{
				myPlayer = PlayerNetworkedController.LocalPlayer.transform;
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

		private void OnDestroy()
		{
			NetworkController.OnHostMigrationDone -= OnHostMigrated;
		}
	}
}