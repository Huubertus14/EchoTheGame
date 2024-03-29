using Fusion;
using Project.Echo.Player.Visuals;
using System;
using Project.Echo.Setting;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Threading.Tasks;

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

			_respawnAbles = GetComponentsInChildren<IRespawnAble>();
			_respawnTimer = TickTimer.None;
			_visualController = GetComponentInChildren<PlayerVisualController>();
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

		public override void FixedUpdateNetwork()
		{
			if (_respawnTimer.Expired(Runner))
			{
				_respawnTimer = TickTimer.None;

				foreach (IRespawnAble respawnObject in _respawnAbles)
				{
					respawnObject.Respawn();
				}
				EnablePlayer();
			}
		}

		public void DisablePlayer()
		{
			_isPlayerActive = false;
		}

		public void RespawnPlayer(float delaySeconds)
		{
			_respawnTimer = TickTimer.CreateFromSeconds(Runner, delaySeconds);
			OnPlayerDied?.Invoke();
		}
	}
}