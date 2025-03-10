using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Net.Sockets;

namespace TetraClashDec24
{
    public class App : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private AppState _currentState;

        public SpriteFont font;
        public SpriteFont titleFont;
        public Texture2D baseTexture;
        public Texture2D buttonTexture;
        public Texture2D highlightTexture;
        private Texture2D backgroundTexture;

        public TcpClient _client = null;
        public NetworkStream _stream = null;

        public string Username;
        public int Rating;

        public string CachePath = "cache.txt";
        public App()
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

            MouseState mouse = Mouse.GetState();
            if (File.Exists(CachePath))
            {
                string[] lines = File.ReadAllLines(CachePath);
                Username = lines[0];
                _currentState = new LoginState(this, mouse.LeftButton, Username);
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
            font = Content.Load<SpriteFont>(@"myFont");
            titleFont = Content.Load<SpriteFont>(@"titleFont");
            baseTexture = Content.Load<Texture2D>(@"base");
            buttonTexture = Content.Load<Texture2D>(@"button");
            highlightTexture = Content.Load<Texture2D>(@"highlight");
            backgroundTexture = Content.Load<Texture2D>(@"background");
            _currentState.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _currentState.Update(gameTime);

            if (_client != null)
            {
                if (!_client.Connected)
                {
                    Console.WriteLine("Client Disconnected, client i s not null");
                    string error = "Disconnected from Server, please log in again.";
                    if (_currentState is not LoginState)
                    {
                        ChangeState(new LoginState(this, ButtonState.Pressed, Username, error));
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 1920, 1080), Color.White);
            spriteBatch.End();
            _currentState.Draw(gameTime);
            base.Draw(gameTime);
        }

        public void ChangeState(AppState newState)
        {
            _currentState = newState;
            _currentState.LoadContent();
        }
    }
}