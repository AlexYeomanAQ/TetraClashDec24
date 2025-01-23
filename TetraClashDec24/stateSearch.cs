using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using TetraClashDec24;
using Microsoft.Xna.Framework.Graphics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;

namespace TetraClashDec24
{
    class SearchState : AppState
    {
        private SpriteFont SearchFont;
        private Button CancelButton;

        private bool isFound;
        private string searchMessage = "";
        private int matchID;

        private ButtonState prevClickState;
        public SearchState(App1 app, ButtonState clickState) : base(app)
        {
            prevClickState = clickState;
            isFound = false;
            StartSearchAsync();
        }

        public override void LoadContent()
        {
            CancelButton = new Button(@"base", 860, 710, 200, 100, Color.White, "Cancel");
            CancelButton.LoadContent(App.Content);

            SearchFont = App.Content.Load<SpriteFont>(@"myFont");
        }

        public override void Update(GameTime gameTime)
        {
            if (isFound)
            {
                App.ChangeState(new MainGameState(App, prevClickState));
            }

            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
            {
                Point mousePosition = new Point(mouse.X, mouse.Y);
                if (CancelButton.Box.Contains(mousePosition))
                {
                    
                }
            }
            prevClickState = mouse.LeftButton;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(App.GraphicsDevice);
            spriteBatch.Begin();
            Vector2 textSize = SearchFont.MeasureString(searchMessage);
            float textX = (1920 / 2) - (textSize.X / 2);
            float textY = (1080 / 2) - (textSize.Y / 2);
            spriteBatch.DrawString(SearchFont, searchMessage, new Vector2(textX, textY), Color.Black);
            CancelButton.Draw(spriteBatch);
            spriteBatch.End();
        }

        private async void StartSearchAsync()
        {
            searchMessage = "Searching...";
            try
            {
                string response = await Task.Run(() => Client.sendMessage($"search:{Game.Username}"));

                if (response.StartsWith("found:"))
                {
                    matchID = int.Parse(response.Substring(6));
                    searchMessage = $"Match Found! ID: {matchID}";
                }
                else
                {
                    searchMessage = "No Match Found";
                }
            }
            catch (Exception ex)
            {
                searchMessage = $"Error: {ex.Message}";
            }
            finally
            {
                isFound = true;
            }
        }
    }
}
