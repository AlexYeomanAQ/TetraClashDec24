using System;

namespace TetraClashDec24
{
    // The GameController class orchestrates the gameplay of a Tetris-like game.
    public class GameController
    {
        private Tetromino currentTetromino; // Field holding the current tetromino in play.

        // Public property for accessing the current tetromino; resets it when assigned.
        public Tetromino CurrentTetromino
        {
            get => currentTetromino; // Return the current tetromino.
            private set { currentTetromino = value; currentTetromino.Reset(); } // Set and reset tetromino.
        }

        public int TotalLinesCleared; // Total lines cleared throughout the game.
        public int Level;             // Current game level.
        public int Score;             // Player's current score.
        public GameBoard GameBoard { get; } // The game board instance.
        public TetrominoQueue TetrominoQueue { get; } // Manages the upcoming tetrominos.
        public bool GameOver { get; set; } // Flag indicating whether the game is over.
        public Tetromino HeldTetromino { get; private set; } // The tetromino held for later use.
        public bool CanHold { get; private set; } // Indicates if the player can hold a tetromino.

        public event EventHandler BlockLanded;      // Event fired when a tetromino lands.
        public event EventHandler<int> LinesCleared;  // Event fired when lines are cleared (carries number of lines).

        // Constructor: Initializes game state, including board, tetromino queue, and starting tetromino.
        public GameController(int seed)
        {
            GameBoard = new GameBoard(22, 10); // Create a game board with 22 rows and 10 columns.
            TetrominoQueue = new TetrominoQueue(seed); // Initialize tetromino queue with a seed.
            CanHold = true; // Allow holding a tetromino initially.
            TotalLinesCleared = 0; // Start with 0 cleared lines.
            Level = 0; // Initial level is 0.
            Score = 0; // Initial score is 0.
            CurrentTetromino = TetrominoQueue.FetchAndRefresh(); // Fetch the first tetromino.
        }

        // Checks if the current tetromino fits on the board by verifying each block's position.
        private bool TetrominoFits()
        {
            foreach (Position p in CurrentTetromino.BlockPositions()) // For each block in the tetromino,
            {
                if (!GameBoard.IsCellClear(p.Row, p.Column)) // if the corresponding board cell is not clear,
                {
                    return false; // the tetromino does not fit.
                }
            }
            return true; // All blocks fit in clear cells.
        }

        // Rotates the tetromino clockwise and reverts if it doesn't fit.
        public void RotateTetrominoCW()
        {
            CurrentTetromino.TurnClockwise(); // Rotate tetromino clockwise.
            if (!TetrominoFits()) // If the new orientation collides,
            {
                currentTetromino.TurnAntiClockwise(); // revert to the previous orientation.
            }
        }

        // Rotates the tetromino counterclockwise and reverts if it doesn't fit.
        public void RotateTetrominoCCW()
        {
            CurrentTetromino.TurnAntiClockwise(); // Rotate tetromino counterclockwise.
            if (!TetrominoFits()) // If the new orientation collides,
            {
                CurrentTetromino.TurnClockwise(); // revert to the previous orientation.
            }
        }

        // Moves the tetromino one cell to the left; reverts the move if it doesn't fit.
        public void TranslateTetrominoLeft()
        {
            CurrentTetromino.Translate(0, -1); // Move tetromino left by one column.
            if (!TetrominoFits()) // If the move causes a collision,
            {
                CurrentTetromino.Translate(0, 1); // revert the move.
            }
        }

        // Moves the tetromino one cell to the right; reverts the move if it doesn't fit.
        public void TranslateTetrominoRight()
        {
            CurrentTetromino.Translate(0, 1); // Move tetromino right by one column.
            if (!TetrominoFits()) // If the move causes a collision,
            {
                CurrentTetromino.Translate(0, -1); // revert the move.
            }
        }

        // Determines if the game is over by checking if the top two rows are occupied.
        private bool IsGameOver()
        {
            return !(GameBoard.IsLineEmpty(0) && GameBoard.IsLineEmpty(1)); // Game over if either top row is not empty.
        }

