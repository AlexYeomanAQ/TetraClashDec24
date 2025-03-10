using System.Collections.Generic;

namespace TetraClashDec24
{
    public abstract class Tetromino
    {
        protected abstract Position[][] Blocks { get; }
        protected abstract Position StartingPos { get; }
        public abstract int TetrominoID { get; }

        private int rotationIndex;
        private Position placement;

        public Tetromino()
        {
            placement = new Position(StartingPos.Row, StartingPos.Column);
        }

        public IEnumerable<Position> BlockPositions(bool ignore_offset = false)
        {
            foreach (Position p in Blocks[ignore_offset ? 0 : rotationIndex])
            {
                if (ignore_offset)
                {
                    yield return new Position(p.Row, p.Column);
                }
                else
                {
                    yield return new Position(p.Row + placement.Row, p.Column + placement.Column);
                }
            }
        }

        public void TurnClockwise()
        {
            rotationIndex = (rotationIndex + 1) % Blocks.Length;
        }

        public void TurnAntiClockwise()
        {
            if (rotationIndex == 0)
            {
                rotationIndex = Blocks.Length - 1;
            }
            else
            {
                rotationIndex--;
            }
        }

        public void Translate(int rows, int columns)
        {
            placement.Row += rows;
            placement.Column += columns;
        }

        public void Reset()
        {
            rotationIndex = 0;
            placement.Row = StartingPos.Row;
            placement.Column = StartingPos.Column;
        }
    }
}
