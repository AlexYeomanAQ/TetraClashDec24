using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetraClashDec24;

public class SettingsState : GameState
{
    Button TestButton;

    private ButtonState prevClickState;
    public SettingsState(Game1 game, ButtonState clickState) : base(game)
    {
        prevClickState = clickState;
    }

    public override void LoadContent()
    {
        TestButton = new Button(@"base", 500, 500, 500, 500, Color.White, "skibidi!");
        TestButton.LoadContent(Game.Content);
    }

    public override void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();

        if (mouse.LeftButton == ButtonState.Pressed && mouse.LeftButton != prevClickState)
        {
            Point mousePosition = new Point(mouse.X, mouse.Y);
            if (TestButton.Box.Contains(mousePosition))
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
        TestButton.Draw(spriteBatch);
        spriteBatch.End();
    }
}
