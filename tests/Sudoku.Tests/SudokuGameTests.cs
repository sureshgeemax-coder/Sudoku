using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sudoku.Core;

namespace Sudoku.Tests
{
    [TestClass]
    public sealed class SudokuGameTests
    {
        private readonly SudokuPuzzleGenerator _generator = new SudokuPuzzleGenerator();

        [TestMethod]
        public void InitialPuzzle_HasExactlyThirtyFixedCells()
        {
            var puzzle = _generator.CreateInitialPuzzle();

            Assert.AreEqual(30, puzzle.FixedCellCount);
        }

        [TestMethod]
        public void InitialPuzzle_IsValidAndHasTheExpectedUniqueSolution()
        {
            var puzzle = _generator.CreateInitialPuzzle();
            var solver = new SudokuSolver();

            Assert.AreEqual(0, new SudokuValidator().Validate(puzzle.CopyClues()).Count);
            Assert.IsTrue(solver.TrySolve(puzzle.CopyClues(), out var solved));
            Assert.AreEqual(1, solver.CountSolutions(puzzle.CopyClues()));
            AssertGridsEqual(puzzle.CopySolution(), solved);
        }

        [TestMethod]
        public void SetValue_AcceptsAValidMove()
        {
            var game = CreateGame();

            var result = game.SetValue(0, 2, 4);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(4, game.Board.GetValue(0, 2));
            StringAssert.Contains(result.Message, "Move accepted");
        }

        [TestMethod]
        public void SetValue_CannotChangeAPreFilledCell()
        {
            var game = CreateGame();

            var result = game.SetValue(0, 0, 6);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(5, game.Board.GetValue(0, 0));
            StringAssert.Contains(result.Message, "A1");
            StringAssert.Contains(result.Message, "pre-filled");
        }

        [TestMethod]
        public void ClearValue_CannotClearAPreFilledCell()
        {
            var game = CreateGame();

            var result = game.ClearValue(0, 0);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(5, game.Board.GetValue(0, 0));
            StringAssert.Contains(result.Message, "cannot be cleared");
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(10)]
        public void SetValue_RejectsValuesOutsideOneThroughNine(int value)
        {
            var game = CreateGame();

            var result = game.SetValue(0, 2, value);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(0, game.Board.GetValue(0, 2));
            StringAssert.Contains(result.Message, "1 to 9");
        }

        [TestMethod]
        public void Check_DetectsDuplicateInRow()
        {
            var game = CreateGame();
            game.SetValue(0, 2, 3);

            var issue = game.Check().Single(item => item.Unit == ValidationUnit.Row && item.Number == 3);

            Assert.AreEqual(0, issue.UnitIndex);
            StringAssert.Contains(issue.Message, "Row A");
        }

        [TestMethod]
        public void Check_DetectsDuplicateInColumn()
        {
            var game = CreateGame();
            game.SetValue(2, 0, 5);

            var issue = game.Check().Single(item => item.Unit == ValidationUnit.Column && item.Number == 5);

            Assert.AreEqual(0, issue.UnitIndex);
            StringAssert.Contains(issue.Message, "Column 1");
        }

        [TestMethod]
        public void Check_DetectsDuplicateInSubgrid()
        {
            var game = CreateGame();
            game.SetValue(1, 2, 8);

            var issue = game.Check().Single(item => item.Unit == ValidationUnit.Subgrid && item.Number == 8);

            CollectionAssert.Contains(issue.Cells.ToList(), new CellPosition(1, 2));
            StringAssert.Contains(issue.Message, "3x3 subgrid");
        }

        [TestMethod]
        public void Check_IgnoresEmptyCells()
        {
            var game = CreateGame();

            Assert.AreEqual(0, game.Check().Count);
        }

        [TestMethod]
        public void GiveHint_FillsAnEmptyCellWithItsSolutionValue()
        {
            var game = CreateGame();

            var result = game.GiveHint();

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(4, game.Board.GetValue(0, 2));
            StringAssert.Contains(result.Message, "A3 = 4");
        }

        [TestMethod]
        public void GiveHint_DoesNotOverwriteAUserEnteredValue()
        {
            var game = CreateGame();
            game.SetValue(0, 2, 4);

            var result = game.GiveHint();

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(4, game.Board.GetValue(0, 2));
            Assert.AreEqual(6, game.Board.GetValue(0, 3));
            StringAssert.Contains(result.Message, "A4 = 6");
        }

