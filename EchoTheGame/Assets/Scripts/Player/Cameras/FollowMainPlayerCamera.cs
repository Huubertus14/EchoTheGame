using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Project.Echo.Networking;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Project.Echo.Player.Cameras
{
    public class FollowMainPlayerCamera : MonoBehaviour
    {
        NetworkRunner _runner; //TODO find my runner?
		Transform _playerTransform;

		[SerializeField]private Vector3 _offset = new Vector3(0,30,0);

		private async void Awake()
		{
			enabled = false;
			Debug.Log("a"); 
			while (NetworkController.Instance==null )
			{
				await UniTask.NextFrame();
			}
			Debug.Log("b");
			while ( !NetworkController.Instance.NetworkLoaded)
			{
				await UniTask.NextFrame();
			}
			Debug.Log("c");
			while (_runner == null)
			{
				await UniTask.NextFrame();
				_runner = NetworkController.Instance.Runner;

			}
			Debug.Log("d");
			while (_playerTransform == null)
			{
				await UniTask.NextFrame();

				var localPlayer = _runner.GetPlayerObject(_runner.LocalPlayer);
				if (localPlayer != null)
				{
					_playerTransform = localPlayer.transform;
				}
			}
			Debug.Log("e");
			enabled = true;
		}

		private void Update()
		{
			transform.position = _playerTransform.position + _offset;
		}
	}
}