using Fusion;
using UnityEngine;

namespace Project.Echo.Projectiles.Utilities
{
	public static class ProjectileUtility
	{
		public static bool ProjectileCast(NetworkRunner runner, PlayerRef owner, Vector3 firePosition, Vector3 direction, float distance, LayerMask hitMask, out LagCompensatedHit hit)
		{
			var hitOptions = HitOptions.IncludePhysX | HitOptions.SubtickAccuracy | HitOptions.IgnoreInputAuthority;
			return runner.LagCompensation.Raycast(firePosition, direction, distance, owner, out hit, hitMask, hitOptions);
		}

		public static bool CircleCast(NetworkRunner runner, PlayerRef owner, Vector3 firePosition, Vector3 direction, float distance, float radius, int numberOfRays, LayerMask hitMask, out LagCompensatedHit hit)
		{
			hit = default;

			float angleStep = numberOfRays > 2 ? (2f * Mathf.PI) / (numberOfRays - 1) : 0f;
			Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.LookRotation(direction));

			for (int i = 0; i < numberOfRays; i++)
			{
				Vector3 position = firePosition;

				// First ray is always directly in the center
				if (i > 0)
				{
					float angle = angleStep * (i - 1);
					var offset = new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 0f);
					position += rotationMatrix.MultiplyPoint3x4(offset);
				}

				if (ProjectileCast(runner, owner, position, direction, distance, hitMask, out LagCompensatedHit currentHit) == true)
				{
					if (hit.Type == HitType.None || currentHit.Distance < hit.Distance)
					{
						hit = currentHit;
					}
				}
			}

			return hit.Type != HitType.None;
		}
	}
}
