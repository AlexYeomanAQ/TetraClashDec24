using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace TetraClashDec24
{
    class Button
    {
        public App App;
        public Color Colour;
        public Rectangle Box;
        public string Text;
        public bool Highlighted = false;

        public Button(App app, int x, int y, int width, int height, Color colour, string text = "")
        {
            App = app;
            Box = new Rectangle(x, y, width, height);
            Colour = colour;
            Text = text;
        }

        public void LoadContent()
        {
            
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (Highlighted)
            {
                spriteBatch.Draw(App.highlightTexture, Box, Colour);
            }
            else
            {
                spriteBatch.Draw(App.buttonTexture, Box, Colour);
            }

            if (Text != "")
            {
                Vector2 textSize = App.font.MeasureString(Text);
                float textX = Box.X + (Box.Width / 2) - (textSize.X / 2);
                float textY = Box.Y + (Box.Height / 2) - (textSize.Y / 2);
                spriteBatch.DrawString(App.font, Text, new Vector2(textX, textY), Color.Black);
            }
        }

        public void PlaySound()
        {
            App.sound_ButtonClick.Play();
        }
    }
}
