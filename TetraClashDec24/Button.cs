using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TetraClashDec24
{
    class Button
    {
        public App App;
        public Rectangle Box;
        public Color Colour;
        public string Text;

        public Button(App app, int x, int y, int width, int height, Color colour, string text = "")
        {
            App = app;
            Box = new Rectangle(x, y, width, height);
            Colour = colour;
            Text = text;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(App.baseTexture, Box, Colour);

            if (Text != "")
            {
                Vector2 textSize = App.font.MeasureString(Text);
                float textX = Box.X + (Box.Width / 2) - (textSize.X / 2);
                float textY = Box.Y + (Box.Height / 2) - (textSize.Y / 2);
                spriteBatch.DrawString(App.font, Text, new Vector2(textX, textY), Color.Black);
            }
        }
    }

    class InputButton : Button
    {
        public bool highlighted = false;

        public InputButton(App app, int x, int y, int width, int height, Color colour, string text = "") : base(app, x, y, width, height, Color.White)
        {
            App = app;
            Box = new Rectangle(x, y, width, height);
            Colour = colour;
            Text = text;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (highlighted)
                spriteBatch.Draw(App.baseTexture, Box, Color.LightBlue);
            else
                spriteBatch.Draw(App.baseTexture, Box, Color.White);

            if (Text != "" && Text != null)
            {
                Vector2 textSize = App.font.MeasureString(Text);
                float textX = Box.X + (Box.Width / 2) - (textSize.X / 2);
                float textY = Box.Y + (Box.Height / 2) - (textSize.Y / 2);
                spriteBatch.DrawString(App.font, Text, new Vector2(textX, textY), Color.Black);
            }
        }
    }
}
