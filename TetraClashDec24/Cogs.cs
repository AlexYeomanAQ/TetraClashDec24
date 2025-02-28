using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    public class Cogs
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

        public static int[] lineClearPoints = { 0, 40, 100, 300, 1200 };
        public static Vector2 centreTextPos(SpriteFont font, string text, int x, int y)
        {
            Vector2 textSize = font.MeasureString(text);
            float textX = x - (textSize.X / 2);
            float textY = y - (textSize.Y / 2);
            return new Vector2((int)textX, (int)textY);
        }

        public static int getDropRate(int level)
        {
            {
                int frames = level switch
                {
                    0 => 48,
                    1 => 43,
                    2 => 38,
                    3 => 33,
                    4 => 28,
                    5 => 23,
                    6 => 18,
                    7 => 13,
                    8 => 8,
                    9 => 6,
                    >= 10 and <= 12 => 5,
                    >= 13 and <= 15 => 4,
                    >= 16 and <= 18 => 3,
                    >= 19 and <= 28 => 2,
                    _ => 1, // Level 29 and above
                };

                // Convert frames to milliseconds (60 FPS means each frame is ~16.67 ms)
                int millisecondsPerFrame = (int) (1000 / 60);
                return frames * millisecondsPerFrame;
            }
        }
        public static async Task saveCache(string username, string salt)
        {
            await File.WriteAllTextAsync("cache.txt", $"{username}\n{salt}");
        }
    }
}
