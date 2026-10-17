using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Core
{
    /// <summary>Finds duplicate non-zero values in all rows, columns, and 3x3 subgrids.</summary>
    public sealed class SudokuValidator
    {
        public IReadOnlyList<ValidationIssue> Validate(SudokuBoard board)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            return Validate(board.CopyValues());
        }

        public IReadOnlyList<ValidationIssue> Validate(int[,] values)
        {
            EnsureGrid(values);
            var issues = new List<ValidationIssue>();

            for (var row = 0; row < SudokuBoard.Size; row++)
                AddDuplicates(issues, ValidationUnit.Row, row,
                    Enumerable.Range(0, SudokuBoard.Size).Select(column => new CellPosition(row, column)), values);

            for (var column = 0; column < SudokuBoard.Size; column++)
                AddDuplicates(issues, ValidationUnit.Column, column,
                    Enumerable.Range(0, SudokuBoard.Size).Select(row => new CellPosition(row, column)), values);

            for (var boxRow = 0; boxRow < SudokuBoard.BoxSize; boxRow++)
            for (var boxColumn = 0; boxColumn < SudokuBoard.BoxSize; boxColumn++)
            {
                var cells = new List<CellPosition>();
                for (var rowOffset = 0; rowOffset < SudokuBoard.BoxSize; rowOffset++)
                for (var columnOffset = 0; columnOffset < SudokuBoard.BoxSize; columnOffset++)
                    cells.Add(new CellPosition((boxRow * 3) + rowOffset, (boxColumn * 3) + columnOffset));
                AddDuplicates(issues, ValidationUnit.Subgrid, (boxRow * 3) + boxColumn, cells, values);
            }

            return issues;
        }

        public bool IsCompleteAndValid(int[,] values)
        {
            EnsureGrid(values);
            foreach (var value in values)
                if (value < 1 || value > 9) return false;
            return Validate(values).Count == 0;
        }

        private static void AddDuplicates(List<ValidationIssue> issues, ValidationUnit unit, int unitIndex,
            IEnumerable<CellPosition> positions, int[,] values)
        {
            // Grouping each unit once avoids comparing a cell with itself and ignores zero-valued empty cells.
            var duplicates = positions
                .Where(position => values[position.Row, position.Column] != 0)
                .GroupBy(position => values[position.Row, position.Column])
                .Where(group => group.Count() > 1);

            foreach (var duplicate in duplicates)
                issues.Add(new ValidationIssue(unit, unitIndex, duplicate.Key, duplicate));
        }

        private static void EnsureGrid(int[,] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.GetLength(0) != 9 || values.GetLength(1) != 9)
                throw new ArgumentException("A Sudoku grid must be 9x9.", nameof(values));
        }
    }
}
