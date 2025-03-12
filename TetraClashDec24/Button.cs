using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace TetraClashDec24
{
    // This class represents a clickable UI button in the game.
    // It can display text, change appearance when highlighted, and play a sound when interacted with.
    class Button
    {
        // Reference to the main game instance, used to access shared resources such as textures, fonts, and sound effects.
        public App App;
        // The color used to tint the button when drawn.
        public Color Colour;
        // The rectangular area that defines the button's position and size on the screen.
        public Rectangle Box;
        // The text label displayed on the button (optional).
        public string Text;
        // Indicates whether the button is highlighted (e.g., when the mouse hovers over it).
        public bool Highlighted = false;

        // Constructor to initialize the button with its properties.
        // 'app' provides access to shared game resources.
        // 'x', 'y' set the button's position; 'width' and 'height' set its size.
        // 'colour' determines the button's tint, and 'text' is an optional label.
        public Button(App app, int x, int y, int width, int height, Color colour, string text = "")
        {
            App = app;
            Box = new Rectangle(x, y, width, height);
            Colour = colour;
            Text = text;
        }

        // Draws the button using the provided SpriteBatch.
        // The appearance changes based on whether the button is highlighted.
        public void Draw(SpriteBatch spriteBatch)
        {
            // If the button is highlighted, draw using the highlight texture.
            if (Highlighted)
            {
                spriteBatch.Draw(App.highlightTexture, Box, Colour);
            }
            // Otherwise, draw using the regular button texture.
            else
            {
                spriteBatch.Draw(App.buttonTexture, Box, Colour);
            }

            // If the button has text, calculate its centered position and draw it.
            if (Text != "")
            {
                // Measure the size of the text to center it within the button.
                Vector2 textSize = App.font.MeasureString(Text);
                // Calculate horizontal centering.
                float textX = Box.X + (Box.Width / 2) - (textSize.X / 2);
                // Calculate vertical centering.
                float textY = Box.Y + (Box.Height / 2) - (textSize.Y / 2);
                // Draw the text using the game's font in black color.
                spriteBatch.DrawString(App.font, Text, new Vector2(textX, textY), Color.Black);
            }
        }

        // Plays the button click sound effect.
        // The sound is played at a low volume (0.1) with no pitch or pan alterations.
        public void PlaySound()
        {
            App.sound_ButtonClick.Play(0.1f, 0, 0);
        }
    }
}
