using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using TetraClashDec24;
using Microsoft.Xna.Framework.Graphics;

public class MainGameState : AppState
{
    Button PlayButton;
    private ButtonState prevClickState;

    private App1 _app;

    private float _timer = 0f;
    private float _slowBlockUpdate = 0.4f;
    private float _fastBlockUpdate = 0.2f;

    private gameMechanics _tetris;

    private Texture2D[] _blockImages = new Texture2D[7];
    public MainGameState(App1 app, ButtonState clickState) : base(app)
    {
        prevClickState = clickState;

        _app = app;

        _tetris = new gameMechanics(_app);
    }

    public override void LoadContent()
    {
        for (int i = 0; i < _blockImages.Length; i++)
        {
            _blockImages[i] = Game.Content.Load<Texture2D>(@"base"); //change to i when you have textures
        }

        PlayButton = new Button(@"base", 880, 340, 200, 200, Color.White, "skibidi toilet");
        PlayButton.LoadContent(Game.Content);
    }

    public override void Update(GameTime gameTime)
    {

        MouseState mouse = Mouse.GetState();

        if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
        {
            Point mousePosition = new Point(mouse.X, mouse.Y);
        }
        prevClickState = mouse.LeftButton;
    }

    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        spriteBatch.Begin();
        PlayButton.Draw(spriteBatch);
        spriteBatch.End();
    }
}
