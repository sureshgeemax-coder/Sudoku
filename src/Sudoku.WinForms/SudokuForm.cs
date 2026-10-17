using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sudoku.Core;

namespace Sudoku.WinForms
{
    /// <summary>G Sureshkumar Main application window responsible only for presentation and user interaction.</summary>
    public sealed class SudokuForm : Form
    {
        private readonly SudokuPuzzleGenerator _generator = new SudokuPuzzleGenerator();
        private readonly SudokuGridControl _grid = new SudokuGridControl();
        private readonly Label _statusLabel = new Label();
        private SudokuGame _game;
        private bool _completionShown;

        public SudokuForm()
        {
            Text = "Sudoku Game";
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(790, 625);
            MinimumSize = new Size(806, 664);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            KeyPreview = true;
            AccessibleName = "Sudoku Game";

            BuildInterface();
            WireEvents();
            StartGame(_generator.CreateInitialPuzzle(), "Select a cell, then enter 1-9 using the keyboard or number pad.");
        }

        private void BuildInterface()
        {
            var title = new Label
            {
                Text = "Sudoku Game",
                Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Location = new Point(28, 16)
            };
            var subtitle = new Label
            {
                Text = "Fill every row, column, and 3x3 box with the numbers 1-9.",
                ForeColor = Color.FromArgb(71, 85, 105),
                AutoSize = true,
                Location = new Point(32, 62)
            };
            _grid.Location = new Point(28, 94);

            var sidePanel = new Panel { Location = new Point(520, 98), Size = new Size(240, 458) };
            var inputLabel = SectionLabel("Enter a number", 0);
            sidePanel.Controls.Add(inputLabel);

            var numberPad = new TableLayoutPanel
            {
                Location = new Point(0, 32),
                Size = new Size(210, 150),
                ColumnCount = 3,
                RowCount = 3
            };
            for (var index = 0; index < 3; index++)
            {
                numberPad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
                numberPad.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333F));
            }
            for (var value = 1; value <= 9; value++)
            {
                var capturedValue = value;
                var button = CreateButton(value.ToString(), Color.White, Color.FromArgb(30, 64, 175));
                button.Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
                button.Margin = new Padding(4);
                button.Click += (sender, args) => EnterSelectedValue(capturedValue);
                numberPad.Controls.Add(button, (value - 1) % 3, (value - 1) / 3);
            }
            sidePanel.Controls.Add(numberPad);

