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

		public Transform BulletBegin;

		public float FloatTick;
		public bool Interpolate;
		public ProjectileInterpolationData InterpolationData;
	}
}