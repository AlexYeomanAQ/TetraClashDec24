﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    public class GameGrid
    {
        public readonly int[][] grid;
        public int Rows { get; }
        public int Collumns { get; }

        public int this[int r, int c]
        {
            get => grid[r][c];
            set => grid[r][c] = value;
        }

        public GameGrid(int rows, int collumns)
        {
            Rows = rows;
            Collumns = collumns;
            grid = new int[rows][];
            for (int i = 0; i < rows; i++)
            {
                grid[i] = new int[collumns];
            }
        }

        public bool IsInside(int r, int c)
        {
            return r >= 0 && r < Rows && c >= 0 && c < Collumns;
        }

        public bool IsEmpty(int r, int c)
        {
            return IsInside(r, c) && grid[r][c] == 0;
        }

        public bool IsRowFull(int r)
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

        public bool IsRowEmpty(int r)
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

        private void ClearRow(int r)
        {
            for (int c = 0; c < Collumns; c++)
            {
                grid[r][c] = 0;
            }
        }

        private void MoveRowDown(int r, int numRows)
        {
            for (int c = 0; c < Collumns; c++)
            {
                grid[r + numRows][c] = grid[r][c];
                grid[r][c] = 0;
            }
        }

        public int ClearFullRows()
        {
            int cleared = 0;

            for (int r = Rows - 1; r >= 0; r--)
            {
                if (IsRowFull(r))
                {
                    ClearRow(r);
                    cleared++;
                }
                else if (cleared > 0)
                {
                    MoveRowDown(r, cleared);
                }
            }

            return cleared;
        }
    }
}
