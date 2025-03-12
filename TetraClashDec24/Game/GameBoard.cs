namespace TetraClashDec24
{
    // The GameBoard class represents the playing field for the game.
    // It manages a grid of integers where each cell represents a block (or empty space),
    // and provides functionality to manipulate the grid, such as clearing completed rows.
    public class GameBoard
    {
        // The grid is implemented as a jagged array, where each sub-array represents a row.
        public readonly int[][] grid;

        // Properties to hold the total number of rows and columns in the grid.
        public int Rows { get; }
        public int Collumns { get; }

        // Indexer to simplify accessing or setting a specific cell in the grid.
        // It allows using board[r, c] to get or set the value at row r and column c.
        public int this[int r, int c]
        {
            get => grid[r][c];   // Return the value at position (r, c)
            set => grid[r][c] = value;   // Set the value at position (r, c)
        }

        // Constructor for GameBoard.
        // Initializes the board with a specified number of rows and columns.
        public GameBoard(int rows, int collumns)
        {
            // Set the Rows and Collumns properties.
            Rows = rows;
            Collumns = collumns;

            // Initialize the jagged array with the given number of rows.
            grid = new int[rows][];

            // For each row, allocate an array for the columns.
            for (int i = 0; i < rows; i++)
            {
                grid[i] = new int[collumns];
            }
        }

        // Checks if a given cell (r, c) is within the bounds of the grid.
        public bool IsInBounds(int r, int c)
        {
            // Returns true if row and column indices are within valid ranges.
            return r >= 0 && r < Rows && c >= 0 && c < Collumns;
        }

        // Determines if a specific cell (r, c) is clear (i.e., not occupied).
        public bool IsCellClear(int r, int c)
        {
            // A cell is clear if it is in bounds and its value is 0.
            return IsInBounds(r, c) && grid[r][c] == 0;
        }

        // Checks if an entire row (r) is complete (i.e., every cell is non-zero).
        public bool IsLineComplete(int r)
        {
            // Iterate through each column in the row.
            for (int c = 0; c < Collumns; c++)
            {
                // If any cell is 0, the line is not complete.
                if (grid[r][c] == 0)
                {
                    return false;
                }
            }
            // If no cells are 0, the row is complete.
            return true;
        }

        // Checks if an entire row (r) is empty (i.e., every cell is 0).
        public bool IsLineEmpty(int r)
        {
            // Iterate through each column in the row.
            for (int c = 0; c < Collumns; c++)
            {
                // If any cell is non-zero, the line is not empty.
                if (grid[r][c] != 0)
                {
                    return false;
                }
            }
            // If all cells are 0, the row is empty.
            return true;
        }

        // Clears all cells in a specified row (r) by setting their values to 0.
        private void ClearLine(int r)
        {
            // Iterate over each cell in the row and set it to 0.
            for (int c = 0; c < Collumns; c++)
            {
                grid[r][c] = 0;
            }
        }

        // Shifts a row (r) downward by a given number of rows (numRows).
        // This is used after clearing lines to move down the rows above.
        private void ShiftLineDown(int r, int numRows)
        {
            // For each cell in the row, copy its value to the cell numRows below,
            // then set the current cell to 0.
            for (int c = 0; c < Collumns; c++)
            {
                grid[r + numRows][c] = grid[r][c];
                grid[r][c] = 0;
            }
        }

        // Clears all complete lines on the board and shifts down the rows above.
        // Returns the number of lines that were cleared.
        public int ClearFullLines()
        {
            int cleared = 0;  // Counter for the number of lines cleared

            // Iterate from the bottom row upwards.
            for (int r = Rows - 1; r >= 0; r--)
            {
                // If the current row is complete, clear it.
                if (IsLineComplete(r))
                {
                    ClearLine(r);
                    cleared++;  // Increase the cleared lines count
                }
                // If the row is not complete and there have been cleared lines below,
                // shift the row downward by the number of cleared lines.
                else if (cleared > 0)
                {
                    ShiftLineDown(r, cleared);
                }
            }

            // Return the total number of cleared rows.
            return cleared;
        }
    }
}
