using Generator.Controllers;
using System.Collections.Generic;
using UnityEngine;

namespace Generator.Core
{
    public class PlayerMarker : MonoBehaviour
    {
        [SerializeField] private GameObject _flagPrefab;
        [SerializeField] private GameObject _pointPrefab;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private float _moveSpeed = 4f;

        private GameObject _flagInstance;

        private List<GameObject> _points = new List<GameObject>();
        private List<Vector3> _pathCenters = new List<Vector3>();
        private int _targetIndex = -1;
        public void Init(CellController startCell)
        {
            _pathCenters.Clear();
            _points.ForEach(p => Destroy(p));
            _points.Clear();
            _targetIndex = -1;
            var center = startCell.GetCenter();
            _pathCenters.Add(center);

            if (_flagInstance != null) Destroy(_flagInstance);
            _flagInstance = Instantiate(_flagPrefab, center, Quaternion.identity);

            AddPoint(center);

            UpdateLine();
        }

        public void MoveToCell(CellController cell)
        {
            var center = cell.GetCenter();
            _pathCenters.Add(center);
            AddPoint(center);

            _targetIndex = _pathCenters.Count - 1;
            UpdateLine();
        }

        void AddPoint(Vector3 pos)
        {
            var point = Instantiate(_pointPrefab, pos, Quaternion.identity);
            _points.Add(point);
        }

        void UpdateLine()
        {
            if (_lineRenderer == null) return;
            _lineRenderer.positionCount = _pathCenters.Count;
            _lineRenderer.SetPositions(_pathCenters.ToArray());
        }

        void Update()
        {
            if (_flagInstance == null || _targetIndex < 0) return;

            var target = _pathCenters[_targetIndex];
            _flagInstance.transform.position = Vector3.MoveTowards(
                _flagInstance.transform.position,
                target,
                _moveSpeed * Time.deltaTime
            );

            if ((_flagInstance.transform.position - target).sqrMagnitude < 1e-4f)
                _targetIndex = -1;
        }
    }

}
