using UnityEngine;
using Project.Echo.Tuning;

namespace Project.Echo.Environment
{
    /// <summary>Convex, counter-clockwise shoreline in world XZ. Shared by prediction and authority.</summary>
    public sealed class UnderwaterArena : MonoBehaviour
    {
        public static UnderwaterArena Instance { get; private set; }
        public Vector2[] shoreline;
        public struct MovementContact
        {
            public bool blocked;
            public Vector3 point;
            public Vector3 normal;
        }
        private readonly RaycastHit[] hits = new RaycastHit[128];
        private readonly Collider[] overlaps = new Collider[128];

        private void Awake() => Instance = this;
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public bool Contains(Vector3 position) => Contains(position, ArenaSettings.Default.playerRadius);

        public bool Contains(Vector3 position, float margin)
        {
            Vector2 p = new Vector2(position.x, position.z);
            for (int i = 0; i < shoreline.Length; i++)
            {
                Vector2 a = shoreline[i], edge = shoreline[(i + 1) % shoreline.Length] - a;
                Vector2 inward = new Vector2(-edge.y, edge.x).normalized;
                if (Vector2.Dot(p - a, inward) < margin - 0.001f) return false;
            }
            return true;
        }

        public Vector3 Constrain(Vector3 position) => Constrain(position, ArenaSettings.Default.playerRadius);

        public Vector3 Constrain(Vector3 position, float margin)
        {
            Vector2 p = new Vector2(position.x, position.z);
            for (int pass = 0; pass < 64; pass++)
            {
                bool changed = false;
                for (int i = 0; i < shoreline.Length; i++)
                {
                    Vector2 a = shoreline[i], edge = shoreline[(i + 1) % shoreline.Length] - a;
                    Vector2 inward = new Vector2(-edge.y, edge.x).normalized;
                    float distance = Vector2.Dot(p - a, inward);
                    if (distance < margin) { p += inward * (margin - distance + 0.0001f); changed = true; }
                }
                if (!changed) break;
            }
            return new Vector3(p.x, 0, p.y);
        }

        public Vector3 Move(Vector3 origin, Vector3 target)
            => Move(origin, target, out _);

        public Vector3 Move(Vector3 origin, Vector3 target, out MovementContact contact)
        {
            contact = default;
            var settings = ArenaSettings.Default;
            float playerRadius = settings.playerRadius;
            origin = Constrain(origin, playerRadius);
            Vector3 constrained = Constrain(target, playerRadius);
            Vector3 boundaryNormal = constrained - target;
            boundaryNormal.y = 0;
            if (boundaryNormal.sqrMagnitude > 0.000001f)
                contact = new MovementContact { blocked = true, normal = boundaryNormal.normalized,
                    point = constrained - boundaryNormal.normalized * playerRadius };
            target = constrained;
            Vector3 delta = target - origin;
            float distance = delta.magnitude;
            if (distance < 0.0001f) return origin;
            int count = Physics.SphereCastNonAlloc(origin, playerRadius, delta / distance, hits,
                distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (count == hits.Length) return origin;
            float allowed = distance;
            for (int i = 0; i < count; i++)
                if (hits[i].collider.transform.IsChildOf(transform))
                {
                    float safeDistance = Mathf.Max(0, hits[i].distance - settings.collisionClearance);
                    if (safeDistance < allowed)
                    {
                        allowed = safeDistance;
                        contact = new MovementContact { blocked = true, point = hits[i].point, normal = hits[i].normal };
                    }
                }
            return Constrain(origin + delta / distance * allowed, playerRadius);
        }

        public bool IsSpawnClear(Vector3 position)
        {
            float clearance = Mathf.Max(ArenaSettings.Default.spawnClearance, ArenaSettings.Default.playerRadius);
            if (!Contains(position, clearance)) return false;
            int count = Physics.OverlapSphereNonAlloc(position, clearance, overlaps,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (count == overlaps.Length) return false;
            for (int i = 0; i < count; i++)
                if (overlaps[i].transform.IsChildOf(transform)) return false;
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            if (shoreline == null) return;
            Gizmos.color = Color.cyan;
            for (int i = 0; i < shoreline.Length; i++)
            {
                Vector2 a = shoreline[i], b = shoreline[(i + 1) % shoreline.Length];
                Gizmos.DrawLine(new Vector3(a.x, 0, a.y), new Vector3(b.x, 0, b.y));
            }
        }
    }
}
