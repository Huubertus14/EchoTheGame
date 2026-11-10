using Fusion;
using Project.Echo.Projectiles.Contexts;
using Project.Echo.Projectiles.Structs;
using UnityEngine;

namespace Project.Echo.Projectiles.Behaviours
{
    public sealed class HunterTorpedo : SimpleKinematicProjectile
    {
        public override bool NeedsInterpolationData => true;

        public override ProjectileData GetFireData(NetworkRunner runner, Vector3 firePosition, Vector3 fireDirection)
        {
            var data = base.GetFireData(runner, firePosition, fireDirection);
            data.Guidance.Position = firePosition;
            data.Guidance.Direction = fireDirection.normalized;
            return data;
        }

        public override void OnFixedUpdate(ProjectileContext context, ref ProjectileData data)
        {
            var runner = context.Runner;
            int age = runner.Tick - data.FireTick;
            if (age <= 0) return;
            if (HasExpired(runner, data))
            {
                data.IsFinished = true;
                return;
            }

            ref var guidance = ref data.Guidance;
            if (age == 1)
                guidance.Target = FindTarget(context, guidance.Position, guidance.Direction);

            if (!guidance.Target.IsNone)
            {
                if (TryGetTarget(runner, guidance.Target, guidance.Position, out var offset) &&
                    Vector3.Dot(guidance.Direction, offset) > 0f &&
                    SonarPropagation.HasClearPath(runner.GetPhysicsScene(), guidance.Position, guidance.Position + offset))
                    guidance.Direction = Vector3.RotateTowards(guidance.Direction, offset.normalized,
                        Tuning.turnRate * Mathf.Deg2Rad * runner.DeltaTime, 0f);
                else
                    guidance.Target = PlayerRef.None;
            }

            float step = data.FireVelocity.magnitude * runner.DeltaTime;
            Sweep(context, ref data, guidance.Position, guidance.Direction, step);
            guidance.Position += guidance.Direction * step;
        }

        private PlayerRef FindTarget(ProjectileContext context, Vector3 origin, Vector3 direction)
        {
            PlayerRef nearest = PlayerRef.None;
            float bestDistance = Tuning.lockRange * Tuning.lockRange;
            float cone = Mathf.Cos(Tuning.lockHalfAngle * Mathf.Deg2Rad);
            foreach (var player in context.Runner.ActivePlayers)
            {
                if (player == context.InputAuthority ||
                    !TryGetTarget(context.Runner, player, origin, out var offset) ||
                    offset.sqrMagnitude >= bestDistance || Vector3.Dot(direction, offset.normalized) < cone ||
                    !SonarPropagation.HasClearPath(context.Runner.GetPhysicsScene(), origin, origin + offset)) continue;
                bestDistance = offset.sqrMagnitude;
                nearest = player;
            }
            return nearest;
        }

        private static bool TryGetTarget(NetworkRunner runner, PlayerRef player, Vector3 origin, out Vector3 offset)
        {
            offset = default;
            if (!runner.TryGetPlayerObject(player, out var target) || !target ||
                !target.TryGetBehaviour<PlayerHealthController>(out var health) || !health.Alive) return false;
            offset = target.transform.position - origin;
            offset.y = 0f;
            return offset.sqrMagnitude > 0.0001f;
        }

        protected override Vector3 GetRenderPosition(ProjectileContext context, ref ProjectileData data)
        {
            if (context.Interpolate)
            {
                var samples = context.InterpolationData;
                if (samples.From.IsActive && samples.From.FireTick == data.FireTick &&
                    samples.To.IsActive && samples.To.FireTick == data.FireTick)
                    return Vector3.Lerp(samples.From.Guidance.Position, samples.To.Guidance.Position, samples.Alpha);
            }
            return data.Guidance.Position;
        }
    }
}
