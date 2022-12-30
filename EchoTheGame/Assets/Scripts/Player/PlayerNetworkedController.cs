using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using Project.Echo.Player.Visuals;
using System;

namespace Project.Echo.Player
{
    public class PlayerNetworkedController : NetworkBehaviour
    {
        [Networked(OnChanged = nameof(ActivePlayerChanged))]
        public bool IsPlayerActive { get; set; }

		public Action OnPlayerDied;
		public Action OnRespawned;

		private PlayerVisualController _visualController;
		[Networked]
		private TickTimer _respawnTimer { get; set; }

		public override void Spawned()
		{
			base.Spawned();
			_respawnTimer = TickTimer.None;
			_visualController = GetComponentInChildren<PlayerVisualController>();
			IsPlayerActive = true;
		}

		private void EnablePlayer()
		{
			IsPlayerActive = true;
		}

		public static void ActivePlayerChanged(Changed<PlayerNetworkedController> changed)
		{
			var beh = changed.Behaviour;
			beh._visualController.gameObject.SetActive(beh.IsPlayerActive);
		}

		public override void FixedUpdateNetwork()
		{
			if (_respawnTimer.Expired(Runner))
			{
				EnablePlayer();
				OnRespawned?.Invoke();
				_respawnTimer = TickTimer.None;
			}
		}

		public void DisablePlayer()
		{
			IsPlayerActive = false;
		}

		public void RespawnPlayer(float delaySeconds)
		{
			OnPlayerDied?.Invoke();
			_respawnTimer = TickTimer.CreateFromSeconds(Runner, delaySeconds);
		}
	}
}