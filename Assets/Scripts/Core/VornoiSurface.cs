using Generator.Utils;
using GK;
using System.Collections.Generic;
using UnityEngine;

namespace Generator.Core
{
    public class VornoiSurface : MonoBehaviour
    {
        [SerializeField] private MeshFilter _filter;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private MeshCollider _collider;
        [SerializeField] private List<Vector2> _polygon;
        [SerializeField] private bool _isStartingCell;

        private float _area = -1.0f;


        public MeshFilter Filter => _filter;
        public MeshRenderer Renderer => _renderer;
        public MeshCollider Collider => _collider;
        public List<Vector2> Polygon => _polygon;

        public float Area
        {
            get
            {
                if (_area < 0.0f)
                {
                    _area = Geom.Area(Polygon);
                }

                return _area;
            }
        }

        public bool IsStartingCell => _isStartingCell;

        public void MarkAsStartingCell()
        {
            _isStartingCell = true;
            gameObject.name = "StartingCell";
        }

        public void Initialize(float thickness)
        {
            var pos = transform.position;

            if (Polygon.Count == 0)
            {

                var scale = 0.5f * transform.localScale;

                Polygon.Add(new Vector2(-scale.x, -scale.y));
                Polygon.Add(new Vector2(scale.x, -scale.y));
                Polygon.Add(new Vector2(scale.x, scale.y));
                Polygon.Add(new Vector2(-scale.x, scale.y));

                thickness = 2.0f * scale.z;

                transform.localScale = Vector3.one;
            }

            var mesh = PolygonUtils.MeshFromPolygon(Polygon, thickness);

            Filter.sharedMesh = mesh;
            Collider.sharedMesh = mesh;
        }
    }
}
