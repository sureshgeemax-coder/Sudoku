using System;

namespace Sudoku.Core
{
    /// <summary>Suresh Identifies a zero-based cell while presenting the user-facing A1-I9 notation.</summary>
    public readonly struct CellPosition : IEquatable<CellPosition>
    {
        public CellPosition(int row, int column)
        {
            if (row < 0 || row >= SudokuBoard.Size)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (column < 0 || column >= SudokuBoard.Size)
                throw new ArgumentOutOfRangeException(nameof(column));

            Row = row;
            Column = column;
        }

        public int Row { get; }
        public int Column { get; }
        public string Name => $"{(char)('A' + Row)}{Column + 1}";

        public bool Equals(CellPosition other) => Row == other.Row && Column == other.Column;
        public override bool Equals(object obj) => obj is CellPosition other && Equals(other);
        public override int GetHashCode() => (Row * SudokuBoard.Size) + Column;
        public override string ToString() => Name;
    }
}
