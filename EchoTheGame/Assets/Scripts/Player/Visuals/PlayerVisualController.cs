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

		private Renderer[] _renderers;

		private async void Awake()
		{
			await UniTask.WaitUntil(() => NetworkController.NetworkLoaded);
			
			_playerController = GetComponentInParent<PlayerController>();
			await UniTask.WaitUntil(()=> _playerController.IsPlayerInitialized);

			_renderers = GetComponentsInChildren<Renderer>();

			if (_playerController.IsLocalPlayer)
			{
				SetColor(Color.blue);
			}
			else
			{
				SetColor(Color.red);
			}
		}

		private void SetColor(Color col)
		{
			foreach (var ren in _renderers)
			{
				ren.material.color = col;
			}
		}
	}
}