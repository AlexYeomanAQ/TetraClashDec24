using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net;

namespace TetraClashDec24
{
    public class MainMenuState : AppState
    {
        SpriteBatch spriteBatch;

        Button PlayButton;

        private ButtonState prevClickState;
        List<(int Score, DateTime Date)> highscores;

        public MainMenuState(App app, ButtonState clickState) : base(app)
        {
            prevClickState = clickState;
            spriteBatch = new SpriteBatch(App.GraphicsDevice);
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
                highscores = JsonSerializer.Deserialize<List<(int Score, DateTime Date)>>(await Client.SendMessageAsync(stream, message, true));
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void DrawHighscores()
        {
            if (highscores == null || highscores.Count == 0) return;

            int startX = 100;
            int startY = 300;
            int lineHeight = 40;
            Color headerColor = Color.Yellow;
            Color textColor = Color.White;

            // Draw header
            spriteBatch.DrawString(App.font, "Highscores", new Vector2(startX, startY - lineHeight), headerColor);
            spriteBatch.DrawString(App.font, "Score", new Vector2(startX, startY), headerColor);
            spriteBatch.DrawString(App.font, "Date", new Vector2(startX + 200, startY), headerColor);

            // Draw highscores
            for (int i = 0; i < Math.Min(highscores.Count, 10); i++)
            {
                int yOffset = startY + (i + 1) * lineHeight;
                spriteBatch.DrawString(App.font, highscores[i].Score.ToString(),
                    new Vector2(startX, yOffset), textColor);
                spriteBatch.DrawString(App.font, highscores[i].Date.ToString("yyyy-MM-dd HH:mm"),
                    new Vector2(startX + 200, yOffset), textColor);
            }
        }
    }
}