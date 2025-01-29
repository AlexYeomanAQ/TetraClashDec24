using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;
using System.ComponentModel.Design;

namespace TetraClashDec24
{
    class SearchState : AppState
    {
        private SpriteFont SearchFont;
        private Button CancelButton;

        private bool isFound;
        private string searchMessage = "";
        int seed;
        private int matchID;

        private ButtonState prevClickState;
        public SearchState(App app, ButtonState clickState) : base(app)
        {
            prevClickState = clickState;
            isFound = false;
            StartSearchAsync();
        }

        public override void LoadContent()
        {
            CancelButton = new Button(@"base", 860, 710, 200, 100, Color.White, "Cancel");
            CancelButton.LoadContent(App.Content);

            SearchFont = App.Content.Load<SpriteFont>(@"titleFont");
        }

        public override void Update(GameTime gameTime)
        {
            if (isFound)
            {
                App.ChangeState(new MainGameState(App, prevClickState)); //, matchID, seed
            }

            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
            {
                Point mousePosition = new Point(mouse.X, mouse.Y);
                if (CancelButton.Box.Contains(mousePosition))
                {
                    //Add request to cancel
                }
            }
            prevClickState = mouse.LeftButton;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(App.GraphicsDevice);
            spriteBatch.Begin();
            Vector2 textSize = SearchFont.MeasureString(searchMessage);
            float textX = 1920 / 2 - textSize.X / 2;
            float textY = 1080 / 2 - textSize.Y / 2;
            spriteBatch.DrawString(SearchFont, searchMessage, new Vector2(textX, textY), Color.Black);
            CancelButton.Draw(spriteBatch);
            spriteBatch.End();
        }

        private async void StartSearchAsync()
        {
            searchMessage = "Searching...";
            try
            {
                string response = await Task.Run(() => Client.sendMessage($"search:{App.Username}"));

                if (response.StartsWith("found:"))
                {
                    string[] args = response.Split(':');

                    matchID = int.Parse(args[1]);
                    seed = int.Parse(args[2]);

                    
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
