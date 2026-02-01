using System.Collections.Generic;
using UnityEngine;
namespace Generator.Utils
{
    public static class PolygonUtils
    {

        public static bool ShareEdge(
            List<Vector2> a,
            List<Vector2> b,
            float eps)
        {
            int hits = 0;

            foreach (var va in a)
                foreach (var vb in b)
                    if ((va - vb).sqrMagnitude < eps * eps)
                        hits++;

            return hits >= 2;
        }

        public static Mesh MeshFromPolygon(List<Vector2> polygon, float thickness)
        {
            var count = polygon.Count;
            var verts = new Vector3[6 * count];
            var norms = new Vector3[6 * count];
            var tris = new int[3 * (4 * count - 4)];


            var vi = 0;
            var ni = 0;
            var ti = 0;

            var ext = 0.5f * thickness;

            for (int i = 0; i < count; i++)
            {
                verts[vi++] = new Vector3(polygon[i].x, polygon[i].y, ext);
                norms[ni++] = Vector3.forward;
            }

            for (int i = 0; i < count; i++)
            {
                verts[vi++] = new Vector3(polygon[i].x, polygon[i].y, -ext);
                norms[ni++] = Vector3.back;
            }

            for (int i = 0; i < count; i++)
            {
                var iNext = i == count - 1 ? 0 : i + 1;

                verts[vi++] = new Vector3(polygon[i].x, polygon[i].y, ext);
                verts[vi++] = new Vector3(polygon[i].x, polygon[i].y, -ext);
                verts[vi++] = new Vector3(polygon[iNext].x, polygon[iNext].y, -ext);
                verts[vi++] = new Vector3(polygon[iNext].x, polygon[iNext].y, ext);

                var norm = Vector3.Cross(polygon[iNext] - polygon[i], Vector3.forward).normalized;

                norms[ni++] = norm;
                norms[ni++] = norm;
                norms[ni++] = norm;
                norms[ni++] = norm;
            }


            for (int vert = 2; vert < count; vert++)
            {
                tris[ti++] = 0;
                tris[ti++] = vert - 1;
                tris[ti++] = vert;
            }

            for (int vert = 2; vert < count; vert++)
            {
                tris[ti++] = count;
                tris[ti++] = count + vert;
                tris[ti++] = count + vert - 1;
            }

            for (int vert = 0; vert < count; vert++)
            {
                var si = 2 * count + 4 * vert;

                tris[ti++] = si;
                tris[ti++] = si + 1;
                tris[ti++] = si + 2;

                tris[ti++] = si;
                tris[ti++] = si + 2;
                tris[ti++] = si + 3;
            }


            var mesh = new Mesh();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.normals = norms;

            return mesh;
        }

        public static List<Vector2> ShrinkPolygonTowardsCenter(List<Vector2> polygon, float gap)
        {
            var result = new List<Vector2>(polygon.Count);
            if (polygon.Count == 0)
                return result;

            var center = Vector2.zero;
            for (int i = 0; i < polygon.Count; i++)
                center += polygon[i];
            center /= polygon.Count;

            for (int i = 0; i < polygon.Count; i++)
            {
                var v = polygon[i];
                var dir = center - v;
                var dist = dir.magnitude;

                if (dist <= Mathf.Epsilon || gap <= 0.0f)
                {
                    result.Add(v);
                }
                else
                {
                    var move = Mathf.Min(gap, dist * 0.999f);
                    result.Add(v + dir.normalized * move);
                }
            }

            return result;
        }

        public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            if (polygon == null || polygon.Count < 3)
                return false;

            var inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var xi = polygon[i].x;
                var yi = polygon[i].y;
                var xj = polygon[j].x;
                var yj = polygon[j].y;

                var intersect = ((yi > point.y) != (yj > point.y)) &&
                                (point.x < (xj - xi) * (point.y - yi) / (yj - yi + Mathf.Epsilon) + xi);
                if (intersect)
                    inside = !inside;
            }

            return inside;
        }

        public static List<Vector2> EnsureMinVertices(List<Vector2> polygon, int minVerts)
        {
            var result = new List<Vector2>(polygon);
            if (result.Count == 0)
                return result;

            int attempts = 0;
            while (result.Count < minVerts && attempts < 32)
            {
                var bestIndex = 0;
                var bestLen = 0.0f;
                for (int i = 0; i < result.Count; i++)
                {
                    var a = result[i];
                    var b = result[(i + 1) % result.Count];
                    var len = (b - a).sqrMagnitude;
                    if (len > bestLen)
                    {
                        bestLen = len;
                        bestIndex = i;
                    }
                }

                var vA = result[bestIndex];
                var vB = result[(bestIndex + 1) % result.Count];
                var mid = 0.5f * (vA + vB);
                mid += new Vector2(Random.Range(-1e-3f, 1e-3f), Random.Range(-1e-3f, 1e-3f));
                result.Insert(bestIndex + 1, mid);

                attempts++;
            }

            return result;
        }
    }
}
