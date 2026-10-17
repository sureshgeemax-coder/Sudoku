using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sudoku.Core
{
    public enum ValidationUnit
    {
        Row,
        Column,
        Subgrid
    }

    /// <summary>Describes one duplicated value and every cell participating in that conflict.</summary>
    public sealed class ValidationIssue
    {
        public ValidationIssue(ValidationUnit unit, int unitIndex, int number, IEnumerable<CellPosition> cells)
        {
            Unit = unit;
            UnitIndex = unitIndex;
            Number = number;
            Cells = new ReadOnlyCollection<CellPosition>(cells.ToList());
        }

        public ValidationUnit Unit { get; }
        public int UnitIndex { get; }
        public int Number { get; }
        public IReadOnlyList<CellPosition> Cells { get; }

        public string Message
        {
            get
            {
                switch (Unit)
                {
                    case ValidationUnit.Row:
                        return $"Number {Number} already exists in Row {(char)('A' + UnitIndex)}.";
                    case ValidationUnit.Column:
                        return $"Number {Number} already exists in Column {UnitIndex + 1}.";
                    default:
                        return $"Number {Number} already exists in the same 3x3 subgrid.";
                }
            }
        }
    }
}