            var actionsLabel = SectionLabel("Game actions", 198);
            sidePanel.Controls.Add(actionsLabel);
            var actions = new TableLayoutPanel
            {
                Location = new Point(0, 230),
                Size = new Size(220, 210),
                ColumnCount = 2,
                RowCount = 3
            };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (var index = 0; index < 3; index++) actions.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333F));

            var hintButton = CreateButton("Hint", Color.FromArgb(254, 249, 195), Color.FromArgb(133, 77, 14));
            hintButton.Click += (sender, args) => ApplyResult(_game.GiveHint());
            var checkButton = CreateButton("Check", Color.FromArgb(220, 252, 231), Color.FromArgb(22, 101, 52));
            checkButton.Click += CheckBoard;
            var clearButton = CreateButton("Clear Cell", Color.FromArgb(241, 245, 249), Color.FromArgb(51, 65, 85));
            clearButton.Click += (sender, args) => ClearSelectedCell();
            var newButton = CreateButton("New Game", Color.FromArgb(219, 234, 254), Color.FromArgb(30, 64, 175));
            newButton.Click += (sender, args) => StartGame(_generator.CreateNewPuzzle(), "New 30-clue puzzle created. Select a cell to begin.");
            var quitButton = CreateButton("Quit", Color.FromArgb(254, 226, 226), Color.FromArgb(153, 27, 27));
            quitButton.Click += (sender, args) => Close();
            actions.Controls.Add(hintButton, 0, 0);
            actions.Controls.Add(checkButton, 1, 0);
            actions.Controls.Add(clearButton, 0, 1);
            actions.SetColumnSpan(clearButton, 2);
            actions.Controls.Add(newButton, 0, 2);
            actions.Controls.Add(quitButton, 1, 2);
            sidePanel.Controls.Add(actions);

            var statusPanel = new Panel
            {
                Location = new Point(28, 566),
                Size = new Size(732, 44),
                BackColor = Color.FromArgb(226, 232, 240),
                Padding = new Padding(12, 9, 12, 8)
            };
            _statusLabel.Dock = DockStyle.Fill;
            _statusLabel.ForeColor = Color.FromArgb(30, 41, 59);
            _statusLabel.AutoEllipsis = true;
            _statusLabel.AccessibleName = "Game status";
            statusPanel.Controls.Add(_statusLabel);

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(_grid);
            Controls.Add(sidePanel);
            Controls.Add(statusPanel);
            AcceptButton = checkButton;
            CancelButton = quitButton;
        }

        private void WireEvents()
        {
            _grid.NumberEntered += (sender, args) => ApplyResult(_game.SetValue(args.Position.Row, args.Position.Column, args.Value));
            _grid.ClearRequested += (sender, args) => ClearSelectedCell();
            _grid.SelectionChanged += (sender, args) =>
            {
                if (!_grid.SelectedCell.HasValue) return;
                var cell = _grid.SelectedCell.Value;
                var cellType = _game.Board.IsFixed(cell.Row, cell.Column) ? "pre-filled" : "editable";
                ShowStatus($"Selected Cell {cell.Name} ({cellType}).", false);
            };
        }

        private void StartGame(SudokuPuzzle puzzle, string message)
        {
            _game = new SudokuGame(puzzle);
            _completionShown = false;
            _grid.Bind(_game.Board);
            ShowStatus(message, false);
            _grid.Focus();
        }

        private void EnterSelectedValue(int value)
        {
            if (!_grid.SelectedCell.HasValue)
            {
                ShowStatus("Select a cell before entering a number.", true);
                return;
            }
            var cell = _grid.SelectedCell.Value;
            ApplyResult(_game.SetValue(cell.Row, cell.Column, value));
        }

        private void ClearSelectedCell()
        {
            if (!_grid.SelectedCell.HasValue)
            {
                ShowStatus("Select an editable cell before clearing it.", true);
                return;
            }
            var cell = _grid.SelectedCell.Value;
            ApplyResult(_game.ClearValue(cell.Row, cell.Column));
        }

        private void CheckBoard(object sender, EventArgs eventArgs)
        {
            var issues = _game.Check();
            if (issues.Count == 0)
            {
                _grid.SetErrorCells(Array.Empty<CellPosition>());
                ShowStatus(_game.IsCompleted() ? "You have successfully completed the Sudoku puzzle!" : "No rule violations detected.", false);
                if (_game.IsCompleted()) ShowCompletionMessage();
                return;
            }

            var cells = issues.SelectMany(issue => issue.Cells);
            _grid.SetErrorCells(cells);
            var suffix = issues.Count == 1 ? string.Empty : $" ({issues.Count} conflicts found)";
            ShowStatus(issues[0].Message + suffix, true);
        }

        private void ApplyResult(SudokuMoveResult result)
        {
            var conflict = result.Message.IndexOf("conflict", StringComparison.OrdinalIgnoreCase) >= 0;
            // Recalculate all conflicts so an unrelated action cannot hide an unresolved violation.
            _grid.SetErrorCells(_game.Check().SelectMany(issue => issue.Cells));
            ShowStatus(result.Message, !result.Succeeded || conflict);
            if (_game.IsCompleted() && !_completionShown)
            {
                _completionShown = true;
                ShowCompletionMessage();
            }
        }

        private void ShowCompletionMessage()
        {
            MessageBox.Show("You have successfully completed the Sudoku puzzle!\n\nChoose New Game to play another puzzle, or Quit to exit.",
                "Puzzle Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowStatus(string message, bool isError)
        {
            _statusLabel.Text = message;
            _statusLabel.ForeColor = isError ? Color.FromArgb(153, 27, 27) : Color.FromArgb(30, 41, 59);
        }

        private static Label SectionLabel(string text, int top) => new Label
        {
            Text = text,
            Location = new Point(4, top),
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 41, 59)
        };

        private static Button CreateButton(string text, Color backColor, Color foreColor) => new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(5),
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor,
            ForeColor = foreColor,
            UseVisualStyleBackColor = false,
            Cursor = Cursors.Hand,
            AccessibleName = text
        };
    }
}
