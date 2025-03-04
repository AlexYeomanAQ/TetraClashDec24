﻿using Microsoft.Xna.Framework.Content;
using System;
using System.Web;
using System.Windows.Forms.VisualStyles;

namespace TetraClashDec24
{
    public class GameState
    {
        private Block currentBlock;
        public Block CurrentBlock
        {
            get => currentBlock;
            private set
            {
                currentBlock = value;
                currentBlock.Reset();
            }
        }
        public int TotalLinesCleared;
        public int Level;
        public int Score;
        public GameGrid GameGrid { get; }
        public BlockQueue BlockQueue { get; }
        public bool GameOver { get; set; }
        public Block HeldBlock { get; private set; }
        public bool CanHold { get; private set; }

        public GameState(int seed)
        {
            GameGrid = new GameGrid(22, 10);
            BlockQueue = new BlockQueue(seed);
            CanHold = true;
            TotalLinesCleared = 0;
            Level = 0;
            Score = 0;
            CurrentBlock = BlockQueue.GetAndUpdate();
        }

        private bool BlockFits()
        {
            foreach (Position p in CurrentBlock.TilePositions())
            {
                if (!GameGrid.IsEmpty(p.Row, p.Column))
                {
                    return false;
                }
            }
            return true;
        }

        public void RotateBlockCW()
        {
            CurrentBlock.RotateCW();

            if (!BlockFits())
            {
                currentBlock.RotateCCW();
            }
        }

        public void RotateBlockCCW()
        {
            CurrentBlock.RotateCCW();

            if (!BlockFits())
            {
                CurrentBlock.RotateCW();
            }
        }

        public void MoveBlockLeft()
        {
            CurrentBlock.Move(0, -1);

            if (!BlockFits())
            {
                CurrentBlock.Move(0, 1);
            }
        }

        public void MoveBlockRight()
        {
            CurrentBlock.Move(0, 1);

            if (!BlockFits())
            {
                CurrentBlock.Move(0, -1);
            }
        }

        private bool IsGameOver()
        {
            return !(GameGrid.IsRowEmpty(0) && GameGrid.IsRowEmpty(1));
        }

        public void HoldBlock()
        {
            if (!CanHold)
            {
                return;
            }
            
            if (HeldBlock == null)
            {
                HeldBlock = currentBlock;
                currentBlock = BlockQueue.GetAndUpdate();
            }
            else
            {
                Block temporaryBlock = CurrentBlock;
                CurrentBlock = HeldBlock;
                HeldBlock = temporaryBlock;
            }

            CanHold = false;
        }

        public void PlaceBlock()
        {
            foreach (Position p in CurrentBlock.TilePositions())
            {
                GameGrid[p.Row, p.Column] = CurrentBlock.TetrominoID;
            }

            int numRowsCleared = GameGrid.ClearFullRows();
            Score += Cogs.lineClearPoints[numRowsCleared]*(Level+1);
            for (int i = 0; i < numRowsCleared; i++)
            {
                TotalLinesCleared++;
                if (TotalLinesCleared %10 == 0)
                {
                    Level++;
                }
            }
            if (IsGameOver())
            {
                GameOver = true;
            }
            else
            {
                CurrentBlock = BlockQueue.GetAndUpdate();
                CanHold = true;
            }

        }

        public void MoveBlockDown()
        {
            CurrentBlock.Move(1, 0);

            if (!BlockFits())
            {
                CurrentBlock.Move(-1, 0);
                PlaceBlock();
            }
        }

        private int TileDropDistance(Position p)
        {
            int drop = 0;

            while (GameGrid.IsEmpty(p.Row + 1 + drop, p.Column))
            {
                drop++;
            }

            return drop;
        }

        public int BlockDropDistance()
        {
            int drop = GameGrid.Rows;

            foreach (Position p in CurrentBlock.TilePositions())
            {
                drop = Math.Min(drop, TileDropDistance(p));
            }

            return drop;
        }

        public void DropBlock()
        {
            CurrentBlock.Move(BlockDropDistance(), 0);
            PlaceBlock();
        }
    }
}
