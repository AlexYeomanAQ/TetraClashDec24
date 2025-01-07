using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using TetraClashDec24;
using Microsoft.Xna.Framework.Graphics;

public class MainMenuState : GameState
{
    Button PlayButton;

    private ButtonState prevClickState;
    public MainMenuState(Game1 game, ButtonState clickState) : base(game)
    {
        prevClickState = clickState;
    }

    public override void LoadContent()
    {
        PlayButton = new Button(@"base", 500, 500, 300, 300, Color.White, "Play");
        PlayButton.LoadContent(Game.Content);
    }

    public override void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();

        if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
        {
            Point mousePosition = new Point(mouse.X, mouse.Y);
            if (PlayButton.Box.Contains(mousePosition))
            {
                Game.ChangeState(new SearchState(Game, mouse.LeftButton));
            }
        }
        prevClickState = mouse.LeftButton;
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.CornflowerBlue);
        SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        spriteBatch.Begin();
        PlayButton.Draw(spriteBatch);
        spriteBatch.End();
    }
}
