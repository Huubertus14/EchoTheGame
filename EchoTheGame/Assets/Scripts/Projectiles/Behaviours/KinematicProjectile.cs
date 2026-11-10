using DG.Tweening;
using Fusion;
using Project.Echo.Projectiles.Contexts;
using Project.Echo.Projectiles.Structs;
using UnityEngine;

namespace Project.Echo.Projectiles.Behaviours
{
    public abstract class KinematicProjectile : Projectile
    {
		private Vector3 _startOffset;
		private float _interpolationTime;

		// Projectile INTERFACE

		public override ProjectileData GetFireData(NetworkRunner runner, Vector3 firePosition, Vector3 fireDirection)
		{
			return new ProjectileData()
			{
				FirePosition = firePosition,
				FireVelocity = fireDirection * Tuning.startSpeed,
			};
		}

		public override void OnFixedUpdate(ProjectileContext context, ref ProjectileData data)
		{
			if (HasExpired(context.Runner, data))
			{
				data.IsFinished = true;
			}
		}

		protected bool HasExpired(NetworkRunner runner, ProjectileData data)
		{
			float lifetime = Mathf.Min(Tuning.maxTime, Tuning.maxDistance / data.FireVelocity.magnitude);
			int lifetimeTicks = Mathf.Max(1, Mathf.RoundToInt(lifetime * runner.Simulation.Config.TickRate));
			return runner.Tick >= data.FireTick + lifetimeTicks;
		}

		protected override void OnActivated(ProjectileContext context, ref ProjectileData data)
		{
			base.OnActivated(context, ref data);

			transform.position = context.BulletBegin.position;
			transform.rotation = Quaternion.LookRotation(data.FireVelocity);

			_startOffset = context.BulletBegin.position - data.FirePosition;
			_interpolationTime = 0f;
		}

		public override void OnRender(ProjectileContext context, ref ProjectileData data)
		{
			base.OnRender(context, ref data);
			if (data.IsFinished == true)
			{
				SpawnImpactVisual(context, ref data);
				IsFinished = true;
				return;
			}

			var targetPosition = GetRenderPosition(context, ref data);
			float interpolationProgress = 0f;

			if (targetPosition != data.FirePosition)
			{
				// Do not start interpolation until projectile should actually move
				_interpolationTime += Time.deltaTime;
				interpolationProgress = Tuning.interpolationDuration > 0f
					? Mathf.Clamp01(_interpolationTime / Tuning.interpolationDuration)
					: 1f;
			}

			interpolationProgress = DOVirtual.EasedValue(0f, 1f, interpolationProgress, Tuning.interpolationEase);
			var offset = Vector3.Lerp(_startOffset, Vector3.zero, interpolationProgress);

			var previousPosition = transform.position;
			var nextPosition = targetPosition + offset;
			var direction = nextPosition - previousPosition;

			transform.position = nextPosition;

			if (direction != Vector3.zero)
			{
				transform.rotation = Quaternion.LookRotation(direction);
			}
		}

		// PROTECTED METHODS

		protected abstract Vector3 GetRenderPosition(ProjectileContext context, ref ProjectileData data);
	}
}
