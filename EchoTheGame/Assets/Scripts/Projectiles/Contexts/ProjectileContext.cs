using Fusion;
using Project.Echo.Caches;
using Project.Echo.Projectiles.Structs;
using UnityEngine;

namespace Project.Echo.Projectiles.Contexts
{
	public class ProjectileContext
	{
		public NetworkRunner Runner;
		public ObjectCache Cache;
		public PlayerRef InputAuthority;
		public int OwnerObjectInstanceID;

		// Barrel transform represents position from which projectile visuals should fly out
		// (actual projectile fire calculations are usually done from different point, for example camera)
		public Transform BarrelTransform;

		public float FloatTick;
		public bool Interpolate;
		public ProjectileInterpolationData InterpolationData;
	}
}