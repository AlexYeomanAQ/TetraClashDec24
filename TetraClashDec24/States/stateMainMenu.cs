using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Xna.Framework.Media;
using SharpDX.MediaFoundation;

namespace TetraClashDec24
{
    // Highscore DTO (Data Transfer Object) for JSON serialization and deserialization.
    public class Highscore
    {
        // The player's score.
        public int Score { get; set; }
        // The date the score was achieved.
        public string Date { get; set; }
    }

    // MainMenuState manages the main menu screen of the game.
    // It handles displaying UI elements like the Play and Help buttons,
    // fetching and displaying highscores, and showing a help overlay.
    public class MainMenuState : AppState
    {
        // Music to be played in the main menu.
        public Song menuMusic;
        // SpriteBatch for drawing 2D graphics.
        private SpriteBatch spriteBatch;
        // Texture for the game title/logo.
        public Texture2D titleTexture;

        // Buttons for starting the game and accessing the help menu.
        Button PlayButton;
        Button HelpButton;

        // Previous state of the mouse's left button (to detect new clicks).
        private ButtonState prevClickState;

        // List of highscore entries retrieved from the server.
        List<Highscore> highscores;

        // Variables to manage the help menu overlay.
        private bool showHelpMenu;
        private Rectangle helpMenuBackground; // The background rectangle for the help menu.
        private Button CloseHelpButton;       // Button to close the help menu.

        // Constructor initializes the main menu state, fetches highscores,
        // and sets up the help menu dimensions.
        public MainMenuState(App app, ButtonState clickState) : base(app)
        {
            prevClickState = clickState;
            // Create a new SpriteBatch for drawing.
            spriteBatch = new SpriteBatch(App.GraphicsDevice);
            // Asynchronously fetch highscores from the server.
            FetchHighScores(App._stream, App.Username);

            // Initially, the help menu is hidden.
            showHelpMenu = false;

            // Define dimensions for the help menu background.
            int helpMenuWidth = 600;
            int helpMenuHeight = 400;
            // Center the help menu on the screen.
            helpMenuBackground = new Rectangle(
                (1920 / 2) - (helpMenuWidth / 2),
                (1080 / 2) - (helpMenuHeight / 2),
                helpMenuWidth,
                helpMenuHeight
            );
        }

        // LoadContent is responsible for initializing UI components and loading assets.
        public override void LoadContent()
        {
            // Create the Play button positioned centrally.
            PlayButton = new Button(App, 860, 440, 200, 200, Color.White, "Play");

            // Load the main menu music from the content pipeline.
            menuMusic = App.Content.Load<Song>("menu");
            MediaPlayer.IsRepeating = true; // Ensure the music loops.
            MediaPlayer.Volume = 0.5f;        // Set music volume.

            // Create the Help button positioned in the bottom right corner.
            HelpButton = new Button(App, 1720, 980, 150, 80, Color.White, "Help");

            // Create the Close button for the help menu; positioned relative to the help menu background.
            CloseHelpButton = new Button(App,
                helpMenuBackground.Right - 100,
                helpMenuBackground.Bottom - 60,
                80, 40,
                Color.White, "Close");
        }

        // Update handles user input and updates the state of the main menu.
        public override void Update(GameTime gameTime)
        {
            // Retrieve the current mouse state.
            MouseState mouse = Mouse.GetState();

            // Ensure the menu music is playing.
            if (MediaPlayer.State != MediaState.Playing)
                MediaPlayer.Play(menuMusic);

            // Process a new mouse click (if left button state changed to pressed).
            if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
            {
                // Get the current mouse position.
                Point mousePosition = new Point(mouse.X, mouse.Y);

                // If the help menu is visible, only process the close button.
                if (showHelpMenu)
                {
                    if (CloseHelpButton.Box.Contains(mousePosition))
                    {
                        CloseHelpButton.PlaySound();
                        showHelpMenu = false; // Hide the help overlay.
                    }
                }
                else
                {
                    // Process the Play button click.
                    if (PlayButton.Box.Contains(mousePosition))
                    {
                        PlayButton.PlaySound();
                        // Transition to the SearchState (gameplay selection screen).
                        App.ChangeState(new SearchState(App, mouse.LeftButton));
                    }
                    // Process the Help button click.
                    else if (HelpButton.Box.Contains(mousePosition))
                    {
                        HelpButton.PlaySound();
                        showHelpMenu = true; // Display the help overlay.
                    }
                }
            }

            // Save the current click state for next frame's comparison.
            prevClickState = mouse.LeftButton;
        }

