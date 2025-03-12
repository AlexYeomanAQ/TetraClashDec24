using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    // The Cogs class serves as a utility container that holds various static data and helper methods 
    // used throughout the game. This includes tetromino definitions, scoring values, text centering, 
    // drop rate calculation, and caching functionality.
    public class Cogs
    {
        // Static array of tetromino block configurations.
        // Each inner array represents the relative positions (using Vector2) of blocks that form a tetromino.
        // The shapes defined are: T, O, J, L, I, S, and Z.
        public static readonly Vector2[][] TETROMINOBLOCKS = new Vector2[][]
        {
            new [] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1) }, // T
            new [] { new Vector2(0, 0), new Vector2(0, -1), new Vector2(1, 0), new Vector2(1, -1) }, // O
            new [] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(0, -2) }, // J
            new [] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, -2) }, // L
            new [] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, -1), new Vector2(0, -2) }, // I
            new [] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, -1) }, // S
            new [] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, -1) }  // Z
        };

        // Array of points awarded for clearing a given number of lines.
        // The index corresponds to the number of lines cleared (e.g., 0 for no lines, 1 for one line, etc.).
        public static int[] lineClearPoints = { 0, 40, 100, 300, 1200 };

        // Calculates a centered position for a given text string.
        // Parameters:
        // - font: The SpriteFont used to measure the text.
        // - text: The text to be centered.
        // - x, y: The coordinates around which the text should be centered.
        // Returns a Vector2 representing the top-left position for drawing the text so that it is centered.
        public static Vector2 centreTextPos(SpriteFont font, string text, int x, int y)
        {
            // Measure the size of the text.
            Vector2 textSize = font.MeasureString(text);
            // Calculate the X coordinate for centered text.
            float textX = x - (textSize.X / 2);
            // Calculate the Y coordinate for centered text.
            float textY = y - (textSize.Y / 2);
            // Return the calculated position (rounded to an integer for pixel alignment).
            return new Vector2((int)textX, (int)textY);
        }

        // Determines the drop rate (in milliseconds) for tetrominoes based on the current game level.
        // The drop rate is calculated using a switch expression to convert a level to a specific number of frames.
        // It then converts frames to milliseconds, assuming a 60 frames per second (FPS) rate.
        public static int getDropRate(int level)
        {
            // Determine the number of frames based on the level.
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
                _ => 1, // For level 29 and above.
            };

            // Each frame is approximately 1000/60 milliseconds at 60 FPS.
            int millisecondsPerFrame = (int)(1000 / 60);
            // Return the drop rate in milliseconds.
            return frames * millisecondsPerFrame;
        }

        // Asynchronously saves a cache file with the user's username and salt.
        // This method writes the username and salt to a file called "cache.txt", separated by a newline.
        public static async Task saveCache(string username, string salt)
        {
            await File.WriteAllTextAsync("cache.txt", $"{username}\n{salt}");
        }
    }
}
