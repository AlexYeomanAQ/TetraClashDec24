using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System;

namespace TetraClashDec24
{
    class Button : GameObject
    {
        public Rectangle Box;
        public string TexturePath;
        public Color Colour;
        public string Text;

        public Button(string filePath, int x, int y, int width, int height, Color colour, string text = "")
        {
            Box = new Rectangle(x, y, width, height);
            TexturePath = filePath;
            Colour = colour;
            Text = text;

        }

        public override void LoadContent(ContentManager Content)
        {
            Texture = Content.Load<Texture2D>(@"base");
            GameFont = Content.Load<SpriteFont>(@"myFont");
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Box, Colour);

            if (Text != "")
            {
                Vector2 textSize = GameFont.MeasureString(Text);
                float textX = Box.X + (Box.Width / 2) - (textSize.X / 2);
                float textY = Box.Y + (Box.Height / 2) - (textSize.Y / 2);
                spriteBatch.DrawString(GameFont, Text, new Vector2(textX, textY), Color.Black);
            }
        }
    }

    class InputButton : Button
    {
        public bool highlighted = false;

        public InputButton(string filePath, int x, int y, int width, int height, Color colour, string text = "") : base(filePath, x, y, width, height, Color.White)
        {
            Box = new Rectangle(x, y, width, height);
            TexturePath = filePath;
            Colour = colour;
            Text = text;
        }

        public override void LoadContent(ContentManager Content)
        {
            Texture = Content.Load<Texture2D>(TexturePath);
            GameFont = Content.Load<SpriteFont>(@"myFont");
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (highlighted)
                spriteBatch.Draw(Texture, Box, Color.LightBlue);
            else
                spriteBatch.Draw(Texture, Box, Color.White);

            if (Text != "")
            {
                Vector2 textSize = GameFont.MeasureString(Text);
                float textX = Box.X + (Box.Width / 2) - (textSize.X / 2);
                float textY = Box.Y + (Box.Height / 2) - (textSize.Y / 2);
                spriteBatch.DrawString(GameFont, Text, new Vector2(textX, textY), Color.Black);
            }
        }
    }
}
