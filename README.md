# Sudoku Game

A desktop Sudoku game implemented in C# and Windows Forms for .NET Framework 4.8. The application starts with the required 30-clue puzzle and supports mouse and keyboard play entirely through its graphical interface.

## Requirements

- Windows 10 or later
- .NET Framework 4.8 runtime
- Visual Studio 2022 or later with the **.NET desktop development** workload and .NET Framework 4.8 targeting pack
- Alternatively, a recent .NET SDK plus the .NET Framework 4.8 targeting pack can build the SDK-style projects from a terminal

Windows Forms and .NET Framework 4.8 are Windows-only. No external Sudoku-solving library or runtime service is used. MSTest packages are restored from NuGet for the test project.

## Features

- Labeled 9x9 board with clear 3x3 boundaries and visible empty-cell underscores
- Exactly 30 fixed clues in the initial puzzle
- Distinct styling for fixed clues, editable entries, selection, and conflicts
- Number entry through keys `1`-`9`, the numeric keypad, or on-screen number buttons
- Arrow-key cell navigation and clearing with `Delete`, `Backspace`, `0`, or **Clear Cell**
- Fixed-cell and range protection with user-facing status messages
- Immediate duplicate feedback and full row, column, and subgrid validation through **Check**
- Correct hints that fill an empty editable cell without overwriting user entries
- Valid, unique 30-clue puzzle variations through **New Game**
- Strict completion detection that checks fullness, Sudoku rules, and the stored solution
- Friendly completion, error, instruction, and validation messages

## Design

The solution separates presentation from game rules:

- `src/Sudoku.Core/SudokuPuzzle.cs`: immutable clue and solution data.
- `src/Sudoku.Core/SudokuBoard.cs`: current mutable values and fixed-cell queries.
- `src/Sudoku.Core/SudokuGame.cs`: move, clear, hint, check, and completion use cases.
- `src/Sudoku.Core/SudokuValidator.cs`: shared duplicate detection for rows, columns, and boxes. Empty cells are excluded and values are grouped by unit, so a cell is never compared with itself.
- `src/Sudoku.Core/SudokuSolver.cs`: deterministic backtracking solver with minimum-remaining-values cell selection. It also counts solutions up to a caller-provided limit.
- `src/Sudoku.Core/SudokuPuzzleGenerator.cs`: supplies the required initial puzzle and creates new puzzles by applying digit, row, column, band, and stack permutations. These transformations preserve validity, the 30-clue count, and the initial puzzle's unique solution.
- `src/Sudoku.WinForms/SudokuGridControl.cs`: custom board drawing, labels, selection, keyboard navigation, and visual state.
- `src/Sudoku.WinForms/SudokuForm.cs`: layout, event wiring, and user-facing messages only.
- `tests/Sudoku.Tests`: MSTest logic and WinForms integration coverage.

The original clues and solution are copied into immutable `SudokuPuzzle` storage. Only `SudokuGame` can invoke the board's internal mutation method, ensuring that all changes pass through fixed-cell and input validation. Hints read the stored solution but only fill empty editable cells. Completion requires a full valid board that also matches the current puzzle solution.

Moves that introduce a duplicate remain visible so the player can correct them, matching the assessment examples. The status area immediately explains the conflict and the involved cells are highlighted. **Check** reports existing violations independently.

## Build

From the repository root:

```powershell
dotnet restore Sudoku.sln
dotnet build Sudoku.sln -c Release --no-restore
```

The same solution can be opened in Visual Studio and built with **Build > Build Solution**.

## Run

After building:

```powershell
& ".\src\Sudoku.WinForms\bin\Release\net48\Sudoku.Game.exe"
```

In Visual Studio, set `Sudoku.WinForms` as the startup project and press `F5`.

## Test

```powershell
dotnet test .\tests\Sudoku.Tests\Sudoku.Tests.csproj -c Release --no-build
```

The suite covers the clue count, puzzle validity and uniqueness, invalid public puzzle data, solver input ranges, valid and invalid moves, fixed-cell protection, value range checks, all three duplicate scopes, empty-cell handling, hints, completion locking, generated puzzles, immutability, and real WinForms action-button wiring. Tests use no external services or arbitrary delays.

## Manual UI Verification

1. Launch the application and confirm the title, A-I row labels, 1-9 column labels, 30 fixed clues, underscores, and bold 3x3 borders are visible.
2. Select an editable cell, enter a number with the keyboard and on-screen pad, navigate with arrow keys, and clear it with both the keyboard and **Clear Cell**.
3. Select a fixed cell and verify number entry and clearing are rejected with the cell name in the status area.
4. Enter row, column, and box duplicates; verify immediate messages, red highlighting, and **Check** results.
5. Click **Hint** and confirm an empty cell receives the correct value without changing existing entries.
6. Click **New Game** and confirm a different 30-clue board is displayed.
7. Complete the board and confirm the completion dialog appears only for the correct solution.
8. Click **Quit** and confirm the application closes.

## Project Layout

```text
Sudoku.sln
src/
  Sudoku.Core/       Framework-independent Sudoku rules and algorithms
  Sudoku.WinForms/   Windows Forms executable
tests/
  Sudoku.Tests/      MSTest unit and integration tests
```

## Assumptions And Limitations

- New puzzles are validity-preserving variations of the required unique puzzle rather than newly carved grids. This guarantees 30 clues and a unique solution while avoiding slow or nondeterministic runtime generation.
- Game progress is kept in memory and is not persisted after the application closes.
- Automated tests exercise form construction and button wiring, while visual appearance and pointer interaction use the documented manual verification checklist.
