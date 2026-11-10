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
		public override void OnFixedUpdate(ProjectileContext context, ref ProjectileData data)
		{
			Vector3 previousPosition = GetMovePosition(context.Runner, ref data, context.Runner.Tick - 1);
			var nextPosition = GetMovePosition(context.Runner, ref data, context.Runner.Tick);

			var direction = nextPosition - previousPosition;
			float distance = direction.magnitude;
		
			// Normalize
			if (distance <= Mathf.Epsilon)
			{
				base.OnFixedUpdate(context, ref data);
				return;
			}
			direction /= distance;

			float length = Tuning.length;
			if (length > 0f)
			{
				float elapsedDistanceSqr = (previousPosition - data.FirePosition).sqrMagnitude;
				float projectileLength = Mathf.Min(length, Mathf.Sqrt(elapsedDistanceSqr));

				previousPosition -= direction * projectileLength;
				distance += projectileLength;
			}

			Sweep(context, ref data, previousPosition, direction, distance);
			base.OnFixedUpdate(context, ref data);
		}

		protected void Sweep(ProjectileContext context, ref ProjectileData data, Vector3 origin, Vector3 direction, float distance)
		{
			if (ProjectileUtility.ProjectileCast(context.Runner, context.InputAuthority, origin, direction, distance, Tuning.hitMask, out LagCompensatedHit hit))
			{
				SpawnImpact(context, ref data, hit.Point, (hit.Normal + -direction) * 0.5f);

				var healthController = hit.GameObject.transform.root.GetComponentInChildren<PlayerHealthController>();

				if (healthController != null)
				{
					healthController.HitPlayer(Tuning.damage, context.InputAuthority);
				}

				data.IsFinished = true;
			}

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
