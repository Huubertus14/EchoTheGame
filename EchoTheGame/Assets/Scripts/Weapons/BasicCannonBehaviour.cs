using System.Collections;
using Fusion;
using Project.Echo.Player.Controls.Data;
using Project.Echo.Projectiles;
using Project.Echo.Projectiles.Behaviours;
using UnityEngine;


namespace Project.Echo.Weapons
{
    public class BasicCannonBehaviour : NetworkBehaviour
    {
		public Transform GetBulletTransform => _bulletSpawnPoint;

		[SerializeField] private Transform _bulletSpawnPoint;
		[SerializeField] private Projectile _bulletPrefab;

		private ProjectileManager _projectileManager;
		private TickTimer _tickTimer;

		private void Awake()
		{
			_projectileManager = GetComponent<ProjectileManager>();
			_tickTimer = new TickTimer();
		}

		public override void FixedUpdateNetwork()
		{
			if (_tickTimer.ExpiredOrNotRunning(Runner) && GetInput(out NetworkPlayerMovementData data) && data.IsShooting)
			{
				_projectileManager.AddProjectile(_bulletPrefab);
				_tickTimer = TickTimer.CreateFromSeconds(Runner, 0.8f);
			}
		}

		private void LateUpdate()
		{
			_projectileManager.OnRender();
		}
	}
}
