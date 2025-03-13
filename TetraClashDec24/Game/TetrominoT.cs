namespace TetraClashDec24
{
    // The TetrominoT class represents the "T" shaped tetromino in a Tetris-like game.
    // It inherits from the base Tetromino class, which defines common functionality for all tetromino types.
    public class TetrominoT : Tetromino
    {

        private readonly Position[][] blocks = new Position[][]
        {
            // Rotation state 0: the default orientation of the T tetromino.
            new Position[]
            {
                new (0, 1),
                new (1, 0),  
                new (1, 1),  
                new (1, 2)  
            },
            // Rotation state 1: rotated 90 degrees clockwise.
            new Position[]
            {
                new (0, 1),
                new (1, 1),
                new (1, 2),
                new (2, 1)
            },
            // Rotation state 2: rotated 180 degrees from default.
            new Position[]
            {
                new (1, 0),
                new (1, 1),
                new (1, 2),
                new (2, 1)
            },
            // Rotation state 3: rotated 270 degrees clockwise.
            new Position[]
            {
                new (0, 1),
                new (1, 0),
                new (1, 1),
                new (2, 1)
            }
        };

        // TetrominoID uniquely identifies this tetromino type.
        public override int TetrominoID => 6;

        // The StartingPos property sets the initial position of the tetromino when it spawns on the game board.
        protected override Position StartingPos => new Position(0, 3);

        protected override Position[][] Blocks => blocks;
    }
}