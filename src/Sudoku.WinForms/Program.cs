using System;
using System.Windows.Forms;

namespace Sudoku.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += (sender, args) =>
                MessageBox.Show($"Sudoku encountered an unexpected error:\n{args.Exception.Message}",
                    "Sudoku Game", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Run(new SudokuForm());
        }
    }
}
