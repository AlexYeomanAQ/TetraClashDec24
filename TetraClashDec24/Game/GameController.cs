using System;

namespace TetraClashDec24
{
    public class GameController
    {
        private Tetromino currentTetromino;
        public Tetromino CurrentTetromino
        {
            get => currentTetromino;
            private set
            {
                currentTetromino = value;
                currentTetromino.Reset();
            }
        }
        public int TotalLinesCleared;
        public int Level;
        public int Score;
        public GameBoard GameBoard { get; }
        public TetrominoQueue TetrominoQueue { get; }
        public bool GameOver { get; set; }
        public Tetromino HeldTetromino { get; private set; }
        public bool CanHold { get; private set; }

        public event EventHandler BlockLanded;
        public event EventHandler<int> LinesCleared;

        public GameController(int seed)
        {
            GameBoard = new GameBoard(22, 10);
            TetrominoQueue = new TetrominoQueue(seed);
            CanHold = true;
            TotalLinesCleared = 0;
            Level = 0;
            Score = 0;
            CurrentTetromino = TetrominoQueue.FetchAndRefresh();
        }

        private bool TetrominoFits()
        {
            foreach (Position p in CurrentTetromino.BlockPositions())
            {
                if (!GameBoard.IsCellClear(p.Row, p.Column))
                {
                    return false;
                }
            }
            return true;
        }

        public void RotateTetrominoCW()
        {
            CurrentTetromino.TurnClockwise();

            if (!TetrominoFits())
            {
                currentTetromino.TurnAntiClockwise();
            }
        }

        public void RotateTetrominoCCW()
        {
            CurrentTetromino.TurnAntiClockwise();

            if (!TetrominoFits())
            {
                CurrentTetromino.TurnClockwise();
            }
        }

        public void TranslateTetrominoLeft()
        {
            CurrentTetromino.Translate(0, -1);

            if (!TetrominoFits())
            {
                CurrentTetromino.Translate(0, 1);
            }
        }

        public void TranslateTetrominoRight()
        {
            CurrentTetromino.Translate(0, 1);

            if (!TetrominoFits())
            {
                CurrentTetromino.Translate(0, -1);
            }
        }

        private bool IsGameOver()
        {
            return !(GameBoard.IsLineEmpty(0) && GameBoard.IsLineEmpty(1));
        }

        public void HoldTetromino()
        {
            if (!CanHold)
            {
                return;
            }
            
            if (HeldTetromino == null)
            {
                HeldTetromino = currentTetromino;
                currentTetromino = TetrominoQueue.FetchAndRefresh();
            }
            else
            {
                Tetromino temporaryTetromino = CurrentTetromino;
                CurrentTetromino = HeldTetromino;
                HeldTetromino = temporaryTetromino;
            }

            CanHold = false;
        }

        public void PlaceTetromino()
        {
            foreach (Position p in CurrentTetromino.BlockPositions())
            {
                GameBoard[p.Row, p.Column] = CurrentTetromino.TetrominoID;
            }

            int numRowsCleared = GameBoard.ClearFullLines();

            if (numRowsCleared > 0)
            {
                LinesCleared?.Invoke(this, numRowsCleared);
            }

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
                CurrentTetromino = TetrominoQueue.FetchAndRefresh();
                CanHold = true;
            }
        }

        public void TranslateTetrominoDown()
        {
            CurrentTetromino.Translate(1, 0);

            if (!TetrominoFits())
            {
                CurrentTetromino.Translate(-1, 0);
                PlaceTetromino();
                BlockLanded?.Invoke(this, EventArgs.Empty);
            }
        }

        private int BlockDropDistance(Position p)
        {
            int drop = 0;

            while (GameBoard.IsCellClear(p.Row + 1 + drop, p.Column))
            {
                drop++;
            }

            return drop;
        }

        public int TetrominoDropDistance()
        {
            int drop = GameBoard.Rows;

            foreach (Position p in CurrentTetromino.BlockPositions())
            {
                drop = Math.Min(drop, BlockDropDistance(p));
            }

            return drop;
        }

        public void DropTetromino()
        {
            CurrentTetromino.Translate(TetrominoDropDistance(), 0);
            PlaceTetromino();

            BlockLanded?.Invoke(this, EventArgs.Empty);
        }
    }
}
