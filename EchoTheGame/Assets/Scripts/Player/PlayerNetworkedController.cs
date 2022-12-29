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
        [Networked]
        public bool IsPlayerActive { get; set; }
        private bool _localStatusActive;

		public Action OnPlayerDied;
		public Action OnRespawned;

		private PlayerVisualController _visualController;
		private TickTimer _respawnTimer;

		public override void Spawned()
		{
			base.Spawned();
			_respawnTimer = TickTimer.None;
			_visualController = GetComponentInChildren<PlayerVisualController>();
			IsPlayerActive = true;
			_localStatusActive = true; 
		}

		private void EnablePlayer()
		{
			_localStatusActive = true;
		}

		public override void FixedUpdateNetwork()
		{
			base.FixedUpdateNetwork();
			if (_localStatusActive != IsPlayerActive)
			{
				_visualController.gameObject.SetActive(_localStatusActive);
				IsPlayerActive = _localStatusActive;
			}

			if (!_localStatusActive && _respawnTimer.ExpiredOrNotRunning(Runner))
			{
				EnablePlayer();
				OnRespawned?.Invoke();
			}
		}

		public void DisablePlayer()
		{
			_localStatusActive = false;
		}

		public void RespawnPlayer(float delaySeconds)
		{
			OnPlayerDied?.Invoke();
			_respawnTimer = TickTimer.CreateFromSeconds(Runner, delaySeconds);
		}
	}
}