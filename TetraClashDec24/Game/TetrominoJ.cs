namespace TetraClashDec24
{
    // The TetrominoJ class represents the "J" shaped tetromino piece.
    // It inherits from the abstract Tetromino base class and provides
    // its specific block configurations, unique ID, and starting position.
    public class TetrominoJ : Tetromino
    {
        // Define the block configurations for the "J" tetromino in its 4 rotation states.
        // Each inner array contains relative positions (using the Position class) of the tetromino's blocks.
        private readonly Position[][] blocks = new Position[][]
        {
            // Rotation 0:
            // Shape: a vertical bar with an extra block attached on the right at the bottom.
            new Position[] { new (0, 0), new (1, 0), new (1, 1), new (1, 2) },

            // Rotation 1:
            // Shape: a horizontal bar with an extra block attached above the center.
            new Position[] { new (0, 1), new (0, 2), new (1, 1), new (2, 1) },

            // Rotation 2:
            // Shape: a vertical bar with an extra block attached on the left at the top.
            new Position[] { new (1, 0), new (1, 1), new (1, 2), new (2, 2) },

            // Rotation 3:
            // Shape: a horizontal bar with an extra block attached below the center.
            new Position[] { new (0, 1), new (1, 1), new (2, 0), new (2, 1) },
        };

        // Unique identifier for the "J" tetromino.
        // This is used to distinguish it from other tetromino types.
        public override int TetrominoID => 2;

        // The starting position of the "J" tetromino on the game grid.
        // This position is used to initialize its placement when it appears.
        protected override Position StartingPos => new Position(0, 3);

        // Provide the block configuration to the base Tetromino class.
        // This property returns the array of rotation states defined above.
        protected override Position[][] Blocks => blocks;
    }
}
