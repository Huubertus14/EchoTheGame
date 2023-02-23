using Fusion;
using Project.Echo.Player.Visuals;
using System;
using Project.Echo.Setting;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;

namespace Project.Echo.Player
{
    public class PlayerNetworkedController : NetworkBehaviour
    {
		public static PlayerNetworkedController LocalPlayer;
		public static Action<NetworkRunner> LocalPlayerSpawned;
		public PlayerSettings PlayerSettings => Settings.Player;

		[Networked]
		public NetworkString<_32> PlayerName { get; set; }

		[Networked(OnChanged = nameof(ActivePlayerChanged))]
        private bool _isPlayerActive { get; set; }
		
		[Networked]
		private TickTimer _respawnTimer { get; set; }

		[Networked]
		public int Token { get; set; }

		public Action OnPlayerDied;

		private PlayerVisualController _visualController;
		private PlayerHealthController _healthController;
		private IHiddenRespawnAble[] _hiddenRespawnAbles; 
		private IRespawnAble[] _respawnAbles;
		private Canvas _localPlayerCanvas;

		public override void Spawned()
		{
			_localPlayerCanvas = GetComponentInChildren<Canvas>(true);

			if (Object.HasInputAuthority)
			{
				LocalPlayer = this;
				RPC_SetName(PlayerSettings.PlayerName);
				LocalPlayerSpawned?.Invoke(Runner);
				_localPlayerCanvas.gameObject.SetActive(true);
				_localPlayerCanvas.worldCamera = Camera.main;
			}
			else
			{
				_localPlayerCanvas.gameObject.SetActive(false);
			}
			
			Runner.SetPlayerObject(Object.InputAuthority, Object);
			_hiddenRespawnAbles = GetComponentsInChildren<IHiddenRespawnAble>();
			_respawnAbles = GetComponentsInChildren<IRespawnAble>();
			_respawnTimer = TickTimer.None;
			_visualController = GetComponentInChildren<PlayerVisualController>();
			_healthController = GetComponentInChildren<PlayerHealthController>();
			_isPlayerActive = true;
		}

		private void EnablePlayer()
		{
			
			_isPlayerActive = true;
		}

		[Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
		private void RPC_SetName(string Name)
		{
			PlayerName = Name;
		}

		public static void ActivePlayerChanged(Changed<PlayerNetworkedController> changed)
		{
			var beh = changed.Behaviour;
			beh._visualController.gameObject.SetActive(beh._isPlayerActive);
		}

		private void DisablePlayer()
		{
			_isPlayerActive = false;
		}

		public void RespawnPlayer(float delaySeconds)
		{
			DisablePlayer();
			OnPlayerDied?.Invoke();
			StartCoroutine(RespawnLoop(delaySeconds));
		}

		private IEnumerator RespawnLoop(float delay)
		{
			DisablePlayer();
			yield return new WaitForSeconds(delay/2);

			foreach (IHiddenRespawnAble respawnObject in _hiddenRespawnAbles)
			{
				respawnObject.Respawn(); //not the health player
			}
			yield return new WaitForSeconds(delay / 2); 
			foreach (IRespawnAble respawnObject in _respawnAbles)
			{
				respawnObject.Respawn(); //not the health player
			}
			_healthController.Respawn();
			EnablePlayer();

		}
	}
}