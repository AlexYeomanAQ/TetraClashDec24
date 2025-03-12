namespace TetraClashDec24
{
    // The TetrominoL class represents the "L" shaped tetromino piece.
    // It inherits from the abstract Tetromino base class and provides its specific
    // block configuration for each rotation, a unique identifier, and a starting position.
    public class TetrominoL : Tetromino
    {
        // Define the block configurations for the "L" tetromino in its 4 rotation states.
        // Each inner array holds relative positions for the blocks of the tetromino.
        // These positions define the shape of the tetromino when rotated.
        private readonly Position[][] blocks = new Position[][]
        {
            // Rotation 0:
            // The piece has its long side horizontal with an extra block on the left.
            new Position[] { new (0, 2), new (1, 0), new (1, 1), new (1, 2) },
            
            // Rotation 1:
            // The piece is rotated 90 degrees clockwise.
            // It forms a vertical line with an extra block on the bottom right.
            new Position[] { new (0, 1), new (1, 1), new (2, 1), new (2, 2) },
            
            // Rotation 2:
            // The piece is rotated 180 degrees.
            // It forms a horizontal line with an extra block on the right.
            new Position[] { new (1, 0), new (1, 1), new (1, 2), new (2, 0) },
            
            // Rotation 3:
            // The piece is rotated 270 degrees clockwise.
            // It forms a vertical line with an extra block on the top left.
            new Position[] { new (0, 0), new (0, 1), new (1, 1), new (2, 1) },
        };

        // Unique identifier for the "L" tetromino.
        // This ID can be used to differentiate between different tetromino types.
        public override int TetrominoID => 3;

        // The starting position of the tetromino on the game grid.
        // This determines where the tetromino appears when it is first created.
        protected override Position StartingPos => new Position(0, 3);

        // Provides the block configurations (all rotations) to the base Tetromino class.
        protected override Position[][] Blocks => blocks;
    }
}
