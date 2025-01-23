using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace TetraClashDec24
{
    public class gameMechanics
    {
        private Game game;

        private bool speedUp;

        private int[,] fieldArray;
        private int fieldHeight;
        private int fieldWidth;

        public gameMechanics(Game _game)
        {
            game = _game;

            fieldArray = initializeFieldArray();
            fieldHeight = fieldArray.GetLength(0);
            fieldWidth = fieldArray.GetLength(1);

            speedUp = false;
        }

        public List<int> checkFullLines()
        {
            List<int> lines = new List<int>();
            int row = fieldHeight - 1;
            for (int y = fieldHeight - 1; y >= 0; y--) //Using Y and X for appropriate collumn names
            {
                for (int x = 0; x < fieldWidth; x++)
                {
                    fieldArray[row, x] = fieldArray[y, x];

                }

            }
        }

        private int[,] initializeFieldArray()
        {
            int[,] array = new int[20, 10];
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    array[i, j] = 0;
                }
            }
            return array;
        }
    }
}
