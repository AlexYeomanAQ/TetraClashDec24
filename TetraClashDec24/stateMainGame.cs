using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using TetraClashDec24;
using Microsoft.Xna.Framework.Graphics;

public class MainGameState : GameState
{

    private ButtonState prevClickState;
    public MainGameState (Game1 game, ButtonState clickState) : base(game)
    {
        prevClickState = clickState;
    }

    public override void LoadContent()
    {

    }

    public override void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();

        if (mouse.LeftButton == ButtonState.Pressed)
        {
            Point mousePosition = new Point(mouse.X, mouse.Y);
            
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.CornflowerBlue);
        SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        spriteBatch.Begin();

        spriteBatch.End();
    }
}
