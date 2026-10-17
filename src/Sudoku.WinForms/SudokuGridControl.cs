using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sudoku.Core;

namespace Sudoku.WinForms
{
    /// <summary>Suresh Draws the labeled board and translates mouse/keyboard actions into cell events.</summary>
    internal sealed class SudokuGridControl : Control
    {
        private const int LabelMargin = 30;
        private const int CellSize = 48;
        private const int GridPixels = CellSize * 9;

        private readonly Font _fixedFont = new Font("Segoe UI", 15F, FontStyle.Bold);
        private readonly Font _editableFont = new Font("Segoe UI", 15F, FontStyle.Regular);
        private readonly Font _labelFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        private readonly HashSet<CellPosition> _errorCells = new HashSet<CellPosition>();
        private SudokuBoard _board;
        private CellPosition? _selectedCell;

        public SudokuGridControl()
        {
            DoubleBuffered = true;
            TabStop = true;
            BackColor = Color.White;
            Size = new Size(LabelMargin + GridPixels + 2, LabelMargin + GridPixels + 2);
            MinimumSize = Size;
            MaximumSize = Size;
            AccessibleName = "Sudoku board";
        }

        public event EventHandler<CellInputEventArgs> NumberEntered;
        public event EventHandler ClearRequested;
        public event EventHandler SelectionChanged;

        public CellPosition? SelectedCell => _selectedCell;

        public void Bind(SudokuBoard board)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _selectedCell = null;
            _errorCells.Clear();
            Invalidate();
        }

        public void SetErrorCells(IEnumerable<CellPosition> positions)
        {
            _errorCells.Clear();
            foreach (var position in positions ?? Enumerable.Empty<CellPosition>())
                _errorCells.Add(position);
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            var key = keyData & Keys.KeyCode;
            return key == Keys.Left || key == Keys.Right || key == Keys.Up || key == Keys.Down ||
                   base.IsInputKey(keyData);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.X < LabelMargin || e.Y < LabelMargin || e.X >= LabelMargin + GridPixels || e.Y >= LabelMargin + GridPixels)
                return;

            _selectedCell = new CellPosition((e.Y - LabelMargin) / CellSize, (e.X - LabelMargin) / CellSize);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!_selectedCell.HasValue) return;

            var value = KeyToValue(e.KeyCode);
            if (value > 0)
            {
                NumberEntered?.Invoke(this, new CellInputEventArgs(_selectedCell.Value, value));
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back || e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0)
            {
                ClearRequested?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                return;
            }

            var position = _selectedCell.Value;
            var row = position.Row;
            var column = position.Column;
            if (e.KeyCode == Keys.Left) column = Math.Max(0, column - 1);
            else if (e.KeyCode == Keys.Right) column = Math.Min(8, column + 1);
            else if (e.KeyCode == Keys.Up) row = Math.Max(0, row - 1);
            else if (e.KeyCode == Keys.Down) row = Math.Min(8, row + 1);
            else return;
            _selectedCell = new CellPosition(row, column);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
            e.Handled = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_board == null) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            DrawLabels(e.Graphics);
            DrawCells(e.Graphics);
            DrawGridLines(e.Graphics);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fixedFont.Dispose();
                _editableFont.Dispose();
                _labelFont.Dispose();
            }
            base.Dispose(disposing);
        }

        private void DrawLabels(Graphics graphics)
        {
            using (var brush = new SolidBrush(Color.FromArgb(55, 65, 81)))
            using (var format = CenteredFormat())
            {
                for (var index = 0; index < 9; index++)
                {
                    graphics.DrawString((index + 1).ToString(), _labelFont, brush,
                        new RectangleF(LabelMargin + (index * CellSize), 0, CellSize, LabelMargin), format);
                    graphics.DrawString(((char)('A' + index)).ToString(), _labelFont, brush,
                        new RectangleF(0, LabelMargin + (index * CellSize), LabelMargin, CellSize), format);
                }
            }
        }

        private void DrawCells(Graphics graphics)
        {
            using (var format = CenteredFormat())
            for (var row = 0; row < 9; row++)
            for (var column = 0; column < 9; column++)
            {
                var position = new CellPosition(row, column);
                var bounds = new Rectangle(LabelMargin + (column * CellSize), LabelMargin + (row * CellSize), CellSize, CellSize);
                var isSelected = _selectedCell.HasValue && _selectedCell.Value.Equals(position);
                var isError = _errorCells.Contains(position);
                var isFixed = _board.IsFixed(row, column);
                var background = isError ? Color.FromArgb(254, 226, 226)
                    : isSelected ? Color.FromArgb(219, 234, 254)
                    : isFixed ? Color.FromArgb(229, 231, 235)
                    : Color.White;
                using (var backgroundBrush = new SolidBrush(background))
                    graphics.FillRectangle(backgroundBrush, bounds);

                var value = _board.GetValue(row, column);
                var text = value == 0 ? "_" : value.ToString();
                var foreground = isError ? Color.FromArgb(185, 28, 28)
                    : isFixed ? Color.FromArgb(17, 24, 39)
                    : Color.FromArgb(29, 78, 216);
                using (var textBrush = new SolidBrush(foreground))
                    graphics.DrawString(text, isFixed ? _fixedFont : _editableFont, textBrush, bounds, format);
            }
        }

        private static void DrawGridLines(Graphics graphics)
        {
            for (var index = 0; index <= 9; index++)
            {
                var width = index % 3 == 0 ? 3F : 1F;
                var color = index % 3 == 0 ? Color.FromArgb(31, 41, 55) : Color.FromArgb(156, 163, 175);
                using (var pen = new Pen(color, width))
                {
                    var offset = LabelMargin + (index * CellSize);
                    graphics.DrawLine(pen, LabelMargin, offset, LabelMargin + GridPixels, offset);
                    graphics.DrawLine(pen, offset, LabelMargin, offset, LabelMargin + GridPixels);
                }
            }
        }

        private static StringFormat CenteredFormat() => new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        private static int KeyToValue(Keys key)
        {
            if (key >= Keys.D1 && key <= Keys.D9) return key - Keys.D0;
            if (key >= Keys.NumPad1 && key <= Keys.NumPad9) return key - Keys.NumPad0;
            return 0;
        }
    }
}
