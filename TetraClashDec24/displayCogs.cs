using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    public static class displayCogs
    {
        public static Vector2 centreTextPos(SpriteFont font, string text, int x, int y)
        {
            Vector2 textSize = font.MeasureString(text);
            float textX = x - (textSize.X / 2);
            float textY = y - (textSize.Y / 2);
            return new Vector2((int)textX, (int)textY);
        }

    }
}
