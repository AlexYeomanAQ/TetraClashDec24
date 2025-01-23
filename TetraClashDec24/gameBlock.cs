using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace TetraClashDec24
{
    public class Block
    {
        private Vector2 initPosOffset = new Vector2(9, 0);
        private Tetromino tetromino;
        private Vector2 pos;

        public Block(Tetromino t, Vector2 p)
        {
            tetromino = t;
            pos = p;
        }

        public Vector2 rotate(Vector2 pivotPos)
        {
            Vector2 translated = pos - pivotPos;
            double radians = Math.PI * 90 / 180;
            float cos = (float) Math.Round(Math.Cos(radians), 2);
            float sin = (float) Math.Round(Math.Sin(radians), 2);

            return new Vector2(
                translated.X * cos - translated.Y * sin,
                translated.X * sin + translated.Y * cos
            );
        }

        public bool isCollide(Vector2 pos)
        {
            if (0 <= pos.X && pos.X < 10 && pos.Y < 20 && pos.Y < 0) // || !tetromino.tetris.fieldArray[pos.Y, pos.X]
            {
                return false;
            }
            return true;
        }
    }
}
