using UnityEngine;
using Fusion;
using Project.Echo.Player.Visuals;
using Project.Echo.Tuning;

[RequireComponent(typeof(ParticleSystem))]
public class SonarImpact : NetworkBehaviour
{
    [Networked] private TickTimer Lifetime { get; set; }
    private bool emitted;

    public override void Spawned()
    {
        if (HasStateAuthority) Lifetime = TickTimer.CreateFromSeconds(Runner, ImpactSonarSettings.Default.emitterLifetime);
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && Lifetime.Expired(Runner)) Runner.Despawn(Object);
    }

    private void Awake()
    {
        var particles = GetComponent<ParticleSystem>();
        var main = particles.main;
        main.playOnAwake = false;
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateColor(PlayerRef player, Vector3 surfaceNormal)
    {
        if (emitted) return;
        emitted = true;
        Color color = PlayerVisualController.DefaultPlayerColor;
        if (Runner.TryGetPlayerObject(player, out NetworkObject networkObject))
        {
            var visual = networkObject.GetComponentInChildren<PlayerVisualController>();
            if (visual != null) color = visual.PlayerColor;
        }
        var settings = ImpactSonarSettings.Default;
        WorldSonarPulse.Emit(transform.position + surfaceNormal.normalized * settings.surfaceOffset, color, settings.pulse);
    }
}
