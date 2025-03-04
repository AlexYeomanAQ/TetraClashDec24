using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Net.Sockets;
using System.Text;

namespace TetraClashDec24
{
    class SearchState : AppState
    {
        private Button CancelButton;

        private string searchMessage = "";

        private MouseState mouse;
        private ButtonState prevClickState;
        public SearchState(App app, ButtonState clickState) : base(app)
        {
            prevClickState = clickState;
            RunSearchAsync();
        }

        public override void LoadContent()
        {
            CancelButton = new Button(App, 860, 710, 200, 100, Color.White, "Cancel");
        }

        public override void Update(GameTime gameTime)
        {

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
            Vector2 textSize = App.titleFont.MeasureString(searchMessage);
            float textX = 1920 / 2 - textSize.X / 2;
            float textY = 1080 / 2 - textSize.Y / 2;
            spriteBatch.DrawString(App.titleFont, searchMessage, new Vector2(textX, textY), Color.Black);
            CancelButton.Draw(spriteBatch);
            spriteBatch.End();
        }

        private async Task RunSearchAsync()
        {
            try
            {
                // Send matchmaking request.
                byte[] requestBytes = Encoding.UTF8.GetBytes("search");
                await App._stream.WriteAsync(requestBytes, 0, requestBytes.Length);
                Console.WriteLine("Sent matchmaking request.");

                // Wait for the match found response.
                byte[] buffer = new byte[4096];
                int bytesRead = await App._stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (response.StartsWith("MATCH_FOUND:"))
                    {
                        string[] args = response.Substring(12).Split(':');
                        int matchID = int.Parse(args[0]);
                        string username = args[1];
                        Console.WriteLine("Match found! Match ID: " + matchID);

                        App.ChangeState(new MainGameState(App, prevClickState, matchID, matchID, username));
                    }
                    else if (response.StartsWith("Success"))
                    {
                        App.ChangeState(new MainMenuState(App, prevClickState));
                    }
                    else
                    {
                        Console.WriteLine("Unexpected response: " + response);
                    }
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }

        private async void CancelAsync()
        {
            searchMessage = "Cancelling...";
            try
            {
                await Client.SendMessageAsync(App._stream, $"cancel{App.Username}");
            }
            catch (Exception ex)
            {
                searchMessage = $"Error: {ex.Message}";
            }
        }
    }
}
