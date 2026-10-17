using System;

namespace Sudoku.Core
{
    /// <summary>Owns mutable play state while retaining the immutable puzzle definition.</summary>
    public sealed class SudokuBoard
    {
        public const int Size = 9;
        public const int BoxSize = 3;

        private readonly SudokuPuzzle _puzzle;
        private readonly int[,] _values;

        public SudokuBoard(SudokuPuzzle puzzle)
        {
            _puzzle = puzzle ?? throw new ArgumentNullException(nameof(puzzle));
            _values = puzzle.CopyClues();
        }

        public SudokuPuzzle Puzzle => _puzzle;
        public int GetValue(int row, int column) => _values[row, column];
        public bool IsFixed(int row, int column) => _puzzle.GetClue(row, column) != 0;
        public int[,] CopyValues() => (int[,])_values.Clone();

        internal void SetEditableValue(int row, int column, int value)
        {
            // Mutations are deliberately internal so fixed-cell and range rules cannot be bypassed.
            _values[row, column] = value;
        }
    }
}