        [TestMethod]
        public void IsCompleted_ReturnsFalseForIncompleteBoard()
        {
            Assert.IsFalse(CreateGame().IsCompleted());
        }

        [TestMethod]
        public void IsCompleted_RecognizesCorrectSolution()
        {
            var game = CreateGame();
            FillEditableCells(game, useSolution: true);

            Assert.IsTrue(game.IsCompleted());
        }

        [TestMethod]
        public void IsCompleted_RejectsInvalidFilledBoard()
        {
            var game = CreateGame();
            for (var row = 0; row < 9; row++)
            for (var column = 0; column < 9; column++)
            {
                if (game.Board.IsFixed(row, column) || (row == 0 && column == 2)) continue;
                game.SetValue(row, column, game.Board.Puzzle.GetSolutionValue(row, column));
            }
            game.SetValue(0, 2, 1);

            Assert.IsFalse(game.IsCompleted());
            Assert.IsTrue(game.Check().Count > 0);
        }

        [TestMethod]
        public void GeneratedPuzzle_HasThirtyCluesAndAUniqueValidSolution()
        {
            var puzzle = _generator.CreateNewPuzzle(new Random(12345));
            var solver = new SudokuSolver();

            Assert.AreEqual(30, puzzle.FixedCellCount);
            Assert.AreEqual(0, new SudokuValidator().Validate(puzzle.CopyClues()).Count);
            Assert.IsTrue(solver.TrySolve(puzzle.CopyClues(), out var solved));
            Assert.AreEqual(1, solver.CountSolutions(puzzle.CopyClues()));
            AssertGridsEqual(puzzle.CopySolution(), solved);
        }

        [TestMethod]
        public void PuzzleCopies_DoNotAllowOriginalCluesToBeModified()
        {
            var puzzle = _generator.CreateInitialPuzzle();
            var copy = puzzle.CopyClues();
            copy[0, 0] = 0;

            Assert.AreEqual(5, puzzle.GetClue(0, 0));
        }

        [TestMethod]
        public void Puzzle_RejectsAnInvalidCompletedSolution()
        {
            var valid = _generator.CreateInitialPuzzle();
            var invalidSolution = valid.CopySolution();
            invalidSolution[0, 2] = 5;
            var clues = new int[9, 9];

            Assert.ThrowsException<ArgumentException>(() => new SudokuPuzzle(clues, invalidSolution));
        }

        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(10)]
        public void Solver_RejectsOutOfRangePuzzleValues(int invalidValue)
        {
            var puzzle = _generator.CreateInitialPuzzle().CopyClues();
            puzzle[0, 2] = invalidValue;
            var solver = new SudokuSolver();

            Assert.IsFalse(solver.TrySolve(puzzle, out _));
            Assert.AreEqual(0, solver.CountSolutions(puzzle));
        }

        [TestMethod]
        public void CompletedGame_RejectsFurtherChanges()
        {
            var game = CreateGame();
            FillEditableCells(game, useSolution: true);

            var setResult = game.SetValue(0, 2, 1);
            var clearResult = game.ClearValue(0, 2);
            var hintResult = game.GiveHint();

            Assert.IsFalse(setResult.Succeeded);
            Assert.IsFalse(clearResult.Succeeded);
            Assert.IsFalse(hintResult.Succeeded);
            Assert.AreEqual(4, game.Board.GetValue(0, 2));
            StringAssert.Contains(setResult.Message, "already complete");
        }

        private SudokuGame CreateGame() => new SudokuGame(_generator.CreateInitialPuzzle());

        private static void FillEditableCells(SudokuGame game, bool useSolution)
        {
            for (var row = 0; row < 9; row++)
            for (var column = 0; column < 9; column++)
            {
                if (game.Board.IsFixed(row, column)) continue;
                var value = useSolution ? game.Board.Puzzle.GetSolutionValue(row, column) : 1;
                game.SetValue(row, column, value);
            }
        }

        private static void AssertGridsEqual(int[,] expected, int[,] actual)
        {
            for (var row = 0; row < 9; row++)
            for (var column = 0; column < 9; column++)
                Assert.AreEqual(expected[row, column], actual[row, column], $"Mismatch at [{row},{column}].");
        }
    }
}
