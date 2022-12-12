using Fusion;
using Project.Echo.Projectiles.Enums;
using Project.Echo.Projectiles.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Echo.Projectiles.Utilities
{
	public static class HitUtility 
	{
		public static HitData ProcessHit(PlayerRef instigatorRef, Vector3 direction, LagCompensatedHit hit, float baseDamage, ProjectileHitType hitType)
		{
			var target = GetHitTarget(hit.Hitbox, hit.Collider);
			if (target == null)
				return default;

			HitData hitData = default;

			hitData.Action = ProjectileHitAction.Damage;
			hitData.Amount = baseDamage;
			hitData.Position = hit.Point;
			hitData.Normal = hit.Normal;
			hitData.Direction = direction;
			hitData.Target = target;
			hitData.InstigatorRef = instigatorRef;
			hitData.HitType = hitType;

			return ProcessHit(ref hitData);
}

		public static HitData ProcessHit(NetworkBehaviour instigator, Vector3 direction, LagCompensatedHit hit, float baseDamage, ProjectileHitType hitType)
		{
			var target = GetHitTarget(hit.Hitbox, hit.Collider);
			if (target == null)
				return default;

			HitData hitData = default;

			hitData.Action = ProjectileHitAction.Damage;
			hitData.Amount = baseDamage;
			hitData.Position = hit.Point;
			hitData.Normal = hit.Normal;
			hitData.Direction = direction;
			hitData.Target = target;
			hitData.InstigatorRef = instigator != null ? instigator.Object.InputAuthority : default;
			hitData.Instigator = instigator != null ? instigator.GetComponent<IHitInstigator>() : null;
			hitData.HitType = hitType;

			return ProcessHit(ref hitData);
}

		public static HitData ProcessHit(NetworkBehaviour instigator, Collider collider, float damage, ProjectileHitType hitType)
		{
			var target = GetHitTarget(null, collider);
			if (target == null)
				return default;

			HitData hitData = default;

			hitData.Action = ProjectileHitAction.Damage;
			hitData.Amount = damage;
			hitData.InstigatorRef = instigator.Object.InputAuthority;
			hitData.Instigator = instigator.GetComponent<IHitInstigator>();
			hitData.Position = collider.transform.position;
			hitData.Normal = (instigator.transform.position - collider.transform.position).normalized;
			hitData.Direction = -hitData.Normal;
			hitData.Target = target;
			hitData.HitType = hitType;

			return ProcessHit(ref hitData);
		}

		public static HitData ProcessHit(ref HitData hitData)
		{
			hitData.Target.ProcessHit(ref hitData);

			// For local debug targets we show hit feedback immediately
			// if (hitData.Instigator != null && hitData.Target is Health == false)
			// {
			// 	hitData.Instigator.HitPerformed(hitData);
			// }

			return hitData;
		}

		private static IHitTarget GetHitTarget(Hitbox hitbox, Collider collider)
		{
			if (hitbox != null)
				return hitbox.Root.GetComponent<IHitTarget>();

			if (collider == null)
				return null;

			//if (ObjectLayerMask.HitTargets.value.IsBitSet(collider.gameObject.layer) == false) //TODO check if something is in the right layer mask here
			//	return null;

			return collider.GetComponentInParent<IHitTarget>();
		}
	}
}