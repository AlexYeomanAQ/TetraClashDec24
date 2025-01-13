using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using TetraClashDec24;
using Microsoft.Xna.Framework.Graphics;

public class Template : GameState
{
    Button TestButton;
    public Template(Game1 game) : base(game)
    {

    }

    public override void LoadContent()
    {
        TestButton = new Button(@"base", 100, 100, 500, 500, Color.White, "Template");
        TestButton.LoadContent(Game.Content);
    }

    public override void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();

        if (mouse.LeftButton == ButtonState.Pressed)
        {
            Point mousePosition = new Point(mouse.X, mouse.Y);
            if (TestButton.Box.Contains(mousePosition))
            {
                Game.ChangeState(new SettingsState(Game, mouse.LeftButton));
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        spriteBatch.Begin();
        TestButton.Draw(spriteBatch);
        spriteBatch.End();
    }
}
