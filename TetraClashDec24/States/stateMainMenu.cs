using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Xna.Framework.Media;
using System.Reflection.Metadata;

namespace TetraClashDec24
{
    // Define the Highscore DTO so JSON serialization/deserialization works as expected.
    public class Highscore
    {
        public int Score { get; set; }
        public string Date { get; set; }
    }

    public class MainMenuState : AppState
    {
        public Song menuMusic;
        private SpriteBatch spriteBatch;
        public Texture2D titleTexture;

        Button PlayButton;
        Button HelpButton;

        private ButtonState prevClickState;

        List<Highscore> highscores;

        // Help menu state and components
        private bool showHelpMenu;
        private Rectangle helpMenuBackground;
        private Button CloseHelpButton;

        public MainMenuState(App app, ButtonState clickState) : base(app)
        {
            prevClickState = clickState;
            spriteBatch = new SpriteBatch(App.GraphicsDevice);
            FetchHighScores(App._stream, App.Username);

            // Initialize help menu state
            showHelpMenu = false;

            // Create a centered rectangle for the help menu background
            int helpMenuWidth = 600;
            int helpMenuHeight = 400;
            helpMenuBackground = new Rectangle(
                (1920 / 2) - (helpMenuWidth / 2),
                (1080 / 2) - (helpMenuHeight / 2),
                helpMenuWidth,
                helpMenuHeight
            );
        }

        public override void LoadContent()
        {
            PlayButton = new Button(App, 860, 440, 200, 200, Color.White, "Play");

            menuMusic = App.Content.Load<Song>("menu");
            MediaPlayer.IsRepeating = true; // Loops the music
            MediaPlayer.Volume = 0.5f;

            // Add Help button in the bottom right corner
            HelpButton = new Button(App, 1720, 980, 150, 80, Color.White, "Help");

            // Create close button for the help menu
            CloseHelpButton = new Button(App,
                helpMenuBackground.Right - 100,
                helpMenuBackground.Bottom - 60,
                80, 40,
                Color.White, "Close");
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();

            if (MediaPlayer.State != MediaState.Playing)
                MediaPlayer.Play(menuMusic);

            if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
            {
                Point mousePosition = new Point(mouse.X, mouse.Y);

                if (showHelpMenu)
                {
                    // Only check the close button when help menu is shown
                    if (CloseHelpButton.Box.Contains(mousePosition))
                    {
                        CloseHelpButton.PlaySound();
                        showHelpMenu = false;
                    }
                }
                else
                {
                    // Check other buttons when help menu is not shown
                    if (PlayButton.Box.Contains(mousePosition))
                    {
                        PlayButton.PlaySound();
                        App.ChangeState(new SearchState(App, mouse.LeftButton));
                    }
                    else if (HelpButton.Box.Contains(mousePosition))
                    {
                        HelpButton.PlaySound();
                        showHelpMenu = true;
                    }
                }
            }

            prevClickState = mouse.LeftButton;
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            // Draw the main menu content
            string ratingText = $"Current Rating: {App.Rating}";
            spriteBatch.DrawString(App.titleFont, ratingText, Cogs.centreTextPos(App.titleFont, ratingText, 960, 750), Color.White);
            DrawHighscores();
            PlayButton.Draw(spriteBatch);
            HelpButton.Draw(spriteBatch);

            // Draw the help menu overlay if it's visible
            if (showHelpMenu)
            {
                DrawHelpMenu();
            }

            spriteBatch.End();
        }

        private void DrawHelpMenu()
        {
            // Draw semi-transparent background overlay
            Texture2D pixel = new Texture2D(App.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.Black });
            spriteBatch.Draw(pixel, new Rectangle(0, 0, 1920, 1080), new Color(0, 0, 0, 180));

            // Draw help menu background
            spriteBatch.Draw(pixel, helpMenuBackground, Color.Purple);

            // Draw help menu title
            string titleText = "Game Controls";
            Vector2 titlePos = Cogs.centreTextPos(App.titleFont, titleText, helpMenuBackground.X + helpMenuBackground.Width / 2, helpMenuBackground.Y + 50);
            spriteBatch.DrawString(App.titleFont, titleText, titlePos, Color.White);

            // Draw control instructions
            int startY = helpMenuBackground.Y + 120;
            int leftX = helpMenuBackground.X + 100;
            int lineHeight = 40;

            string[] controls = {
                "Left Arrow: Move piece left",
                "Right Arrow: Move piece right",
                "Z: Rotate piece anticlockwise",
                "X: Rotate piece clockwise",
                "C: Hold the currently falling block"
            };

            for (int i = 0; i < controls.Length; i++)
            {
                spriteBatch.DrawString(App.font, controls[i], new Vector2(leftX, startY + i * lineHeight), Color.White);
            }

            // Draw close button
            CloseHelpButton.Draw(spriteBatch);
        }

        public async Task FetchHighScores(NetworkStream stream, string username)
        {
            try
            {
                string message = $"highscores{username}";
                string highscoresJson = await Client.SendMessageAsync(stream, message, true);
                highscores = highscoresJson != null
                    ? JsonSerializer.Deserialize<List<Highscore>>(highscoresJson)
                    : null;
            }
            catch (Exception ex)
            {
                // Optionally log the error.
            }
        }

        public void DrawHighscores()
        {
            if (highscores == null || highscores.Count == 0)
                return;

            int startX = 100;
            int startY = 300;
            int lineHeight = 40;
            Color textColor = Color.White;

            // Draw header
            spriteBatch.DrawString(App.titleFont, "Highscores", new Vector2(startX + 60, startY - 50), textColor);
            spriteBatch.DrawString(App.titleFont, "Score", new Vector2(startX, startY), textColor, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(App.titleFont, "Date", new Vector2(startX + 200, startY), textColor, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);

            // Draw each highscore entry (up to 10 entries)
            for (int i = 0; i < Math.Min(highscores.Count, 10); i++)
            {
                int yOffset = startY + 10 + (i + 1) * lineHeight;
                spriteBatch.DrawString(App.font, highscores[i].Score.ToString(), new Vector2(startX, yOffset), textColor);
                // Substring the date if it is long enough
                string dateDisplay = highscores[i].Date;
                spriteBatch.DrawString(App.font, dateDisplay, new Vector2(startX + 200, yOffset), textColor);
            }
        }
    }
}