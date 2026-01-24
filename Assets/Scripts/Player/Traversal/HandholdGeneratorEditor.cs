using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Soulslike.Player.Traversal
{
    public class HandholdGeneratorEditor : EditorWindow
    {
        private const float UP_THRESHOLD = 30f;        // Max angle from world up for top triangle
        private const float MIN_DIFF = 45f;            // Min angle between up/forward triangles for normal handhold
        private const float MAX_DIFF = 135f;           // Max angle between up/forward triangles for normal handhold
        private const float MIN_HH_DIST = 0.4f;        // Minimum distance between handholds
        private const float MIN_DIFF_VERTICAL = 60f;   // Forward triangle must be at least this much from up for top corners

        // Filtering constants
        private const float INSIDE_CHECK_RADIUS = 0.08f;
        private const float MIN_CLEARANCE_BELOW = 1.2f;
        private const float CLEARANCE_RAY_OFFSET = 0.05f;

        private static GameObject activeGameObject;

        [MenuItem("Tools/Handholds/Generate From Scene")]
        public static void GenerateHandholdsFromScene()
        {
            activeGameObject = new GameObject("Generated_Handholds");

            MeshFilter[] meshFilters = GameObject.FindObjectsOfType<MeshFilter>();

            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null) continue;

                // Get the mesh collider (if any) to ignore self in overlap/ray checks
                Collider mfCollider = mf.GetComponent<Collider>();
                Collider[] ignoreColliders = mfCollider != null ? new[] { mfCollider } : new Collider[0];

                List<TrianglePair> trianglePairs = BuildTrianglePairs(mf);
                CrawlMeshForHandholds(trianglePairs, ignoreColliders);
            }

            Debug.Log($"Generated handholds from {meshFilters.Length} meshes.");
        }

        // ============================
        // Core Logic
        // ============================

        private static void CrawlMeshForHandholds(List<TrianglePair> trianglePairs, Collider[] ignoreColliders)
        {
            for (int i = 0; i < trianglePairs.Count; i++)
            {
                TrianglePair tp = trianglePairs[i];

                Vector3 n1 = GetNormal(tp.t1);
                Vector3 n2 = GetNormal(tp.t2);

                Triangle upT = tp.t1;
                Triangle forT = tp.t2;

                Vector3 upNormal;
                Vector3 forwardNormal;

                float upScore1 = Vector3.Dot(Vector3.up, n1);
                float upScore2 = Vector3.Dot(Vector3.up, n2);

                if (upScore1 > upScore2)
                {
                    upNormal = n1;
                    forwardNormal = n2;
                }
                else
                {
                    upNormal = n2;
                    forwardNormal = n1;
                    upT = tp.t2;
                    forT = tp.t1;
                }

                float angleBetween = Vector3.Angle(upNormal, forwardNormal);

                float upY = ((upT.v1 + upT.v2 + upT.v3) / 3f).y;
                float forY = ((forT.v1 + forT.v2 + forT.v3) / 3f).y;

                // ----------------------------
                // Top corner detection
                // ----------------------------
                bool isTopCorner = Vector3.Angle(upNormal, Vector3.up) < UP_THRESHOLD
                                   && Vector3.Angle(forwardNormal, Vector3.up) > MIN_DIFF_VERTICAL
                                   && forY < upY; // forward triangle lower than up triangle

                // Normal handhold conditions
                bool normalValid = Vector3.Angle(Vector3.up, upNormal) <= UP_THRESHOLD
                                   && angleBetween >= MIN_DIFF
                                   && angleBetween <= MAX_DIFF
                                   && forY <= upY;

                // Spawn if either normal check passes OR it's a top corner
                if (normalValid || isTopCorner)
                {
                    AddHandholds(tp.e, upNormal, forwardNormal, ignoreColliders);
                }
            }
        }

        private static void AddHandholds(SharedEdge e, Vector3 up, Vector3 fn, Collider[] ignoreColliders)
        {
            Vector3 diff = e.v2 - e.v1;

            if (diff.magnitude < MIN_HH_DIST)
            {
                TryAddHold(e.v1, up, fn, activeGameObject.transform, ignoreColliders);
                TryAddHold(e.v2, up, fn, activeGameObject.transform, ignoreColliders);
                return;
            }
            else
            {
                Vector3 dir = diff.normalized;
                Transform prev = activeGameObject.transform;

                for (float i = 0; i < diff.magnitude; i += MIN_HH_DIST)
                {
                    Vector3 pos = e.v1 + (dir * i);
                    GameObject t = TryAddHold(pos, up, fn, prev, ignoreColliders);
                    if (t != null)
                        prev = t.transform;
                }
            }
        }

        // ============================
        // Filtering Logic
        // ============================

        private static GameObject TryAddHold(Vector3 pos, Vector3 up, Vector3 forward, Transform parent, Collider[] ignoreColliders)
        {
            // 1) Reject if inside another collider (ignore self)
            Collider[] overlaps = Physics.OverlapSphere(pos, INSIDE_CHECK_RADIUS);
            foreach (var c in overlaps)
            {
                if (!c.isTrigger && !System.Array.Exists(ignoreColliders, x => x == c))
                    return null;
            }

            // 2) Reject if not enough space below
            Vector3 rayStart = pos + Vector3.up * CLEARANCE_RAY_OFFSET;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, MIN_CLEARANCE_BELOW))
            {
                if (!System.Array.Exists(ignoreColliders, x => hit.collider == x))
                    return null;
            }

            // Spawn handhold
            return AddHold(pos, up, forward, parent);
        }

        private static GameObject AddHold(Vector3 pos, Vector3 up, Vector3 forward, Transform parent)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.name = "Handhold";
            g.transform.position = pos;
            g.transform.up = up;
            g.transform.forward = forward;
            g.transform.localScale = Vector3.one * 0.15f;
            g.transform.SetParent(parent);

            Object.DestroyImmediate(g.GetComponent<Collider>());
            return g;
        }

        private static Vector3 GetNormal(Triangle t)
        {
            return Vector3.Cross(t.v2 - t.v1, t.v3 - t.v1).normalized;
        }

        // ============================
        // Triangle Pair Builder
        // ============================

        private static List<TrianglePair> BuildTrianglePairs(MeshFilter mf)
        {
            Mesh mesh = mf.sharedMesh;
            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;

            Dictionary<EdgeKey, Triangle> edgeMap = new Dictionary<EdgeKey, Triangle>();
            List<TrianglePair> pairs = new List<TrianglePair>();

            for (int i = 0; i < tris.Length; i += 3)
            {
                Vector3 v1 = mf.transform.TransformPoint(verts[tris[i]]);
                Vector3 v2 = mf.transform.TransformPoint(verts[tris[i + 1]]);
                Vector3 v3 = mf.transform.TransformPoint(verts[tris[i + 2]]);

                Triangle t = new Triangle(v1, v2, v3);

                CheckEdge(v1, v2, t, edgeMap, pairs);
                CheckEdge(v2, v3, t, edgeMap, pairs);
                CheckEdge(v3, v1, t, edgeMap, pairs);
            }

            return pairs;
        }

        private static void CheckEdge(
            Vector3 a,
            Vector3 b,
            Triangle t,
            Dictionary<EdgeKey, Triangle> edgeMap,
            List<TrianglePair> pairs)
        {
            EdgeKey key = new EdgeKey(a, b);

            if (edgeMap.TryGetValue(key, out Triangle other))
            {
                SharedEdge e = new SharedEdge(a, b);
                pairs.Add(new TrianglePair(other, t, e));
            }
            else
            {
                edgeMap[key] = t;
            }
        }

        // ============================
        // Data Types
        // ============================

        private struct Triangle
        {
            public Vector3 v1, v2, v3;
            public Triangle(Vector3 a, Vector3 b, Vector3 c) { v1 = a; v2 = b; v3 = c; }
        }

        private struct SharedEdge
        {
            public Vector3 v1, v2;
            public SharedEdge(Vector3 a, Vector3 b) { v1 = a; v2 = b; }
        }

        private struct TrianglePair
        {
            public Triangle t1, t2;
            public SharedEdge e;
            public TrianglePair(Triangle a, Triangle b, SharedEdge edge) { t1 = a; t2 = b; e = edge; }
        }

        private struct EdgeKey
        {
            private readonly Vector3 a, b;
            public EdgeKey(Vector3 v1, Vector3 v2)
            {
                if (v1.sqrMagnitude < v2.sqrMagnitude) { a = v1; b = v2; } else { a = v2; b = v1; }
            }

            public override int GetHashCode() { return a.GetHashCode() ^ b.GetHashCode(); }
            public override bool Equals(object obj)
            {
                if (!(obj is EdgeKey other)) return false;
                return a == other.a && b == other.b;
            }
        }
    }
}
