using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using System;
using System.Reflection.Metadata.Ecma335;

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

        private MouseState mouse;
        private ButtonState prevClickState;
        public SearchState(App app, ButtonState clickState) : base(app)
        {
            prevClickState = clickState;
            isFound = false;
            SearchAsync();
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
                App.ChangeState(new MainGameState(App, prevClickState, matchID, seed)); //, matchID, seed
            }

            mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
            {
                Point mousePosition = new Point(mouse.X, mouse.Y);
                if (CancelButton.Box.Contains(mousePosition))
                {
                    CancelAsync();
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

        private async void SearchAsync()
        {
            searchMessage = "Searching...";
            try
            {
                string[] args = null;
                string response = await Task.Run(() => Client.SendMessageAsync($"search:{App.Username}"));

                if (response == "Queue")
                {
                   args = await Client.ListenForMatch();
                }
                else
                {
                    Console.WriteLine(response);
                    searchMessage = "Error During Matchmaking. Press Cancel";
                }
                if (args == null)
                {
                    Console.WriteLine(response);
                    searchMessage = "Error During Matchmaking. Press Cancel";
                }
                else
                {
                    matchID = int.Parse(args[0]);
                    seed = int.Parse(args[1]);
                    mouse = Mouse.GetState();
                    App.ChangeState(new MainGameState(App, mouse.LeftButton, matchID, seed));
                }
            }
            catch (Exception ex)
            {
                searchMessage = $"Error: {ex.Message}";
            }
        }

        private async void CancelAsync()
        {
            searchMessage = "Cancelling...";
            try
            {
                string response = await Task.Run(() => Client.SendMessageAsync($"cancel:{App.Username}"));

                if (response == "Success")
                {
                    App.ChangeState(new MainMenuState(App, prevClickState));
                }
                //Any other response should mean that the user has found a match.
            }
            catch (Exception ex)
            {
                searchMessage = $"Error: {ex.Message}";
            }
        }
    }
}
