using Fusion;
using Project.Echo.Projectiles.Interfaces;
using UnityEngine;

namespace Project.Echo.Projectiles.Enums
{
	public struct HitData
	{
		public ProjectileHitAction Action;
		public float Amount;
		public bool IsFatal;
		public Vector3 Position;
		public Vector3 Direction;
		public Vector3 Normal;
		public PlayerRef InstigatorRef;
		public IHitInstigator Instigator;
		public IHitTarget Target;
		public ProjectileHitType HitType;
	}
}