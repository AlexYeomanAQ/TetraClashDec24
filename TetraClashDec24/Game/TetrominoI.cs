namespace TetraClashDec24
{
    // The TetrominoI class represents the "I" shaped tetromino piece,
    // inheriting from the abstract Tetromino base class.
    public class TetrominoI : Tetromino
    {
        // Define the block configurations for the "I" tetromino in its 4 rotation states.
        // Each inner array represents the relative positions of the 4 blocks in a specific rotation.
        private readonly Position[][] blocks = new Position[][]
        {
            // First rotation: horizontal line
            new Position[] { new (1, 0), new (1, 1), new (1, 2), new (1, 3) },
            // Second rotation: vertical line (rotated 90 degrees clockwise)
            new Position[] { new (0, 2), new (1, 2), new (2, 2), new (3, 2) },
            // Third rotation: horizontal line (flipped version of first rotation)
            new Position[] { new (2, 0), new (2, 1), new (2, 2), new (2, 3) },
            // Fourth rotation: vertical line (flipped version of second rotation)
            new Position[] { new (0, 1), new (1, 1), new (2, 1), new (3, 1) },
        };

        // Unique identifier for the "I" tetromino. This can be used to differentiate it from other shapes.
        public override int TetrominoID => 1;

        // The starting position of the tetromino on the grid.
        // Here, the "I" tetromino is positioned off-screen initially (row -1) with a column offset of 3.
        protected override Position StartingPos => new Position(-1, 3);

        // Provides the block configurations for the current tetromino.
        // This property returns the blocks array defined above.
        protected override Position[][] Blocks => blocks;
    }
}
