using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sudoku.Core
{
    /// <summary>Returns the outcome and affected cells without coupling game logic to a UI technology.</summary>
    public sealed class SudokuMoveResult
    {
        public SudokuMoveResult(bool succeeded, string message, IEnumerable<CellPosition> affectedCells = null)
        {
            Succeeded = succeeded;
            Message = message;
            AffectedCells = new ReadOnlyCollection<CellPosition>((affectedCells ?? Enumerable.Empty<CellPosition>()).Distinct().ToList());
        }

        public bool Succeeded { get; }
        public string Message { get; }
        public IReadOnlyList<CellPosition> AffectedCells { get; }
    }
}
