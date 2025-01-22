using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace TetraClashDec24
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private GameState _currentState;

        private Texture2D _background;

        public string Username;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            string cachePath = "cache.txt";
            MouseState mouse = Mouse.GetState();

            if (File.Exists(cachePath))
            {
                string username = File.ReadAllText(cachePath);
                //_currentState = new LoginState(this, mouse.LeftButton, username);
                _currentState = new CreateAccountState(this, mouse.LeftButton);
            }
            else
            {
                _currentState = new CreateAccountState(this, mouse.LeftButton);
            }
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            _background = Content.Load<Texture2D>(@"background");
            _currentState.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            _currentState.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(_background, new Rectangle(0, 0, 1920, 1080), Color.White);
            spriteBatch.End();
            _currentState.Draw(gameTime);
            base.Draw(gameTime);
        }

        public void ChangeState(GameState newState)
        {
            _currentState = newState;
            _currentState.LoadContent();
        }
    }
}