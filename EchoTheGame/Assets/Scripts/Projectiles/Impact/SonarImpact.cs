using UnityEngine;
using Fusion;
using Project.Echo.Player.Visuals;

[RequireComponent(typeof(ParticleSystem))]
public class SonarImpact : NetworkBehaviour
{
    [SerializeField] private WorldSonarPulse.Settings pulse = new WorldSonarPulse.Settings { range = 9f, duration = 0.6f };
    private bool emitted;

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
        Color color = new Color(0.6078f, 0.9137f, 0.8863f);
        if (Runner.TryGetPlayerObject(player, out NetworkObject networkObject))
        {
            var visual = networkObject.GetComponentInChildren<PlayerVisualController>();
            if (visual != null) color = visual.PlayerColor;
        }
        WorldSonarPulse.Emit(transform.position + surfaceNormal.normalized * 0.08f, color, pulse);
    }
}
