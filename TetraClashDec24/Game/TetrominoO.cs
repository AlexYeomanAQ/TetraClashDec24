namespace TetraClashDec24
{
    public class TetrominoO : Tetromino
    {
        private readonly Position[][] blocks = new Position[][]
        {
            new Position[] { new(0,0), new(0,1), new(1,0), new(1,1) }
        };

        public override int TetrominoID => 4;
        protected override Position StartingPos => new Position(0, 4);
        protected override Position[][] Blocks => blocks;
    }
}
