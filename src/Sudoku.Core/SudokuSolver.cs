using System;

namespace Sudoku.Core
{
    /// <summary>Solves Sudoku grids with deterministic backtracking and can verify solution uniqueness.</summary>
    public sealed class SudokuSolver
    {
        public bool TrySolve(int[,] puzzle, out int[,] solution)
        {
            if (puzzle == null) throw new ArgumentNullException(nameof(puzzle));
            solution = (int[,])puzzle.Clone();
            if (!ContainsOnlyPuzzleValues(solution))
                return false;
            if (new SudokuValidator().Validate(solution).Count != 0)
                return false;
            return Solve(solution);
        }

        public int CountSolutions(int[,] puzzle, int maximum = 2)
        {
            if (puzzle == null) throw new ArgumentNullException(nameof(puzzle));
            if (maximum < 1) throw new ArgumentOutOfRangeException(nameof(maximum));
            var values = (int[,])puzzle.Clone();
            if (!ContainsOnlyPuzzleValues(values))
                return 0;
            if (new SudokuValidator().Validate(values).Count != 0)
                return 0;
            var count = 0;
            Count(values, maximum, ref count);
            return count;
        }

        private static bool ContainsOnlyPuzzleValues(int[,] values)
        {
            if (values.GetLength(0) != 9 || values.GetLength(1) != 9)
                throw new ArgumentException("A Sudoku grid must be 9x9.", nameof(values));
            foreach (var value in values)
                if (value < 0 || value > 9) return false;
            return true;
        }

        private static bool Solve(int[,] values)
        {
            if (!FindBestEmptyCell(values, out var row, out var column)) return true;

            for (var candidate = 1; candidate <= 9; candidate++)
            {
                if (!CanPlace(values, row, column, candidate)) continue;
                values[row, column] = candidate;
                if (Solve(values)) return true;
                values[row, column] = 0;
            }
            return false;
        }

        private static void Count(int[,] values, int maximum, ref int count)
        {
            if (count >= maximum) return;
            if (!FindBestEmptyCell(values, out var row, out var column))
            {
                count++;
                return;
            }

            for (var candidate = 1; candidate <= 9 && count < maximum; candidate++)
            {
                if (!CanPlace(values, row, column, candidate)) continue;
                values[row, column] = candidate;
                Count(values, maximum, ref count);
                values[row, column] = 0;
            }
        }

        private static bool FindBestEmptyCell(int[,] values, out int bestRow, out int bestColumn)
        {
            // Minimum-remaining-values ordering keeps solving and uniqueness checks fast and predictable.
            bestRow = -1;
            bestColumn = -1;
            var bestCandidateCount = 10;
            for (var row = 0; row < 9; row++)
            for (var column = 0; column < 9; column++)
            {
                if (values[row, column] != 0) continue;
                var candidateCount = 0;
                for (var value = 1; value <= 9; value++)
                    if (CanPlace(values, row, column, value)) candidateCount++;
                if (candidateCount >= bestCandidateCount) continue;
                bestCandidateCount = candidateCount;
                bestRow = row;
                bestColumn = column;
                if (candidateCount == 0) return true;
            }
            return bestRow >= 0;
        }

        private static bool CanPlace(int[,] values, int row, int column, int candidate)
        {
            for (var index = 0; index < 9; index++)
                if (values[row, index] == candidate || values[index, column] == candidate) return false;

            var startRow = (row / 3) * 3;
            var startColumn = (column / 3) * 3;
            for (var rowOffset = 0; rowOffset < 3; rowOffset++)
            for (var columnOffset = 0; columnOffset < 3; columnOffset++)
                if (values[startRow + rowOffset, startColumn + columnOffset] == candidate) return false;
            return true;
        }
    }
}
