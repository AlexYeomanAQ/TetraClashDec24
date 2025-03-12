namespace TetraClashDec24
{
    // The TetrominoQueue class is responsible for managing the sequence of tetromino pieces
    // It uses a pseudo-random number generator to select tetrominos
    // from a predefined list while ensuring that the same piece does not appear consecutively.
    public class TetrominoQueue
    {
        // Array holding one instance of each type of tetromino.
        // These instances represent the different shapes available in the game.
        public readonly Tetromino[] tetrominos = new Tetromino[]
        {
            new TetrominoI(), // I-shaped tetromino
            new TetrominoJ(), // J-shaped tetromino
            new TetrominoL(), // L-shaped tetromino
            new TetrominoO(), // O-shaped tetromino (square)
            new TetrominoS(), // S-shaped tetromino
            new TetrominoT(), // T-shaped tetromino
            new TetrominoZ(), // Z-shaped tetromino
        };

        // An instance of a linear congruential generator (LCG) used for generating pseudo-random numbers.
        // The random numbers determine which tetromino is selected from the tetrominos array.
        private LCGGenerator LCGGen;

        // Property that holds the next tetromino to be played.
        // It is set privately within the class to control how new pieces are fetched.
        public Tetromino NextTetromino { get; private set; }

        // Constructor for TetrominoQueue.
        // It initializes the random number generator with a seed and selects the initial tetromino.
        public TetrominoQueue(int seed)
        {
            // Initialize the LCG with the given seed to ensure reproducible randomness.
            LCGGen = new LCGGenerator(seed);

            // Set the initial next tetromino using the RandomTetromino method.
            NextTetromino = RandomTetromino();
        }

        // Returns a random tetromino from the tetrominos array.
        // This method uses the LCGGenerator to obtain a pseudo-random index into the array.
        public Tetromino RandomTetromino()
        {
            // The NextTetrominoValue method returns an integer used as an index.
            return tetrominos[LCGGen.NextTetrominoValue()];
        }

        // Retrieves the current tetromino to be played and refreshes the NextTetromino property.
        // It ensures that the new tetromino is different from the one that was just fetched,
        // preventing the same tetromino from appearing consecutively.
        public Tetromino FetchAndRefresh()
        {
            // Store the current next tetromino to return it.
            Tetromino tetromino = NextTetromino;

            // Generate a new tetromino until it differs from the current one.
            do
            {
                // Update NextTetromino with a newly selected random tetromino.
                NextTetromino = RandomTetromino();
            }
            // Continue generating a new tetromino if it has the same ID as the one just fetched.
            while (tetromino.TetrominoID == NextTetromino.TetrominoID);

            // Return the tetromino that was originally stored.
            return tetromino;
        }
    }
}
