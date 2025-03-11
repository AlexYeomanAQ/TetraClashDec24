namespace TetraClashDec24
{
    public class TetrominoQueue
    {
        public readonly Tetromino[] tetrominos = new Tetromino[]
        {
            new TetrominoI(),
            new TetrominoJ(),
            new TetrominoL(),
            new TetrominoO(),
            new TetrominoS(),
            new TetrominoT(),
            new TetrominoZ(),
        };

        private LCGGenerator LCGGen;

        public Tetromino NextTetromino { get; private set; }

        public TetrominoQueue(int seed)
        {
            LCGGen = new LCGGenerator(seed);
            NextTetromino = RandomTetromino();
        }

        public Tetromino RandomTetromino()
        {
            return tetrominos[LCGGen.NextTetrominoValue()];
        }

        public Tetromino FetchAndRefresh()
        {
            Tetromino tetromino = NextTetromino;

            do
            {
                NextTetromino = RandomTetromino();
            }
            while (tetromino.TetrominoID == NextTetromino.TetrominoID);

            return tetromino;
        }
    }
}
