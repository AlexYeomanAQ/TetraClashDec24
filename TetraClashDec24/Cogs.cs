using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    public static class Cogs
    {
        public static readonly Vector2[][] TETROMINOBLOCKS = new Vector2[][]
        {
            new [] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1) }, // T
            new [] { new Vector2(0, 0), new Vector2(0, -1), new Vector2(1, 0), new Vector2(1, -1) }, // O
            new [] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(0, -2) }, // J
            new [] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, -2) }, // L
            new [] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, -1), new Vector2(0, -2) }, // I
            new [] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, -1) }, // S
            new [] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, -1) } // Z
        };
        public static Vector2 centreTextPos(SpriteFont font, string text, int x, int y)
        {
            Vector2 textSize = font.MeasureString(text);
            float textX = x - (textSize.X / 2);
            float textY = y - (textSize.Y / 2);
            return new Vector2((int)textX, (int)textY);
        }
    }
}