        // Draw renders the main menu and, if active, the help menu overlay.
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            // Draw the current player rating, centered on the screen.
            string ratingText = $"Current Rating: {App.Rating}";
            spriteBatch.DrawString(App.titleFont, ratingText, Cogs.centreTextPos(App.titleFont, ratingText, 960, 750), Color.White);

            // Draw the highscores list.
            DrawHighscores();

            // Draw the Play and Help buttons.
            PlayButton.Draw(spriteBatch);
            HelpButton.Draw(spriteBatch);

            // If the help menu is active, draw its overlay.
            if (showHelpMenu)
            {
                DrawHelpMenu();
            }

            spriteBatch.End();
        }

        // DrawHelpMenu renders the semi-transparent overlay and displays game controls.
        private void DrawHelpMenu()
        {
            // Create a 1x1 pixel texture to use for drawing rectangles.
            Texture2D pixel = new Texture2D(App.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.Black });
            // Draw a semi-transparent full-screen overlay.
            spriteBatch.Draw(pixel, new Rectangle(0, 0, 1920, 1080), new Color(0, 0, 0, 180));

            // Draw the help menu background using a solid color.
            spriteBatch.Draw(pixel, helpMenuBackground, Color.Purple);

            // Draw the help menu title ("Game Controls"), centered at the top.
            string titleText = "Game Controls";
            Vector2 titlePos = Cogs.centreTextPos(App.titleFont, titleText, helpMenuBackground.X + helpMenuBackground.Width / 2, helpMenuBackground.Y + 50);
            spriteBatch.DrawString(App.titleFont, titleText, titlePos, Color.White);

            // Set starting positions and line height for the control instructions.
            int startY = helpMenuBackground.Y + 120;
            int leftX = helpMenuBackground.X + 100;
            int lineHeight = 35;

            // Define an array of control instructions.
            string[] controls = {
                "Left Arrow: Move piece left",
                "Right Arrow: Move piece right",
                "Down Arrow: Increase speed of Falling Piece",
                "Up Arrow: Instantly Drop Falling Piece",
                "Z: Rotate piece anticlockwise",
                "X: Rotate piece clockwise",
                "C: Hold the currently falling block"
            };

            // Draw each control instruction line.
            for (int i = 0; i < controls.Length; i++)
            {
                spriteBatch.DrawString(App.font, controls[i], new Vector2(leftX, startY + i * lineHeight), Color.White);
            }

            // Draw the Close button for the help menu.
            CloseHelpButton.Draw(spriteBatch);
        }

        // FetchHighScores retrieves highscores from the server asynchronously.
        public async Task FetchHighScores(NetworkStream stream, string username)
        {
            try
            {
                // Construct a message to request highscores for the given username.
                string message = $"highscores{username}";
                // Send the message and await the JSON response.
                string highscoresJson = await Client.SendMessageAsync(stream, message, true);
                // Deserialize the JSON string into a list of Highscore objects.
                highscores = highscoresJson != null
                    ? JsonSerializer.Deserialize<List<Highscore>>(highscoresJson)
                    : null;
            }
            catch (Exception ex)
            {
                // Optionally log the error (error handling can be enhanced here).
            }
        }

        // DrawHighscores renders the list of highscores on the main menu.
        public void DrawHighscores()
        {
            // If no highscores exist, do not draw anything.
            // Define starting positions and styling for the highscore list.
            int startX = 100;
            int startY = 300;
            int lineHeight = 40;
            Color textColor = Color.White;

            // Draw the header for the highscores section.
            spriteBatch.DrawString(App.titleFont, "Highscores", new Vector2(startX + 60, startY - 50), textColor);
            // Draw column headers for "Score" and "Date".
            spriteBatch.DrawString(App.titleFont, "Score", new Vector2(startX, startY), textColor, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(App.titleFont, "Date", new Vector2(startX + 200, startY), textColor, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);

            if (highscores == null || highscores.Count == 0)
            {
                spriteBatch.DrawString(App.font, "No Highscores Loaded!", new Vector2(startX, startY + 50), textColor); //If no high scores are loaded from player, display generic message
            }
            else
            {
                for (int i = 0; i < Math.Min(highscores.Count, 10); i++)
                {
                    int yOffset = startY + 10 + (i + 1) * lineHeight;
                    // Draw the score.
                    spriteBatch.DrawString(App.font, highscores[i].Score.ToString(), new Vector2(startX, yOffset), textColor);
                    // Draw the date; consider truncating if it is too long.
                    string dateDisplay = highscores[i].Date;
                    spriteBatch.DrawString(App.font, dateDisplay, new Vector2(startX + 200, yOffset), textColor);
                }
            }
            // Loop through the highscores (up to 10 entries) and draw each.
            
        }
    }
}