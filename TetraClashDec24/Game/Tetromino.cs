using System.Collections.Generic;

namespace TetraClashDec24
{
    // Abstract Tetromino class that defines the basic behavior for all tetromino pieces.
    // Each concrete tetromino shape must provide its own block configurations and starting position.
    public abstract class Tetromino
    {
        // Abstract property for tetromino block configurations.
        // where each sub-array represents the block positions for a specific rotation.
        protected abstract Position[][] Blocks { get; }

        // Abstract property for the starting position of the tetromino on the game grid.
        protected abstract Position StartingPos { get; }

        // Abstract property to uniquely identify the type of tetromino (e.g., T, O, L, etc.).
        public abstract int TetrominoID { get; }

        // Internal index to keep track of the current rotation.
        private int rotationIndex;
        // The current placement (position) of the tetromino on the game grid.
        private Position placement;

        // Constructor initializes the tetromino's placement using the starting position.
        public Tetromino()
        {
            // Create a new Position instance based on the defined starting position.
            placement = new Position(StartingPos.Row, StartingPos.Column);
        }

        // Returns the positions of the blocks that make up the tetromino.
        // If ignore_offset is true, the positions are returned without the current placement offset.
        // Otherwise, the current placement (translation) is added to each block position.
        public IEnumerable<Position> BlockPositions(bool ignore_offset = false)
        {
            // Choose the correct rotation configuration based on ignore_offset flag.
            foreach (Position p in Blocks[ignore_offset ? 0 : rotationIndex])
            {
                if (ignore_offset)
                {
                    // Return the block's relative position.
                    yield return new Position(p.Row, p.Column);
                }
                else
                {
                    // Return the block's position adjusted by the current placement on the grid.
                    yield return new Position(p.Row + placement.Row, p.Column + placement.Column);
                }
            }
        }

        // Rotates the tetromino clockwise by advancing the rotation index.
        public void TurnClockwise()
        {
            // Cycle through the available rotations using modulo arithmetic.
            rotationIndex = (rotationIndex + 1) % Blocks.Length;
        }

        // Rotates the tetromino anti-clockwise by decrementing the rotation index.
        public void TurnAntiClockwise()
        {
            // If at the initial rotation, wrap around to the last rotation.
            if (rotationIndex == 0)
            {
                rotationIndex = Blocks.Length - 1;
            }
            else
            {
                // Otherwise, simply decrement the rotation index.
                rotationIndex--;
            }
        }

        // Moves the tetromino by a specified number of rows and columns.
        public void Translate(int rows, int columns)
        {
            placement.Row += rows;
            placement.Column += columns;
        }

        // Resets the tetromino to its initial rotation and starting position.
        public void Reset()
        {
            rotationIndex = 0;
            placement.Row = StartingPos.Row;
            placement.Column = StartingPos.Column;
        }
    }
}
