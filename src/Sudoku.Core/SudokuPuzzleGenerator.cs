using System;
using System.Linq;

namespace Sudoku.Core
{
    /// <summary>Provides the required puzzle and produces equivalent unique 30-clue puzzles.</summary>
    public sealed class SudokuPuzzleGenerator
    {
        private static readonly int[,] InitialClues =
        {
            { 5, 3, 0, 0, 7, 0, 0, 0, 0 },
            { 6, 0, 0, 1, 9, 5, 0, 0, 0 },
            { 0, 9, 8, 0, 0, 0, 0, 6, 0 },
            { 8, 0, 0, 0, 6, 0, 0, 0, 3 },
            { 4, 0, 0, 8, 0, 3, 0, 0, 1 },
            { 7, 0, 0, 0, 2, 0, 0, 0, 6 },
            { 0, 6, 0, 0, 0, 0, 2, 8, 0 },
            { 0, 0, 0, 4, 1, 9, 0, 0, 5 },
            { 0, 0, 0, 0, 8, 0, 0, 7, 9 }
        };

        private static readonly int[,] InitialSolution =
        {
            { 5, 3, 4, 6, 7, 8, 9, 1, 2 },
            { 6, 7, 2, 1, 9, 5, 3, 4, 8 },
            { 1, 9, 8, 3, 4, 2, 5, 6, 7 },
            { 8, 5, 9, 7, 6, 1, 4, 2, 3 },
            { 4, 2, 6, 8, 5, 3, 7, 9, 1 },
            { 7, 1, 3, 9, 2, 4, 8, 5, 6 },
            { 9, 6, 1, 5, 3, 7, 2, 8, 4 },
            { 2, 8, 7, 4, 1, 9, 6, 3, 5 },
            { 3, 4, 5, 2, 8, 6, 1, 7, 9 }
        };

        public SudokuPuzzle CreateInitialPuzzle() => new SudokuPuzzle(InitialClues, InitialSolution);

        public SudokuPuzzle CreateNewPuzzle(Random random = null)
        {
            random = random ?? new Random();
            var rowOrder = CreateUnitOrder(random);
            var columnOrder = CreateUnitOrder(random);
            var digits = Enumerable.Range(1, 9).ToArray();
            Shuffle(digits, random);

            var clues = new int[9, 9];
            var solution = new int[9, 9];
            for (var row = 0; row < 9; row++)
            for (var column = 0; column < 9; column++)
            {
                var sourceClue = InitialClues[rowOrder[row], columnOrder[column]];
                clues[row, column] = sourceClue == 0 ? 0 : digits[sourceClue - 1];
                solution[row, column] = digits[InitialSolution[rowOrder[row], columnOrder[column]] - 1];
            }

            // These transformations preserve all Sudoku constraints, clue count, and uniqueness.
            return new SudokuPuzzle(clues, solution);
        }

        private static int[] CreateUnitOrder(Random random)
        {
            var groups = new[] { 0, 1, 2 };
            Shuffle(groups, random);
            var result = new int[9];
            var target = 0;
            foreach (var group in groups)
            {
                var members = new[] { 0, 1, 2 };
                Shuffle(members, random);
                foreach (var member in members)
                    result[target++] = (group * 3) + member;
            }
            return result;
        }

        private static void Shuffle(int[] values, Random random)
        {
            for (var index = values.Length - 1; index > 0; index--)
            {
                var other = random.Next(index + 1);
                var temporary = values[index];
                values[index] = values[other];
                values[other] = temporary;
            }
        }
    }
}
