using System.Collections.Generic;
namespace Generator.Models
{
    public class CellModel
    {
        public CellState State;
        public readonly List<CellModel> Neighbors = new();

        public bool IsOpened => State == CellState.Opened;
    }
}