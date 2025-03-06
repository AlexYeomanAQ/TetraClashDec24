namespace TetraClashDec24
{
    public class TetrominoT : Tetromino
    {
        private readonly Position[][] blocks = new Position[][]
        {
            new Position[] { new (0,1), new (1,0), new (1,1), new (1,2) },
            new Position[] { new (0,1), new (1,1), new (1,2), new (2,1) },
            new Position[] { new (1,0), new (1,1), new (1,2), new (2,1) },
            new Position[] { new (0,1), new (1,0), new (1,1), new (2,1) },
        };

        public override int TetrominoID => 6;

        protected override Position StartingPos => new Position(0, 3);

        protected override Position[][] Blocks => blocks;
    }
}
