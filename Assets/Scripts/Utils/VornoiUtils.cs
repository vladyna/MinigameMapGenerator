using System.Collections.Generic;
using UnityEngine;
namespace Generator.Utils
{
    public static class VornoiUtils
    {
        public static List<Vector2> PoissonDiskSamples(Vector2 center, List<Vector2> polygon, Vector2 boundsMin, Vector2 boundsMax, float minDist, int k, int maxSamples)
        {
            var samples = new List<Vector2>();
            var active = new List<Vector2>();

            var width = boundsMax.x - boundsMin.x;
            var height = boundsMax.y - boundsMin.y;

            var cellSize = minDist / Mathf.Sqrt(2.0f);
            var gridCols = Mathf.Max(1, Mathf.CeilToInt(width / cellSize));
            var gridRows = Mathf.Max(1, Mathf.CeilToInt(height / cellSize));
            var grid = new int[gridCols, gridRows];
            for (int gx = 0; gx < gridCols; gx++)
                for (int gy = 0; gy < gridRows; gy++)
                    grid[gx, gy] = -1;

            if (PolygonUtils.IsPointInPolygon(center, polygon))
            {
                samples.Add(center);
                active.Add(center);
                var gcx = Mathf.Clamp((int)((center.x - boundsMin.x) / cellSize), 0, gridCols - 1);
                var gcy = Mathf.Clamp((int)((center.y - boundsMin.y) / cellSize), 0, gridRows - 1);
                grid[gcx, gcy] = 0;
            }

            if (samples.Count == 0)
            {
                var attempts = 0;
                while (attempts < k && samples.Count == 0)
                {
                    var rx = Random.Range(boundsMin.x, boundsMax.x);
                    var ry = Random.Range(boundsMin.y, boundsMax.y);
                    var p = new Vector2(rx, ry);
                    if (PolygonUtils.IsPointInPolygon(p, polygon))
                    {
                        samples.Add(p);
                        active.Add(p);
                        var gcx = Mathf.Clamp((int)((p.x - boundsMin.x) / cellSize), 0, gridCols - 1);
                        var gcy = Mathf.Clamp((int)((p.y - boundsMin.y) / cellSize), 0, gridRows - 1);
                        grid[gcx, gcy] = 0;
                    }
                    attempts++;
                }
            }

            while (active.Count > 0 && samples.Count < maxSamples)
            {
                var idx = Random.Range(0, active.Count);
                var point = active[idx];
                var found = false;

                for (int i = 0; i < k; i++)
                {
                    var radius = Random.Range(minDist, 2.0f * minDist);
                    var angle = Random.Range(0.0f, Mathf.PI * 2.0f);
                    var cand = point + new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));

                    if (cand.x < boundsMin.x || cand.x > boundsMax.x || cand.y < boundsMin.y || cand.y > boundsMax.y)
                        continue;
                    if (!PolygonUtils.IsPointInPolygon(cand, polygon))
                        continue;


                    var gcx = Mathf.Clamp((int)((cand.x - boundsMin.x) / cellSize), 0, gridCols - 1);
                    var gcy = Mathf.Clamp((int)((cand.y - boundsMin.y) / cellSize), 0, gridRows - 1);
                    var ok = true;
                    var searchRadius = 2; 
                    for (int gx = Mathf.Max(0, gcx - searchRadius); gx <= Mathf.Min(gridCols - 1, gcx + searchRadius) && ok; gx++)
                    {
                        for (int gy = Mathf.Max(0, gcy - searchRadius); gy <= Mathf.Min(gridRows - 1, gcy + searchRadius); gy++)
                        {
                            var sidx = grid[gx, gy];
                            if (sidx != -1)
                            {
                                var s = samples[sidx];
                                if ((s - cand).sqrMagnitude < minDist * minDist)
                                {
                                    ok = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (ok)
                    {
                        samples.Add(cand);
                        active.Add(cand);
                        grid[gcx, gcy] = samples.Count - 1;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    active.RemoveAt(idx);
            }

            return samples;
        }

        public static List<Vector2> ReduceSites(List<Vector2> sites, int maxSites, List<int> protectedIndices)
        {
            var count = sites.Count;
            if (count <= maxSites)
                return new List<Vector2>(sites);

            var keep = new bool[count];

            for (int i = 0; i < protectedIndices.Count; i++)
            {
                var pi = protectedIndices[i];
                if (pi >= 0 && pi < count)
                    keep[pi] = true;
            }

            keep[0] = true;

            var selected = new List<int>();
            for (int i = 0; i < count; i++)
            {
                if (keep[i])
                    selected.Add(i);
            }


            while (selected.Count < maxSites)
            {
                var bestIdx = -1;
                var bestDist = -1.0f;
                for (int i = 0; i < count; i++)
                {
                    if (keep[i])
                        continue;

                    var minDistSq = float.MaxValue;
                    for (int s = 0; s < selected.Count; s++)
                    {
                        var d = (sites[i] - sites[selected[s]]).sqrMagnitude;
                        if (d < minDistSq) minDistSq = d;
                    }

                    if (minDistSq > bestDist)
                    {
                        bestDist = minDistSq;
                        bestIdx = i;
                    }
                }

                if (bestIdx == -1)
                    break;

                keep[bestIdx] = true;
                selected.Add(bestIdx);
            }

            var result = new List<Vector2>();
            result.Add(sites[0]);

            for (int i = 1; i < count; i++)
            {
                if (keep[i])
                    result.Add(sites[i]);
                if (result.Count >= maxSites)
                    break;
            }

            return result;
        }
    }
}
