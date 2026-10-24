using UnityEngine;
using Fusion;
using Project.Echo.Player.Controls.Data;
using Project.Echo.Player;
using Project.Echo.Player.Visuals;

public class PlayerSonarBehaviour : NetworkBehaviour
{
    [SerializeField] private WorldSonarPulse.Settings pulse = new WorldSonarPulse.Settings();
    [SerializeField, Min(0.1f)] private float cooldown = 0.4f;
    private TickTimer nextPing;
    private bool pingHeld;
    [SerializeField] private WorldSonarPulse.Settings collisionPulse = new WorldSonarPulse.Settings { range = 9f, duration = 0.6f };
    [SerializeField, Min(0.1f)] private float collisionCooldown = 0.65f;
    private TickTimer nextCollisionPing;

    public override void Spawned()
    {
        // The old sparse, colliding particles no longer control visibility or draw another wave.
        var particles = GetComponent<ParticleSystem>();
        var main = particles.main;
        main.playOnAwake = false;
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        nextPing = TickTimer.None;
        nextCollisionPing = TickTimer.None;
    }

    public void EmitCollision(Vector3 contactPoint, Vector3 normal)
    {
        if (!HasStateAuthority || !Runner.IsForward || !nextCollisionPing.ExpiredOrNotRunning(Runner)) return;
        nextCollisionPing = TickTimer.CreateFromSeconds(Runner, collisionCooldown);
        RPC_PlayCollisionPulse(contactPoint + normal.normalized * 0.08f);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayCollisionPulse(Vector3 origin)
    {
        var player = GetComponentInParent<PlayerNetworkedController>();
        var visual = player ? player.GetComponentInChildren<PlayerVisualController>() : null;
        WorldSonarPulse.Emit(origin, visual ? visual.PlayerColor : PlayerVisualController.DefaultPlayerColor, collisionPulse);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority || !Runner.IsForward) return;
        if (!GetInput(out NetworkPlayerMovementData data)) return;
        bool pressed = data.IsPing && !pingHeld;
        pingHeld = data.IsPing;
        if (pressed && nextPing.ExpiredOrNotRunning(Runner))
        {
            nextPing = TickTimer.CreateFromSeconds(Runner, cooldown);
            RPC_PlayParticleEffect(transform.position);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_PlayParticleEffect(Vector3 origin)
    {
        var player = GetComponentInParent<PlayerNetworkedController>();
        var visual = player ? player.GetComponentInChildren<PlayerVisualController>() : null;
        Color color = visual ? visual.PlayerColor : (HasInputAuthority ? PlayerVisualController.DefaultPlayerColor : Color.red);
        WorldSonarPulse.Emit(origin, color, pulse);
    }
}
