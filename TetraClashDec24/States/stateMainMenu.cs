using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TetraClashDec24
{
    public class MainMenuState : AppState
    {
        Button PlayButton;

        private ButtonState prevClickState;
        public MainMenuState(App game, ButtonState clickState) : base(game)
        {
            prevClickState = clickState;
        }

        public override void LoadContent()
        {
            PlayButton = new Button(App, 880, 340, 200, 200, Color.White, "Play");
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
            {
                Point mousePosition = new Point(mouse.X, mouse.Y);
                if (PlayButton.Box.Contains(mousePosition))
                {
                    App.ChangeState(new SearchState(App, mouse.LeftButton));
                }
            }
            prevClickState = mouse.LeftButton;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(App.GraphicsDevice);
            spriteBatch.Begin();
            PlayButton.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}