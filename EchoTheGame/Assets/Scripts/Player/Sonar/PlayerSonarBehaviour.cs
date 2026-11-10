using UnityEngine;
using Fusion;
using Project.Echo.Player.Controls.Data;
using Project.Echo.Player;
using Project.Echo.Player.Visuals;
using Project.Echo.Tuning;

public class PlayerSonarBehaviour : NetworkBehaviour, IRespawnAble
{
    [SerializeField] private PlayerSonarSettings settings;
    [Networked] private TickTimer NextPing { get; set; }
    [Networked] private NetworkBool PingHeld { get; set; }
    private PlayerHealthController health;
    private TickTimer nextCollisionPing;

    public override void Spawned()
    {
        // The old sparse, colliding particles no longer control visibility or draw another wave.
        var particles = GetComponent<ParticleSystem>();
        var main = particles.main;
        main.playOnAwake = false;
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        health = GetComponentInParent<PlayerHealthController>();
        nextCollisionPing = TickTimer.None;
    }

    public void EmitCollision(Vector3 contactPoint, Vector3 normal)
    {
        if (!HasStateAuthority || !Runner.IsForward || !nextCollisionPing.ExpiredOrNotRunning(Runner)) return;
        nextCollisionPing = TickTimer.CreateFromSeconds(Runner, settings.collisionCooldown);
        RPC_PlayCollisionPulse(contactPoint + normal.normalized * settings.collisionSurfaceOffset);
    }

    public void Respawn()
    {
        if (!HasStateAuthority) return;
        NextPing = TickTimer.None;
        PingHeld = false;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayCollisionPulse(Vector3 origin)
    {
        var player = GetComponentInParent<PlayerNetworkedController>();
        var visual = player ? player.GetComponentInChildren<PlayerVisualController>() : null;
        WorldSonarPulse.Emit(origin, visual ? visual.PlayerColor : PlayerVisualController.DefaultPlayerColor, settings.collisionPulse);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !Runner.IsForward || !health || !health.Alive ||
            NetworkedPlayerGameModeController.Coordinator?.Phase != 2) return;
        if (!GetInput(out NetworkPlayerMovementData data)) return;
        bool pressed = data.IsPing && !PingHeld;
        PingHeld = data.IsPing;
        if (pressed && NextPing.ExpiredOrNotRunning(Runner))
        {
            NextPing = TickTimer.CreateFromSeconds(Runner, settings.cooldown);
            RPC_PlayParticleEffect(transform.position);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayParticleEffect(Vector3 origin)
    {
        var player = GetComponentInParent<PlayerNetworkedController>();
        var visual = player ? player.GetComponentInChildren<PlayerVisualController>() : null;
        Color color = visual ? visual.PlayerColor : (HasInputAuthority ? PlayerVisualController.DefaultPlayerColor : Color.red);
        WorldSonarPulse.Emit(origin, color, settings.pulse);

        // Only the scanning captain receives contact visibility from this wave.
        if (!HasInputAuthority || settings.contactRevealDuration <= 0f) return;
        float range = settings.pulse.range;
        foreach (var other in Runner.ActivePlayers)
        {
            if (other == Object.InputAuthority || !Runner.TryGetPlayerObject(other, out var target) || !target ||
                !target.TryGetBehaviour<PlayerHealthController>(out var targetHealth) || !targetHealth.Alive) continue;
            Vector3 offset = target.transform.position - origin;
            offset.y = 0f;
            if (offset.sqrMagnitude <= range * range)
                target.GetComponentInChildren<PlayerVisualController>()?.RevealContact(settings.contactRevealDuration);
        }
    }
}
