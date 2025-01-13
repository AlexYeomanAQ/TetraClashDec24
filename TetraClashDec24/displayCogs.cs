using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    public class displayCogs
    {
        int defaultWidth = 1920;
        int defaultHeight = 1080;

        GraphicsDevice _graphicsDevice;
        public displayCogs(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public Vector2 vectorPosCalculation(Vector2 vector)
        {
            int currentWidth = _graphicsDevice.Viewport.Width;
            int currentHeight = _graphicsDevice.Viewport.Width;
            float sf_X = currentWidth / defaultWidth;
            float sf_Y = currentHeight / defaultHeight;
            return new Vector2((int)vector.X * sf_X, (int)vector.Y * sf_Y);
        }

    }
}
