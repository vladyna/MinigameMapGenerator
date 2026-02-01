using Generator.Controllers;
using Generator.Models;
using Generator.Utils;
using Generator.Views;
using GK;
using System.Collections.Generic;
using UnityEngine;

namespace Generator.Core
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private VornoiSurface _startingSurface;
        [SerializeField] private VornoiSurface _voronoiSurfacePrefab;

        [SerializeField] private float _thickness = 1.0f;
        [SerializeField] private float _gap = 0.05f;
        [SerializeField] private Color _polygonColor = Color.white;
        [SerializeField] private Color _startingCellColor = Color.green;

        [SerializeField] private float _minSiteDistance = 0.45f;
        [SerializeField] private int _poissonAttempts = 30;
        [SerializeField] private int _maxSamples = 256;

        [SerializeField] private int _maxPieces = 20;

        [SerializeField] private int _minPolygonCorners = 8;

        [SerializeField] private int _extraCenterNeighbors = 6;
        [SerializeField] private float _centerNeighborRadius = 0.8f;
        [SerializeField] private float _centerNeighborJitter = 0.12f;

        private List<VornoiSurface> _generatedPieces = new List<VornoiSurface>();

        public void Initialize()
        {
            _startingSurface.Initialize(_thickness);
        }

        public void GenerateMap(int seed, out List<CellController> controllers, out CellController startCell)
        {
            controllers = null;
            startCell = null;

            ClearGeneratedPieces();
            UnityEngine.Random.InitState(seed);

            if (_startingSurface != null)
                _startingSurface.gameObject.SetActive(true);

            GenerateVoronoi(transform.position, out controllers, out startCell, _gap);
        }

        public void GenerateVoronoi(Vector2 position, out List<CellController> controllers, out CellController startCell, float gap = 0.0f)
        {
            controllers = null;
            startCell = null;

            var (boundsMin, boundsMax) = CalculatePolygonBounds(_startingSurface.Polygon);

            var samples = VornoiUtils.PoissonDiskSamples(position, _startingSurface.Polygon, boundsMin, boundsMax, _minSiteDistance, _poissonAttempts, _maxSamples);

            var sitesList = BuildInitialSites(position, samples);
            EnsureCenterNeighbors(ref sitesList, position);

            var protectedIndices = GetProtectedIndices(sitesList, position);

            var reducedSites = VornoiUtils.ReduceSites(sitesList, _maxPieces, protectedIndices);
            var sites = reducedSites.ToArray();

            var calc = new VoronoiCalculator();
            var clip = new VoronoiClipper();
            var diagram = calc.CalculateDiagram(sites);

            var piecesBySite = CreatePiecesFromSites(diagram, sites, gap);

            var centralPiece = FindCentralPiece(piecesBySite, sites);
            if (centralPiece == null && piecesBySite.ContainsKey(0))
                centralPiece = piecesBySite[0];

            if (centralPiece != null)
            {
                centralPiece.MarkAsStartingCell();
                if (centralPiece.Renderer != null)
                    centralPiece.Renderer.material.color = _startingCellColor;
            }

            _startingSurface.gameObject.SetActive(false);
            CreateCellConnections(centralPiece, out controllers, out startCell);
        }

        private (Vector2, Vector2) CalculatePolygonBounds(List<Vector2> polygon)
        {
            var boundsMin = new Vector2(float.MaxValue, float.MaxValue);
            var boundsMax = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < polygon.Count; i++)
            {
                var v = polygon[i];
                if (v.x < boundsMin.x) boundsMin.x = v.x;
                if (v.y < boundsMin.y) boundsMin.y = v.y;
                if (v.x > boundsMax.x) boundsMax.x = v.x;
                if (v.y > boundsMax.y) boundsMax.y = v.y;
            }

            return (boundsMin, boundsMax);
        }

        private List<Vector2> BuildInitialSites(Vector2 center, List<Vector2> samples)
        {
            var sites = new List<Vector2> { center };

            for (int i = 0; i < samples.Count; i++)
            {
                if ((samples[i] - center).sqrMagnitude < 1e-6f)
                    continue;
                sites.Add(samples[i]);
            }

            return sites;
        }

        private void EnsureCenterNeighbors(ref List<Vector2> sitesList, Vector2 position)
        {
            var neighbors = 0;
            for (int i = 1; i < sitesList.Count; i++)
            {
                if ((sitesList[i] - position).magnitude <= _centerNeighborRadius)
                    neighbors++;
            }

            if (neighbors >= _extraCenterNeighbors)
                return;

            var need = _extraCenterNeighbors - neighbors;
            for (int n = 0; n < need; n++)
            {
                var angle = 2.0f * Mathf.PI * (n / (float)need) + Random.Range(-_centerNeighborJitter, _centerNeighborJitter);
                var dist = _centerNeighborRadius * (0.8f + Random.Range(-0.12f, 0.12f));
                var p = position + new Vector2(dist * Mathf.Cos(angle), dist * Mathf.Sin(angle));

                if (!PolygonUtils.IsPointInPolygon(p, _startingSurface.Polygon))
                    continue;

                var tooClose = false;
                for (int j = 0; j < sitesList.Count; j++)
                {
                    if ((sitesList[j] - p).sqrMagnitude < (_minSiteDistance * _minSiteDistance * 0.36f))
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                    sitesList.Add(p);
            }
        }

        private List<int> GetProtectedIndices(List<Vector2> sitesList, Vector2 position)
        {
            var protectedIndices = new List<int> { 0 };
            for (int i = 1; i < sitesList.Count; i++)
            {
                if ((sitesList[i] - position).magnitude <= _centerNeighborRadius)
                    protectedIndices.Add(i);
            }
            return protectedIndices;
        }

        private Dictionary<int, VornoiSurface> CreatePiecesFromSites(VoronoiDiagram diagram, Vector2[] sites, float gap)
        {
            var clip = new VoronoiClipper();
            var piecesBySite = new Dictionary<int, VornoiSurface>();
            var clipped = new List<Vector2>();

            for (int i = 0; i < sites.Length; i++)
            {
                clipped.Clear();
                clip.ClipSite(diagram, _startingSurface.Polygon, i, ref clipped);

                if (clipped.Count == 0)
                    continue;

                var inset = PolygonUtils.ShrinkPolygonTowardsCenter(clipped, gap);

                if (inset.Count > 0 && inset.Count < _minPolygonCorners)
                    inset = PolygonUtils.EnsureMinVertices(inset, _minPolygonCorners);

                if (inset.Count <= 2)
                    continue;

                var newGo = Instantiate(_voronoiSurfacePrefab, _startingSurface.transform.parent);
                newGo.transform.localPosition = _startingSurface.transform.localPosition;
                newGo.transform.localRotation = _startingSurface.transform.localRotation;

                newGo.Polygon.Clear();
                newGo.Polygon.AddRange(inset);
                newGo.Initialize(_thickness);

                _generatedPieces.Add(newGo);
                piecesBySite[i] = newGo;

                if (newGo.Renderer != null)
                    newGo.Renderer.material.color = _polygonColor;
            }

            return piecesBySite;
        }

        private VornoiSurface FindCentralPiece(Dictionary<int, VornoiSurface> piecesBySite, Vector2[] sites)
        {
            foreach (var kv in piecesBySite)
            {
                if (PolygonUtils.IsPointInPolygon(sites[0], kv.Value.Polygon))
                    return kv.Value;
            }
            return null;
        }

        private void CreateCellConnections(VornoiSurface centralPiece, out List<CellController> controllers, out CellController startCell)
        {
            var models = new List<CellModel>();
            controllers = new List<CellController>();
            var surfaces = new List<VornoiSurface>();

            foreach (var piece in _generatedPieces)
            {
                var model = new CellModel();
                models.Add(model);

                var view = piece.gameObject.GetComponent<CellView>();
                var controller = new CellController(model, view, piece);
                controllers.Add(controller);

                surfaces.Add(piece);
            }

            CellGraphBuilder.Build(models, surfaces, _gap * 2);

            var startIndex = surfaces.IndexOf(centralPiece);
            startCell = startIndex >= 0 ? controllers[startIndex] : (controllers.Count > 0 ? controllers[0] : null);
        }

        void ClearGeneratedPieces()
        {
            if (_generatedPieces == null || _generatedPieces.Count == 0)
                return;

            for (int i = 0; i < _generatedPieces.Count; i++)
            {
                var piece = _generatedPieces[i];
                if (piece == null)
                    continue;

                if (Application.isPlaying)
                {
                    piece.gameObject.SetActive(false);
                    Destroy(piece.gameObject);
                }
                else
                {
                    DestroyImmediate(piece.gameObject);
                }
            }

            _generatedPieces.Clear();
        }
    }
}