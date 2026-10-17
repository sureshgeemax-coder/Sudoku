using System;

namespace Sudoku.Core
{
    /// <summary>Immutable pairing of a puzzle and its verified solution.</summary>
    public sealed class SudokuPuzzle
    {
        private readonly int[,] _clues;
        private readonly int[,] _solution;

        public SudokuPuzzle(int[,] clues, int[,] solution)
        {
            EnsureGrid(clues, nameof(clues));
            EnsureGrid(solution, nameof(solution));
            _clues = (int[,])clues.Clone();
            _solution = (int[,])solution.Clone();

            for (var row = 0; row < SudokuBoard.Size; row++)
            for (var column = 0; column < SudokuBoard.Size; column++)
            {
                var clue = _clues[row, column];
                var solved = _solution[row, column];
                if (clue < 0 || clue > 9)
                    throw new ArgumentException("Puzzle values must be between 0 and 9.", nameof(clues));
                if (solved < 1 || solved > 9 || (clue != 0 && clue != solved))
                    throw new ArgumentException("The solution must contain 1-9 and match every clue.", nameof(solution));
            }

            var validator = new SudokuValidator();
            if (!validator.IsCompleteAndValid(_solution))
                throw new ArgumentException("The supplied solution is not a valid completed Sudoku grid.", nameof(solution));
            if (validator.Validate(_clues).Count != 0)
                throw new ArgumentException("The supplied clues contain a Sudoku rule violation.", nameof(clues));
        }

        public int FixedCellCount
        {
            get
            {
                var count = 0;
                foreach (var value in _clues)
                    if (value != 0) count++;
                return count;
            }
        }

        public int GetClue(int row, int column) => _clues[row, column];
        public int GetSolutionValue(int row, int column) => _solution[row, column];
        public int[,] CopyClues() => (int[,])_clues.Clone();
        public int[,] CopySolution() => (int[,])_solution.Clone();

        private static void EnsureGrid(int[,] grid, string parameterName)
        {
            if (grid == null) throw new ArgumentNullException(parameterName);
            if (grid.GetLength(0) != SudokuBoard.Size || grid.GetLength(1) != SudokuBoard.Size)
                throw new ArgumentException("A Sudoku grid must be 9x9.", parameterName);
        }
    }
}
