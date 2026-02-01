using Generator.Models;
using Generator.Utils;
using System.Collections.Generic;

namespace Generator.Core
{
    public static class CellGraphBuilder
    {
        public static void Build(
            List<CellModel> models,
            List<VornoiSurface> surfaces,
            float eps)
        {

            for (int i = 0; i < models.Count; i++)
                for (int j = i + 1; j < models.Count; j++)
                {
                    if (PolygonUtils.ShareEdge(
                        surfaces[i].Polygon,
                        surfaces[j].Polygon,
                        eps))
                    {
                        models[i].Neighbors.Add(models[j]);
                        models[j].Neighbors.Add(models[i]);
                    }
                }
        }
    }

}
