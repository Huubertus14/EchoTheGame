using UnityEngine;

namespace Project.Echo.Environment
{
    /// <summary>Convex, counter-clockwise shoreline in world XZ. Shared by prediction and authority.</summary>
    public sealed class UnderwaterArena : MonoBehaviour
    {
        public static UnderwaterArena Instance { get; private set; }
        public Vector2[] shoreline;
        public const float PlayerRadius = 0.85f;
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

        public bool Contains(Vector3 position, float margin = PlayerRadius)
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

        public Vector3 Constrain(Vector3 position, float margin = PlayerRadius)
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
            origin = Constrain(origin);
            Vector3 constrained = Constrain(target);
            Vector3 boundaryNormal = constrained - target;
            boundaryNormal.y = 0;
            if (boundaryNormal.sqrMagnitude > 0.000001f)
                contact = new MovementContact { blocked = true, normal = boundaryNormal.normalized,
                    point = constrained - boundaryNormal.normalized * PlayerRadius };
            target = constrained;
            Vector3 delta = target - origin;
            float distance = delta.magnitude;
            if (distance < 0.0001f) return origin;
            int count = Physics.SphereCastNonAlloc(origin, PlayerRadius, delta / distance, hits,
                distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (count == hits.Length) return origin;
            float allowed = distance;
            for (int i = 0; i < count; i++)
                if (hits[i].collider.transform.IsChildOf(transform))
                {
                    float safeDistance = Mathf.Max(0, hits[i].distance - 0.02f);
                    if (safeDistance < allowed)
                    {
                        allowed = safeDistance;
                        contact = new MovementContact { blocked = true, point = hits[i].point, normal = hits[i].normal };
                    }
                }
            return Constrain(origin + delta / distance * allowed);
        }

        public bool IsSpawnClear(Vector3 position)
        {
            if (!Contains(position, 2f)) return false;
            int count = Physics.OverlapSphereNonAlloc(position, 2f, overlaps,
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
