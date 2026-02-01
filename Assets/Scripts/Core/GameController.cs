using Generator.Controllers;
using Generator.Models;
using Generator.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Generator.Core
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private PlayerMarker _playerMarker;
        [SerializeField] private GameView _gameView;
        [SerializeField] private MapGenerator _mapGenerator;
        public static GameController Instance;

        private List<CellController> _cells;
        private int _opened;
        private CellController _currentCell;
        private SeedController _seedController;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            _seedController = new SeedController();
            _gameView.Init(_seedController);
            _gameView.OnGenerateMapClickedEvent += OnGenerateMapClicked;
            _mapGenerator.Initialize();
            OnGenerateMapClicked();
        }

        private void OnDestroy()
        {
            _gameView.OnGenerateMapClickedEvent -= OnGenerateMapClicked;
        }

        public void Init(List<CellController> cells, CellController start)
        {
            _cells = cells;

            foreach (var c in cells)
            {
                c.Model.State = CellState.Locked;
                c.SyncView();
            }        

            start.Model.State = CellState.Opened;
            start.SyncView();
            _currentCell = start;
            _playerMarker.Init(start);
            _opened = 1;
            UpdateAvailable();
        }

        public void OnGenerateMapClicked()
        {
            _mapGenerator.GenerateMap(_seedController.Seed, out var controllers, out var startCell);
            Init(controllers, startCell);
        }

        public void OnCellClicked(CellController cell)
        {
            if (cell.Model.State != CellState.Available)
                return;

            cell.Model.State = CellState.Opened;
            cell.SyncView();
            _opened++;

            _currentCell = cell;
            _playerMarker.MoveToCell(cell);
            UpdateAvailable();
            CheckWin();
        }

        private void UpdateAvailable()
        {
            foreach (var cell in _cells)
            {
                if (cell.Model.State == CellState.Available)
                {
                    cell.Model.State = CellState.Locked;
                    cell.SyncView();
                }
            }

            if (_currentCell == null)
                return;

            foreach (var neighbor in _currentCell.Model.Neighbors)
            {
                if (!neighbor.IsOpened)
                {
                    neighbor.State = CellState.Available;
                    var ctrl = _cells.Find(c => c.Model == neighbor);
                    ctrl?.SyncView();
                }
            }
        }

        private void CheckWin()
        {
            if (_opened < _cells.Count)
                return;
            _gameView.ShowWinScreen();
        }
    }
}