        // Allows the player to hold the current tetromino for later use, swapping if one is already held.
        public void HoldTetromino()
        {
            if (!CanHold) return; // Exit if holding is not allowed.

            if (HeldTetromino == null) // If no tetromino is held,
            {
                HeldTetromino = currentTetromino; // hold the current tetromino,
                currentTetromino = TetrominoQueue.FetchAndRefresh(); // and fetch a new one.
            }
            else // If there is already a held tetromino,
            {
                Tetromino temporaryTetromino = CurrentTetromino; // store the current tetromino temporarily,
                CurrentTetromino = HeldTetromino; // swap with the held tetromino,
                HeldTetromino = temporaryTetromino; // assign the held tetromino to the temporary one.
            }
            CanHold = false; // Disable further holds until the tetromino is placed.
        }

        // Places the current tetromino onto the board, clears full lines, updates score/level, and fetches a new tetromino.
        public void PlaceTetromino()
        {
            foreach (Position p in CurrentTetromino.BlockPositions()) // For each block of the tetromino,
            {
                GameBoard[p.Row, p.Column] = CurrentTetromino.TetrominoID; // mark the board cell with the tetromino's ID.
            }
            int numRowsCleared = GameBoard.ClearFullLines(); // Clear full lines and get the count.
            if (numRowsCleared > 0) // If any lines were cleared,
            {
                LinesCleared?.Invoke(this, numRowsCleared); // trigger the LinesCleared event.
            }
            Score += Cogs.lineClearPoints[numRowsCleared] * (Level + 1); // Update score based on cleared lines and level.
            for (int i = 0; i < numRowsCleared; i++) // For each cleared line,
            {
                TotalLinesCleared++; // increment total cleared lines.
                if (TotalLinesCleared % 10 == 0) // Every 10 cleared lines,
                {
                    Level++; // increase the level.
                }
            }
            if (IsGameOver()) // Check if game over condition is met,
            {
                GameOver = true; // set the game over flag.
            }
            else // If the game continues,
            {
                CurrentTetromino = TetrominoQueue.FetchAndRefresh(); // fetch the next tetromino,
                CanHold = true; // and re-enable holding.
            }
        }

        // Moves the tetromino down by one row; if it collides, places it on the board.
        public void TranslateTetrominoDown()
        {
            CurrentTetromino.Translate(1, 0); // Move tetromino down by one row.
            if (!TetrominoFits()) // If the tetromino collides after moving,
            {
                CurrentTetromino.Translate(-1, 0); // revert the move,
                PlaceTetromino(); // place the tetromino on the board,
                BlockLanded?.Invoke(this, EventArgs.Empty); // and trigger the BlockLanded event.
            }
        }

        // Computes how many rows a single block can drop before collision.
        private int BlockDropDistance(Position p)
        {
            int drop = 0; // Initialize drop distance.
            while (GameBoard.IsCellClear(p.Row + 1 + drop, p.Column)) // While the cell below is clear,
            {
                drop++; // increment drop distance.
            }
            return drop; // Return the computed drop distance.
        }

        // Computes the maximum drop distance for the entire tetromino by finding the minimal drop distance among its blocks.
        public int TetrominoDropDistance()
        {
            int drop = GameBoard.Rows; // Start with the maximum possible drop distance.
            foreach (Position p in CurrentTetromino.BlockPositions()) // For each block,
            {
                drop = Math.Min(drop, BlockDropDistance(p)); // update drop to the smallest drop distance.
            }
            return drop; // Return the minimal drop distance.
        }

        // Instantly drops the tetromino to the lowest valid position.
        public void DropTetromino()
        {
            CurrentTetromino.Translate(TetrominoDropDistance(), 0); // Move tetromino down by the computed drop distance.
            PlaceTetromino(); // Place the tetromino on the board.
            BlockLanded?.Invoke(this, EventArgs.Empty); // Trigger the BlockLanded event.
        }
    }
}
