namespace TetraClashDec24
{
    // This class represents the "S" shaped tetromino.
    // It inherits from the base Tetromino class which provides common properties and functionality
    public class TetrominoS : Tetromino
    {
        // 'blocks' stores the relative positions of each block in the tetromino for its different rotation states.
        // Each inner array represents a distinct rotation state of the tetromino.
        // The coordinates are offsets from a reference point, typically the tetromino's starting position.
        private readonly Position[][] blocks = new Position[][]
        {
            // Rotation state 0 (default orientation):
            // The S-shaped tetromino is formed by positioning the blocks relative to the reference point.
            new Position[]
            {
                new (0, 1),
                new (0, 2),
                new (1, 0),
                new (1, 1)
            },
            // Rotation state 1 (rotated orientation):
            // The blocks are rearranged to represent the rotated form of the S tetromino.
            new Position[]
            {
                new (0, 1),
                new (1, 1),
                new (1, 2),
                new (2, 2) 
            }
        };

        // TetrominoID uniquely identifies this tetromino type.
        public override int TetrominoID => 5;

        // Defines the starting position for the tetromino when it spawns on the game board.
        protected override Position StartingPos => new Position(0, 3);

        // This property returns the array of rotation states (each a set of block positions).
        protected override Position[][] Blocks => blocks;
    }
}
