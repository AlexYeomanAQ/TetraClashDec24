using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TetraClashDec24
{
    public class SettingsState : AppState
    {
        Button TestButton;

        private ButtonState prevClickState;
        public SettingsState(App game, ButtonState clickState) : base(game)
        {
            prevClickState = clickState;
        }

        public override void LoadContent()
        {
            TestButton = new Button(App, 500, 500, 500, 500, Color.White, "skibidi!");
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed && mouse.LeftButton != prevClickState)
            {
                Point mousePosition = new Point(mouse.X, mouse.Y);
                if (TestButton.Box.Contains(mousePosition))
                {
                    App.ChangeState(new MainMenuState(App, mouse.LeftButton));
                }
            }
            prevClickState = mouse.LeftButton;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(App.GraphicsDevice);
            spriteBatch.Begin();
            TestButton.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}