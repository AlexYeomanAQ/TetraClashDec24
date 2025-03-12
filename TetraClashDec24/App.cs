using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.IO;
using System.Net.Sockets;

namespace TetraClashDec24
{
    // Main game class inheriting from XNA's Game class.
    public class App : Game
    {
        // Manages the graphics device and settings.
        private readonly GraphicsDeviceManager graphics;
        // Used to batch draw sprites (2D textures).
        private SpriteBatch spriteBatch;

        // Holds the current state of the application (e.g., login, create account, gameplay, etc.).
        private AppState _currentState;

        // Fonts used for rendering text in the game.
        public SpriteFont font;
        public SpriteFont titleFont;
        // Textures for UI elements and background.
        public Texture2D baseTexture;
        public Texture2D buttonTexture;
        public Texture2D highlightTexture;
        private Texture2D backgroundTexture;

        // Audio assets: background music and various sound effects.
        public Song gameMusic;
        public SoundEffect sound_ButtonClick;
        public SoundEffect sound_TetrominoLand;
        public SoundEffect sound_LineClear;
        public SoundEffect sound_Lose;
        public SoundEffect sound_Win;

        // Networking components for server communication.
        public TcpClient _client = null;
        public NetworkStream _stream = null;

        // User account information.
        public string Username;
        public int Rating; // Player ranked rating for matches

        // Path to a local cache file that stores user data.
        public string CachePath = "cache.txt";

        // Constructor: Set up graphics settings and content directory.
        public App()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content"; // The directory where game assets are stored.
            IsMouseVisible = true; // Make the mouse cursor visible.

            // Set the desired screen resolution.
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.ApplyChanges(); // Apply the resolution settings.
        }

        // Called when the game initializes.
        protected override void Initialize()
        {
            // Get the current mouse state (used later for state initialization).
            MouseState mouse = Mouse.GetState();

            // Check if a cache file exists to load a saved username.
            if (File.Exists(CachePath))
            {
                // Read all lines from the cache file.
                string[] lines = File.ReadAllLines(CachePath);
                Username = lines[0]; // Assume the first line is the username.
                // Set the current state to the LoginState with the cached username.
                _currentState = new LoginState(this, mouse.LeftButton, Username);
            }
            else
            {
                // If no cache file exists, start with the account creation state.
                _currentState = new CreateAccountState(this, mouse.LeftButton);
            }
            base.Initialize();
        }

        // Called to load all game content (textures, fonts, sounds, etc.).
        protected override void LoadContent()
        {
            // Create a new SpriteBatch instance for drawing.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load font assets from the Content pipeline.
            font = Content.Load<SpriteFont>(@"myFont");
            titleFont = Content.Load<SpriteFont>(@"titleFont");

            // Load texture assets for various UI elements and background.
            baseTexture = Content.Load<Texture2D>(@"base");
            buttonTexture = Content.Load<Texture2D>(@"button");
            highlightTexture = Content.Load<Texture2D>(@"highlight");
            backgroundTexture = Content.Load<Texture2D>(@"background");

            // Load audio assets including background music and sound effects.
            gameMusic = Content.Load<Song>(@"game");
            sound_ButtonClick = Content.Load<SoundEffect>(@"Select");
            sound_TetrominoLand = Content.Load<SoundEffect>(@"TetrominoLand");
            sound_LineClear = Content.Load<SoundEffect>(@"LineClear");
            sound_Lose = Content.Load<SoundEffect>(@"Lose");
            sound_Win = Content.Load<SoundEffect>(@"Win");

            // Let the current application state load its own specific content.
            _currentState.LoadContent();
        }

        // Called once per frame to update the game logic.
        protected override void Update(GameTime gameTime)
        {
            // Check if the player pressed the Back button on a gamepad or the Escape key on the keyboard.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit(); // Exit the game if so.

            // Update the current state (delegates logic specific to the current screen or mode).
            _currentState.Update(gameTime);

            // Network disconnection handling:
            if (_client != null)
            {
                // Check if the client is no longer connected.
                if (!_client.Connected)
                {
                    Console.WriteLine("Client Disconnected, client is not null");
                    // Define an error message for the user.
                    string error = "Disconnected from Server, please log in again.";
                    // If the current state is not already the LoginState, switch to it with an error message.
                    if (_currentState is not LoginState)
                    {
                        ChangeState(new LoginState(this, ButtonState.Pressed, Username, error));
                    }
                }
            }

            base.Update(gameTime);
        }

        // Called once per frame to render the game.
        protected override void Draw(GameTime gameTime)
        {
            // Clear the screen with a base color.
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Begin drawing using the SpriteBatch.
            spriteBatch.Begin();
            // Draw the background image to fill the entire screen.
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 1920, 1080), Color.White);
            spriteBatch.End();

            // Let the current state handle its own drawing logic.
            _currentState.Draw(gameTime);
            base.Draw(gameTime);
        }

        // Method to change the current game state.
        public void ChangeState(AppState newState)
        {
            _currentState = newState; // Switch to the new state.
            _currentState.LoadContent(); // Load the new state's content.
        }
    }
}
