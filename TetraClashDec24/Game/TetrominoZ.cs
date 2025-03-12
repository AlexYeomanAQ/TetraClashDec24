namespace TetraClashDec24
{
    // The TetrominoZ class represents the "Z" shaped tetromino in a Tetris-like game.
    // It inherits from the base Tetromino class, which defines common functionality for all tetromino types.
    public class TetrominoZ : Tetromino
    {
        private readonly Position[][] blocks = new Position[][]
        {
            // Rotation state 0: the default orientation of the Z tetromino.
            new Position[]
            {
                new (0, 0),
                new (0, 1),
                new (1, 1),
                new (1, 2)
            },
            // Rotation state 1: rotated 90 degrees clockwise.
            new Position[]
            {
                new (0, 2),
                new (1, 1),
                new (1, 2),
                new (2, 1)
            }
        };

        // TetrominoID uniquely identifies this tetromino type.
        public override int TetrominoID => 7;

        // The StartingPos property sets the initial position of the tetromino when it spawns on the game board.
        protected override Position StartingPos => new Position(0, 3);

        // Blocks property defines the shape of the tetromino in different rotation states.
        protected override Position[][] Blocks => blocks;
    }
}