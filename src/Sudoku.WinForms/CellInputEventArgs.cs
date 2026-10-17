using System;
using Sudoku.Core;

namespace Sudoku.WinForms
{
    internal sealed class CellInputEventArgs : EventArgs
    {
        public CellInputEventArgs(CellPosition position, int value)
        {
            Position = position;
            Value = value;
        }

        public CellPosition Position { get; }
        public int Value { get; }
    }
}
