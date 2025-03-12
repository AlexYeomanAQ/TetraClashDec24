namespace TetraClashDec24
{
    // The TetrominoO class represents the "O" tetromino piece,
    // which is a square shape. It inherits from the abstract Tetromino class
    // and provides the specific block configuration, unique ID, and starting position.
    public class TetrominoO : Tetromino
    {
        // Define the block configuration for the "O" tetromino.
        // Since the square shape doesn't change with rotation,
        // only one configuration is needed.
        private readonly Position[][] blocks = new Position[][]
        {
            // The blocks array contains one rotation state for the O tetromino.
            // The positions are defined relative to a local origin.
            new Position[] { new(0, 0), new(0, 1), new(1, 0), new(1, 1) }
        };

        // Unique identifier for the "O" tetromino.
        // This ID distinguishes it from other tetromino types.
        public override int TetrominoID => 4;

        // The starting position on the game grid for the "O" tetromino.
        // It determines where the piece appears when first spawned.
        protected override Position StartingPos => new Position(0, 4);

        // Expose the block configurations to the base Tetromino class.
        // Since the "O" tetromino does not rotate, only one configuration is used.
        protected override Position[][] Blocks => blocks;
    }
}
