using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using System;
using System.Text;
using Microsoft.Xna.Framework.Media;

namespace TetraClashDec24
{
    // SearchState handles matchmaking: sending a search request to the server,
    // waiting for a match to be found, and transitioning to the main game when ready.
    class SearchState : AppState
    {
        // Button to cancel the matchmaking search.
        private Button CancelButton;

        // Message displayed on the screen indicating the search status.
        private string searchMessage = "Searching...";

        // Current mouse state and previous click state for input handling.
        private MouseState mouse;
        private ButtonState prevClickState;

        // Constructor: Initializes the state with the current click state and
        // immediately starts the asynchronous search operation.
        public SearchState(App app, ButtonState clickState) : base(app)
        {
            prevClickState = clickState;
            // Start the matchmaking search in the background.
            RunSearchAsync();
        }

        // LoadContent initializes the UI elements for this state.
        public override void LoadContent()
        {
            // Create a Cancel button at the specified position and size.
            CancelButton = new Button(App, 860, 710, 200, 100, Color.White, "Cancel");
        }

        // Update is called every frame to process user input and update state.
        public override void Update(GameTime gameTime)
        {
            // Get the current state of the mouse.
            mouse = Mouse.GetState();

            // Check if the left mouse button was just pressed.
            if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
            {
                Point mousePosition = new Point(mouse.X, mouse.Y);
                // If the Cancel button is clicked, play its sound and cancel the search.
                if (CancelButton.Box.Contains(mousePosition))
                {
                    CancelButton.PlaySound();
                    CancelAsync();
                }
            }
            // Update the previous mouse click state for the next frame.
            prevClickState = mouse.LeftButton;
        }

        // Draw renders the current search message and the Cancel button.
        public override void Draw(GameTime gameTime)
        {
            // Create a new SpriteBatch for drawing.
            SpriteBatch spriteBatch = new SpriteBatch(App.GraphicsDevice);
            spriteBatch.Begin();

            // Measure the search message text to center it on the screen.
            Vector2 textSize = App.titleFont.MeasureString(searchMessage);
            float textX = 1920 / 2 - textSize.X / 2;
            float textY = 1080 / 2 - textSize.Y / 2;
            // Draw the search message in black.
            spriteBatch.DrawString(App.titleFont, searchMessage, new Vector2(textX, textY), Color.Black);

            // Draw the Cancel button.
            CancelButton.Draw(spriteBatch);

            spriteBatch.End();
        }

        // RunSearchAsync sends a matchmaking request to the server and waits for a response.
        private async Task RunSearchAsync()
        {
            try
            {
                // Create a UTF8-encoded byte array for the "search" request.
                byte[] requestBytes = Encoding.UTF8.GetBytes("search");
                // Write the search request to the network stream.
                await App._stream.WriteAsync(requestBytes, 0, requestBytes.Length);
                Console.WriteLine("Sent matchmaking request.");

                // Prepare a buffer to receive the server's response.
                byte[] buffer = new byte[4096];
                // Read the server response asynchronously.
                int bytesRead = await App._stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    // Convert the response bytes to a string.
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // If a match is found, the response starts with "MATCH_FOUND:"
                    if (response.StartsWith("MATCH_FOUND:"))
                    {
                        // Extract match parameters (e.g., matchID and opponent username) from the response.
                        string[] args = response.Substring(12).Split(':');
                        int matchID = int.Parse(args[0]);
                        string username = args[1];
                        Console.WriteLine("Match found! Match ID: " + matchID);

                        // Stop the menu music before transitioning to the game.
                        MediaPlayer.Stop();
                        // Change to the MainGameState, passing in matchID and the opponent's username.
                        App.ChangeState(new MainGameState(App, prevClickState, matchID, matchID, username));
                    }
                    // If the server responds with "Success", return to the main menu.
                    else if (response.StartsWith("Success"))
                    {
                        App.ChangeState(new MainMenuState(App, prevClickState));
                    }
                    else
                    {
                        // Log any unexpected responses for debugging.
                        Console.WriteLine("Unexpected response: " + response);
                    }
                }
            }
            catch (Exception ex)
            {
                // If an exception occurs, write the error message to the console.
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }

        // CancelAsync sends a cancel request to the server and updates the search message.
        private async void CancelAsync()
        {
            // Update the search message to indicate cancellation is in progress.
            searchMessage = "Cancelling...";
            try
            {
                // Send a cancel message including the username to the server.
                await Client.SendMessageAsync(App._stream, $"cancel{App.Username}");
            }
            catch (Exception ex)
            {
                // On error, update the search message with the error information.
                searchMessage = $"Error: {ex.Message}";
            }
            // Reset the previous click state after cancellation.
            prevClickState = ButtonState.Released;
        }
    }
}
