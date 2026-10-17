using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Core
{
    /// <summary>Coordinates moves, hints, validation, and completion for one Sudoku game.</summary>
    public sealed class SudokuGame
    {
        private readonly SudokuValidator _validator;

        public SudokuGame(SudokuPuzzle puzzle, SudokuValidator validator = null)
        {
            Board = new SudokuBoard(puzzle ?? throw new ArgumentNullException(nameof(puzzle)));
            _validator = validator ?? new SudokuValidator();
        }

        public SudokuBoard Board { get; }

        public SudokuMoveResult SetValue(int row, int column, int value)
        {
            var position = GetPosition(row, column);
            if (IsCompleted())
                return new SudokuMoveResult(false, "The puzzle is already complete. Start a new game to continue playing.");
            if (Board.IsFixed(row, column))
                return new SudokuMoveResult(false, $"Invalid move: Cell {position.Name} is pre-filled and cannot be changed.", new[] { position });
            if (value < 1 || value > 9)
                return new SudokuMoveResult(false, "Invalid input: enter a number from 1 to 9.", new[] { position });

            Board.SetEditableValue(row, column, value);
            var conflicts = GetIssuesFor(position).ToList();
            if (conflicts.Count > 0)
                return new SudokuMoveResult(true, $"Entry accepted with a conflict: {conflicts[0].Message}", conflicts.SelectMany(issue => issue.Cells));
            if (IsCompleted())
                return new SudokuMoveResult(true, "You have successfully completed the Sudoku puzzle!", new[] { position });
            return new SudokuMoveResult(true, $"Move accepted: Cell {position.Name} = {value}.", new[] { position });
        }

        public SudokuMoveResult ClearValue(int row, int column)
        {
            var position = GetPosition(row, column);
            if (IsCompleted())
                return new SudokuMoveResult(false, "The puzzle is already complete. Start a new game to continue playing.");
            if (Board.IsFixed(row, column))
                return new SudokuMoveResult(false, $"Invalid move: Cell {position.Name} is pre-filled and cannot be cleared.", new[] { position });
            if (Board.GetValue(row, column) == 0)
                return new SudokuMoveResult(true, $"Cell {position.Name} is already empty.", new[] { position });

            Board.SetEditableValue(row, column, 0);
            return new SudokuMoveResult(true, $"Cell {position.Name} cleared.", new[] { position });
        }

        public SudokuMoveResult GiveHint()
        {
            if (IsCompleted())
                return new SudokuMoveResult(false, "The puzzle is already complete. Start a new game to continue playing.");
            for (var row = 0; row < 9; row++)
            for (var column = 0; column < 9; column++)
            {
                // Only empty editable cells are candidates, so hints never overwrite user work or alter clues.
                if (Board.IsFixed(row, column) || Board.GetValue(row, column) != 0) continue;
                var value = Board.Puzzle.GetSolutionValue(row, column);
                Board.SetEditableValue(row, column, value);
                var position = new CellPosition(row, column);
                var message = IsCompleted()
                    ? "You have successfully completed the Sudoku puzzle!"
                    : $"Hint: Cell {position.Name} = {value}.";
                return new SudokuMoveResult(true, message, new[] { position });
            }
            return new SudokuMoveResult(false, "A hint is unavailable because no empty editable cells remain.");
        }

        public IReadOnlyList<ValidationIssue> Check() => _validator.Validate(Board);

        public bool IsCompleted()
        {
            var values = Board.CopyValues();
            if (!_validator.IsCompleteAndValid(values)) return false;
            for (var row = 0; row < 9; row++)
            for (var column = 0; column < 9; column++)
                if (values[row, column] != Board.Puzzle.GetSolutionValue(row, column)) return false;
            return true;
        }

        private IEnumerable<ValidationIssue> GetIssuesFor(CellPosition position) =>
            _validator.Validate(Board).Where(issue => issue.Cells.Contains(position));

        private static CellPosition GetPosition(int row, int column) => new CellPosition(row, column);
    }
}
