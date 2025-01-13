using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using TetraClashDec24;
using Microsoft.Xna.Framework.Graphics;

namespace TetraClashDec24
{
    class SearchState : GameState
    {
        private SpriteFont SearchFont;
        private Button CancelButton;

        private ButtonState prevClickState;
        public SearchState(Game1 game, ButtonState clickState) : base(game)
        {
            prevClickState = clickState;
        }

        public override void LoadContent()
        {
            CancelButton = new Button(@"base", 860, 710, 200, 100, Color.White, "Cancel");
            CancelButton.LoadContent(Game.Content);

            SearchFont = Game.Content.Load<SpriteFont>(@"myFont");
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
            {
                Point mousePosition = new Point(mouse.X, mouse.Y);
                if (CancelButton.Box.Contains(mousePosition))
                {
                    Game.ChangeState(new MainMenuState(Game, mouse.LeftButton));
                }
            }
            prevClickState = mouse.LeftButton;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            spriteBatch.Begin();
            Vector2 textSize = SearchFont.MeasureString("Searching");
            float textX = (1920 / 2) - (textSize.X / 2);
            float textY = (1080 / 2) - (textSize.Y / 2);
            spriteBatch.DrawString(SearchFont, "Searching", new Vector2(textX, textY), Color.Black);
            CancelButton.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
