using System;
using System.Collections;
using Fusion;
using Project.Echo.Player.Controls.Data;
using Project.Echo.Player;
using Project.Echo.Player.Visuals;
using Project.Echo.Projectiles;
using Project.Echo.Projectiles.Behaviours;
using UnityEngine;


namespace Project.Echo.Weapons
{
    public class BasicCannonBehaviour : NetworkBehaviour
    {
		public Transform GetBulletTransform => _bulletSpawnPoint;

		[SerializeField] private Transform _bulletSpawnPoint;
		private ProjectileManager _projectileManager;
		private PlayerNetworkedController _player;
		private PlayerHealthController _health;
		[Networked] private TickTimer NextShot { get; set; }
		
		private void Awake()
		{
			_projectileManager = GetComponent<ProjectileManager>();
			_player = GetComponent<PlayerNetworkedController>();
			_health = GetComponent<PlayerHealthController>();
		}

		public override void FixedUpdateNetwork()
		{
			if (!HasStateAuthority || NetworkedPlayerGameModeController.Coordinator?.Phase != 2 ||
				!_health.Alive) return;

			if (NextShot.ExpiredOrNotRunning(Runner) && GetInput(out NetworkPlayerMovementData data) && data.IsShooting)
			{
				_projectileManager.AddProjectile(_player.Tuning.primaryProjectile);
				NextShot = TickTimer.CreateFromSeconds(Runner, _player.Tuning.fireCooldown);
			}
		}

		private void LateUpdate()
		{
			_projectileManager.OnRender();
		}
	}
}
