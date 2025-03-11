using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.Json;

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
        SpriteBatch spriteBatch;
        Button PlayButton;
        private ButtonState prevClickState;
        List<Highscore> highscores;

        public MainMenuState(App app, ButtonState clickState) : base(app)
        {
            prevClickState = clickState;
            spriteBatch = new SpriteBatch(App.GraphicsDevice);
            // Fire-and-forget; consider awaiting or handling exceptions as needed.
            FetchHighScores(App._stream, App.Username);
        }

        public override void LoadContent()
        {
            PlayButton = new Button(App, 860, 440, 200, 200, Color.White, "Play");
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
            {
                Point mousePosition = new Point(mouse.X, mouse.Y);
                if (PlayButton.Box.Contains(mousePosition))
                {
                    PlayButton.PlaySound();
                    App.ChangeState(new SearchState(App, mouse.LeftButton));
                }
            }
            prevClickState = mouse.LeftButton;
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            string ratingText = $"Current Rating: {App.Rating}";
            spriteBatch.DrawString(App.titleFont, ratingText, Cogs.centreTextPos(App.titleFont, ratingText, 960, 750), Color.White);
            DrawHighscores();
            PlayButton.Draw(spriteBatch);
            spriteBatch.End();
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
            spriteBatch.DrawString(App.titleFont, "Highscores", new Vector2(startX+60, startY - 50), textColor);
            spriteBatch.DrawString(App.titleFont, "Score", new Vector2(startX, startY), textColor, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(App.titleFont, "Date", new Vector2(startX + 200, startY), textColor, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);

            // Draw each highscore entry (up to 10 entries)
            for (int i = 0; i < Math.Min(highscores.Count, 10); i++)
            {
                int yOffset = startY+10 + (i + 1) * lineHeight;
                spriteBatch.DrawString(App.font, highscores[i].Score.ToString(), new Vector2(startX, yOffset), textColor);
                // Substring the date if it is long enough
                string dateDisplay = highscores[i].Date;
                spriteBatch.DrawString(App.font, dateDisplay, new Vector2(startX + 200, yOffset), textColor);
            }
        }
    }
}
