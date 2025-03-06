namespace TetraClashDec24
{
    public class GameBoard
    {
        public readonly int[][] grid;
        public int Rows { get; }
        public int Collumns { get; }

        public int this[int r, int c]
        {
            get => grid[r][c];
            set => grid[r][c] = value;
        }

        public GameBoard(int rows, int collumns)
        {
            Rows = rows;
            Collumns = collumns;
            grid = new int[rows][];
            for (int i = 0; i < rows; i++)
            {
                grid[i] = new int[collumns];
            }
        }

        public bool IsInBounds(int r, int c)
        {
            return r >= 0 && r < Rows && c >= 0 && c < Collumns;
        }

        public bool IsCellClear(int r, int c)
        {
            return IsInBounds(r, c) && grid[r][c] == 0;
        }

        public bool IsLineComplete(int r)
        {
            for (int c = 0; c < Collumns; c++)
            {
                if (grid[r][c] == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsLineEmpty(int r)
        {
            for (int c = 0; c < Collumns; c++)
            {
                if (grid[r][c] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void ClearLine(int r)
        {
            for (int c = 0; c < Collumns; c++)
            {
                grid[r][c] = 0;
            }
        }

        private void ShiftLineDown(int r, int numRows)
        {
            for (int c = 0; c < Collumns; c++)
            {
                grid[r + numRows][c] = grid[r][c];
                grid[r][c] = 0;
            }
        }

        public int ClearFullLines()
        {
            int cleared = 0;

            for (int r = Rows - 1; r >= 0; r--)
            {
                if (IsLineComplete(r))
                {
                    ClearLine(r);
                    cleared++;
                }
                else if (cleared > 0)
                {
                    ShiftLineDown(r, cleared);
                }
            }

            return cleared;
        }
    }
}
