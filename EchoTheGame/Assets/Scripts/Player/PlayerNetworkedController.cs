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

		public PlayerStats PlayerStats { get; private set; }
		public PlayerSettings PlayerSettings => Settings.Player;

		[Networked]
		public NetworkString<_32> PlayerName { get; set; }

		[Networked(OnChanged = nameof(ActivePlayerChanged))]
        private bool _isPlayerActive { get; set; }
		
		[Networked]
		private TickTimer _respawnTimer { get; set; }

		public Action OnPlayerDied;
		public Action OnRespawned;

		private PlayerVisualController _visualController;
		
		public async override void Spawned()
		{
			if (Object.HasInputAuthority)
			{
				LocalPlayer = this;
				RPC_SetName(PlayerSettings.PlayerName); 
				List<Task> initTasks = new();
				var scene = SceneManager.GetActiveScene();
				foreach (GameObject root in scene.GetRootGameObjects())
				{
					foreach (var item in root.GetComponentsInChildren<IPlayerJoinedInitialization>(true))
					{
						initTasks.Add(item.Init(Runner));
					}
				}

				await Task.WhenAll(initTasks);
			}
			
			PlayerStats = new PlayerStats();

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
			PlayerName = PlayerSettings.PlayerName;
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
				OnRespawned?.Invoke();
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