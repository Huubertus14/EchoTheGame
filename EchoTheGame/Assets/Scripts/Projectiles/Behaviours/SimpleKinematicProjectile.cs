using Fusion;
using Project.Echo.Projectiles.Contexts;
using Project.Echo.Projectiles.Enums;
using Project.Echo.Projectiles.Structs;
using Project.Echo.Projectiles.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using UnityEngine;

namespace Project.Echo.Projectiles.Behaviours
{
    public class SimpleKinematicProjectile : KinematicProjectile
{
		[SerializeField]
		private int _damage = 10;
		[SerializeField]
		private ProjectileHitType _hitType = ProjectileHitType.Projectile;
		[SerializeField]
		private LayerMask _hitMask;

		public override void OnFixedUpdate(ProjectileContext context, ref ProjectileData data)
		{
			var previousPosition = GetMovePosition(context.Runner, ref data, context.Runner.Tick - 1);
			var nextPosition = GetMovePosition(context.Runner, ref data, context.Runner.Tick);

			var direction = nextPosition - previousPosition;
			float distance = direction.magnitude;

			// Normalize
			direction /= distance;

			if (_length > 0f)
			{
				float elapsedDistanceSqr = (previousPosition - data.FirePosition).sqrMagnitude;
				float projectileLength = elapsedDistanceSqr > _length * _length ? _length : Mathf.Sqrt(elapsedDistanceSqr);

				previousPosition -= direction * projectileLength;
				distance += projectileLength;
			}

			if (ProjectileUtility.ProjectileCast(context.Runner, context.InputAuthority, previousPosition, direction, distance, _hitMask, out LagCompensatedHit hit) == true)
			{
				SpawnImpact(context, ref data, hit.Point, (hit.Normal + -direction) * 0.5f);

				var healthController = hit.GameObject.transform.root.GetComponentInChildren<PlayerHealthController>();

				if (healthController != null)
				{
					healthController.HitPlayer(_damage);
				}

				data.IsFinished = true;
			}

			base.OnFixedUpdate(context, ref data);
		}

		protected override Vector3 GetRenderPosition(ProjectileContext context, ref ProjectileData data)
		{
			return GetMovePosition(context.Runner, ref data, context.FloatTick);
		}

		private Vector3 GetMovePosition(NetworkRunner runner, ref ProjectileData data, float currentTick)
		{
			float time = (currentTick - data.FireTick) * runner.DeltaTime;

			if (time <= 0f)
				return data.FirePosition;

			return data.FirePosition + data.FireVelocity * time;
		}
	}
}
