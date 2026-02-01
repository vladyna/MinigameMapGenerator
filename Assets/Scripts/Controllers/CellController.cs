using Generator.Core;
using Generator.Models;
using Generator.Views;
using UnityEngine;

namespace Generator.Controllers
{
    public class CellController
    {
        private readonly CellModel _model;
        private readonly CellView _view;
        private readonly VornoiSurface _voronoiSurface;

        public CellModel Model => _model;

        public CellController(CellModel model, CellView view, VornoiSurface voronoiSurface)
        {
            _model = model;
            _view = view;

            _view.Clicked += OnClicked;
            SyncView();
            _voronoiSurface = voronoiSurface;
        }

        void OnClicked()
        {
            GameController.Instance.OnCellClicked(this);
        }

        public Vector3 GetCenter()
        {
            var poly = _voronoiSurface.Polygon;
            Vector2 center = Vector2.zero;
            foreach (var v in poly) center += v;
            center /= poly.Count;
            return new Vector3(center.x, center.y, 0f);
        }

        public void SyncView()
        {
            _view.SetColor(_model.State switch
            {
                CellState.Locked => Color.gray,
                CellState.Available => Color.white,
                CellState.Opened => Color.green,
                _ => Color.magenta
            });
        }
    }
}

