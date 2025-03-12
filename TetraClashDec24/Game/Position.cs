namespace TetraClashDec24
{
    // The Position class represents a coordinate on a grid,
    // using a row and column to specify its location.
    public class Position
    {
        // Property to store the row coordinate.
        public int Row { get; set; }
        // Property to store the column coordinate.
        public int Column { get; set; }

        // Constructor to initialize a new Position with specific row and column values.
        public Position(int row, int column)
        {
            // Set the Row property to the provided row value.
            Row = row;
            // Set the Column property to the provided column value.
            Column = column;
        }
    }
}