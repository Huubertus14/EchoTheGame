using Project.Echo.Projectiles.Enums;

namespace Project.Echo.Projectiles.Interfaces
{
	public interface IHitTarget
	{
		bool IsActive { get; }

		void ProcessHit(ref HitData hit);
	}
}