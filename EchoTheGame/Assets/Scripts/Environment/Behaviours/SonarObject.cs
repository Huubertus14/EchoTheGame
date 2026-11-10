using UnityEngine;
using Project.Echo.Tuning;

/// <summary>Marks reusable sonar geometry. Surface reveal follows the original emitted world-space pulse.</summary>
[RequireComponent(typeof(MeshRenderer))]
public class SonarObject : MonoBehaviour, IHitSonarAble
{
    // Compatibility for callers outside the normal ping/impact path. No per-object animation.
    public void HitBySonar(Vector3 firstHitPosition, Color hitColor)
    {
        WorldSonarPulse.Emit(firstHitPosition, hitColor, ImpactSonarSettings.Default.pulse);
    }
}
