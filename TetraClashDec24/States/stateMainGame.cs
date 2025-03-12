using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework; 
using Microsoft.Xna.Framework.Graphics; 
using System; 
using System.Threading.Tasks;
using System.Net.Sockets; 
using System.Text.Json; 
using System.Text; 
using Microsoft.Xna.Framework.Media; 

namespace TetraClashDec24 // Define the TetraClashDec24 namespace
{
    // MainGameState manages the primary gameplay state, including input, rendering, network updates, and game logic.
    public class MainGameState : AppState
    {
        private SpriteBatch spriteBatch; // SpriteBatch used for drawing textures onto the screen. 
        private Texture2D[] tetrominoTextures; // Array holding textures for each tetromino type.
        private Texture2D gridTexture; // Texture for the grid (if used).

        private int tileSize = 25; // Size of each tile in the grid.
        private int PlayerGridX; // X-coordinate where the player's grid is drawn.
        private int PlayerGridY; // Y-coordinate where the player's grid is drawn.
        private int EnemyGridX;  // X-coordinate where the enemy's grid is drawn.
        private int EnemyGridY;  // Y-coordinate where the enemy's grid is drawn.

        private int dropTimer; // Timer used to track drop intervals.
        private int dropRate;  // Current rate at which tetrominos drop.
        private bool fastDrop; // Flag to indicate if fast drop is enabled.

        private GameController gameController; // GameController instance managing game logic and tetromino behavior.

        private long MatchID; // Unique match identifier.

        private string enemyUsername; // Opponent's username.
        private int[][] enemyGrid; // 2D array representing the opponent's game grid.
        private string enemyLevel = "0"; // Opponent's level (as a string).
        private string enemyScore = "0"; // Opponent's score (as a string).

        private bool ScoreSent = false; // Flag indicating whether the score has been sent.
        private bool hasPlayedResultSound = false; // Flag to ensure the result sound is played only once.
        private string MatchResult = ""; // String holding the result of the match.
        private int RatingAdjustment; // Player's rating adjustment after the match.

        private Button returnToMenu_Button; // Button for returning to the main menu.

        private bool _isTetrominoTaskRunning = false; // Flag indicating if the tetromino drop task is running.

        private KeyboardState keyboard; // Current keyboard state.
        private MouseState mouse; // Current mouse state.

        private KeyboardState prevKeyboardState; // Previous frame's keyboard state.
        private ButtonState prevClickState; // Previous frame's mouse click state.

        private DateTime matchStartTime; // Time when the match started.
        private TimeSpan matchDuration = TimeSpan.FromMinutes(1); // Total match duration (1 minute).

        // Modified constructor: accepts TcpClient instead of a NetworkStream.
        public MainGameState(App app, ButtonState clickState, long matchID, int seed, string enemy_username) : base(app)
        {
            prevClickState = clickState; // Store the previous mouse click state.
            spriteBatch = new SpriteBatch(App.GraphicsDevice); // Initialize spriteBatch with the graphics device.
            tetrominoTextures = new Texture2D[8]; // Create an array for 8 tetromino textures.
            gameController = new GameController(seed); // Initialize the game controller with the provided seed.
            gameController.BlockLanded += OnBlockLanded; // Subscribe to the BlockLanded event.
            gameController.LinesCleared += OnLinesCleared; // Subscribe to the LinesCleared event.
            PlayerGridX = 1920 / 4 - (gameController.GameBoard.Collumns * tileSize / 2) - 200; // Calculate player's grid X position.
            PlayerGridY = 1080 / 2 - ((gameController.GameBoard.Rows - 2) * tileSize / 2); // Calculate player's grid Y position.
            enemyUsername = enemy_username; // Set the enemy's username.
            EnemyGridX = 1920 * 3 / 4 - (gameController.GameBoard.Collumns * tileSize / 2) - 200; // Calculate enemy's grid X position.
            EnemyGridY = PlayerGridY; // Set enemy grid Y position to match player's grid Y.
            dropTimer = 0; // Initialize drop timer to 0.
            dropRate = Cogs.getDropRate(gameController.Level); // Set drop rate based on the current level.
            MatchID = matchID; // Assign the match ID.
            matchStartTime = DateTimeOffset.FromUnixTimeSeconds(MatchID).UtcDateTime; // Convert matchID into a UTC DateTime.
            enemyGrid = new int[22][]; // Create the enemy grid with 22 rows.
            for (int i = 0; i < 22; i++) // For each row in the enemy grid...
            {
                enemyGrid[i] = new int[10]; // ...initialize it with 10 columns.
            }
            returnToMenu_Button = new Button(App, 860, 700, 200, 100, Color.White, "Return to Menu"); // Create a return-to-menu button.
            _ = Task.Run(() => SendGridUpdatesAsync(App._stream)); // Start an asynchronous task to send grid updates.
            _ = Task.Run(() => ListenForGridUpdatesAsync(App._stream)); // Start an asynchronous task to listen for grid updates.
            _ = Task.Run(() => TimerTaskAsync()); // Start the match timer asynchronously.
        }

        // LoadContent loads game assets like textures and media.
        public override void LoadContent()
        {
            for (int i = 0; i < tetrominoTextures.Length + 1; i++) // Loop to load all tetromino textures.
            {
                tetrominoTextures[i] = App.Content.Load<Texture2D>(@$"{i}"); // Load texture with identifier matching the loop index.
            }
            MediaPlayer.IsRepeating = true; // Set media player to loop the music.
            MediaPlayer.Volume = 0.5f; // Set the music volume.
        }

        // Update processes user input, game state, and triggers tetromino drop.
        public override void Update(GameTime gameTime)
        {
            prevKeyboardState = keyboard; // Save previous keyboard state.
            mouse = Mouse.GetState(); // Update mouse state.
            keyboard = Keyboard.GetState(); // Update keyboard state.
            if (MediaPlayer.State != MediaState.Playing && App.gameMusic != null) // If music is not playing and exists...
                MediaPlayer.Play(App.gameMusic); // ...start playing game music.
            if (MatchResult == "") // If match result is not yet determined (game is ongoing)...
            {
                Keys[] pressedKeys = keyboard.GetPressedKeys(); // Get all currently pressed keys.
                bool fastSwitch = false; // Initialize fast drop flag.
                foreach (Keys key in pressedKeys) // Iterate through each pressed key...
                {
                    if (key == Keys.Down) fastSwitch = true; // If Down key is pressed, enable fast drop.
                    else if (key != Keys.Down && !fastSwitch) fastSwitch = false; // Otherwise, disable fast drop.
                    if (prevKeyboardState.IsKeyUp(key)) // If key was not pressed in the previous state...
                    {
                        if (key == Keys.Left) gameController.TranslateTetrominoLeft(); // Move tetromino left.
                        else if (key == Keys.Right) gameController.TranslateTetrominoRight(); // Move tetromino right.
                        else if (key == Keys.Up) gameController.DropTetromino(); // Instantly drop tetromino.
                        else if (key == Keys.Z) gameController.RotateTetrominoCCW(); // Rotate tetromino counterclockwise.
                        else if (key == Keys.X) gameController.RotateTetrominoCW(); // Rotate tetromino clockwise.
                        else if (key == Keys.C) gameController.HoldTetromino(); // Hold current tetromino.
                    }
                }
                fastDrop = fastSwitch; // Update fastDrop flag based on key input.
                if (!_isTetrominoTaskRunning && MatchResult == "") // If tetromino drop task is not running and game is ongoing...
                {
                    DropTetromino(); // Start the tetromino drop process.
                }
            }
            else // Else, if match result is set (game over)...
            {
                MediaPlayer.Stop(); // Stop the music.
                if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton) // If left mouse button just pressed...
                {
                    Point mousePosition = new Point(mouse.X, mouse.Y); // Get current mouse position.
                    if (returnToMenu_Button.Box.Contains(mousePosition)) // If mouse click is within the return-to-menu button...
                    {
                        App.ChangeState(new MainMenuState(App, mouse.LeftButton)); // Change the state to the main menu.
                    }
                }
            }
            prevClickState = mouse.LeftButton; // Update previous mouse click state.
        }

        // OnBlockLanded handles the event when a tetromino block lands.
        private void OnBlockLanded(object sender, EventArgs e) { App.sound_TetrominoLand.Play(); } // Play landing sound.

        // OnLinesCleared handles the event when lines are cleared.
        private void OnLinesCleared(object sender, int numLines) { App.sound_LineClear.Play(); } // Play line clear sound.

        // Draw renders all game elements on the screen.
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend); // Begin sprite drawing with alpha blending.
            DrawGrid(gameController.GameBoard.grid, PlayerGridX, PlayerGridY); // Draw player's grid.
            DrawTetromino(gameController.CurrentTetromino, PlayerGridX, PlayerGridY); // Draw current tetromino.
            DrawGhostTetromino(gameController.CurrentTetromino, PlayerGridX, PlayerGridY); // Draw ghost tetromino.
            DrawPreviewTetromino(gameController.TetrominoQueue); // Draw preview of next tetromino.
            DrawHeldTetromino(); // Draw held tetromino.
            DrawGrid(enemyGrid, EnemyGridX, EnemyGridY); // Draw enemy's grid.

            spriteBatch.DrawString(App.titleFont, "You", new Vector2(PlayerGridX + 100, PlayerGridY), Color.White); // Draw "You" label on player's side.
            spriteBatch.DrawString(App.titleFont, "Level", Cogs.centreTextPos(App.titleFont, "Level", 760, 520), Color.White); // Draw "Level" label.
            spriteBatch.DrawString(App.titleFont, gameController.Level.ToString(), Cogs.centreTextPos(App.font, gameController.Level.ToString(), 760, 550), Color.White); // Draw player's level.
            spriteBatch.DrawString(App.titleFont, "Lines Cleared", Cogs.centreTextPos(App.titleFont, "Lines Cleared", 760, 640), Color.White); // Draw "Lines Cleared" label.
            spriteBatch.DrawString(App.titleFont, gameController.TotalLinesCleared.ToString(), Cogs.centreTextPos(App.font, gameController.TotalLinesCleared.ToString(), 760, 670), Color.White); // Draw total lines cleared.
            spriteBatch.DrawString(App.titleFont, "Score", Cogs.centreTextPos(App.titleFont, "Score", 760, 760), Color.White); // Draw "Score" label.
            spriteBatch.DrawString(App.titleFont, gameController.Score.ToString(), Cogs.centreTextPos(App.font, gameController.Score.ToString(), 760, 780), Color.White); // Draw player's score.
            spriteBatch.DrawString(App.titleFont, enemyUsername, Cogs.centreTextPos(App.titleFont, enemyUsername, EnemyGridX + 150, EnemyGridY), Color.White); // Draw enemy username.
            spriteBatch.DrawString(App.titleFont, "Level", Cogs.centreTextPos(App.titleFont, "Level", 1650, 520), Color.White); // Draw enemy "Level" label.
            spriteBatch.DrawString(App.titleFont, enemyLevel, Cogs.centreTextPos(App.font, enemyLevel, 1650, 550), Color.White); // Draw enemy level.
            spriteBatch.DrawString(App.titleFont, "Score", Cogs.centreTextPos(App.titleFont, "Score", 1650, 640), Color.White); // Draw enemy "Score" label.
            spriteBatch.DrawString(App.titleFont, enemyScore, Cogs.centreTextPos(App.font, enemyScore, 1650, 670), Color.White); // Draw enemy score.
            if (gameController.GameOver) // If the game is over...
            {
                spriteBatch.Draw(App.baseTexture, new Rectangle(0, 0, 1920, 1080), new Color(64, 64, 64, 64)); // Draw semi-transparent overlay.
                spriteBatch.Draw(App.baseTexture, new Rectangle(660, 440, 600, 400), new Color(240, 240, 240)); // Draw result panel.
                spriteBatch.DrawString(App.titleFont, MatchResult, Cogs.centreTextPos(App.titleFont, MatchResult, 960, 540), Color.Black); // Draw match result.
                if (RatingAdjustment != null) // If rating adjustment is available...
                {
                    if (!hasPlayedResultSound) // If result sound has not been played...
                    {
                        if (RatingAdjustment >= 0) App.sound_Win.Play(0.5f, 0, 0); // Play win sound if rating increased.
                        else App.sound_Lose.Play(0.5f, 0, 0); // Play lose sound if rating decreased.
                        hasPlayedResultSound = true; // Mark result sound as played.
                    }
                    string ratingAdjustmentString = $"Rating Change: {RatingAdjustment}"; // Create rating adjustment message.
                    spriteBatch.DrawString(App.titleFont, ratingAdjustmentString, Cogs.centreTextPos(App.titleFont, ratingAdjustmentString, 960, 640), Color.Black); // Draw rating adjustment message.
                }
                returnToMenu_Button.Draw(spriteBatch); // Draw the return-to-menu button.
            }
            else // Otherwise (game ongoing)...
            {
                DrawTimer(); // Draw the match timer.
            }
            spriteBatch.End(); // End spriteBatch drawing.
        }

        // DrawGrid renders a grid based on a 2D integer array at a given position.
        private void DrawGrid(int[][] grid, int x, int y)
        {
            int Rows = grid.Length; // Determine number of rows.
            int Columns = grid[0].Length; // Determine number of columns (assumes at least one row exists).
            for (int r = 2; r < Rows; r++) // Loop from row 2 to avoid hidden rows.
            {
                for (int c = 0; c < Columns; c++) // Loop through each column.
                {
                    int id = grid[r][c]; // Get the tetromino ID at the current cell.
                    spriteBatch.Draw(tetrominoTextures[id], new Rectangle(x + (c * tileSize), y + (r * tileSize), tileSize, tileSize), Color.White); // Draw the cell with its tetromino texture.
                }
            }
        }

        // DropTetromino continuously moves the tetromino downward until the game is over.
        private async void DropTetromino()
        {
            _isTetrominoTaskRunning = true; // Set flag to indicate the drop task is running.
            try
            {
                while (!gameController.GameOver) // Continue while the game is not over.
                {
                    dropRate = Cogs.getDropRate(gameController.Level); // Update the drop rate based on current level.
                    if (fastDrop) await Task.Delay(Math.Min(100, dropRate)); // If fast drop is enabled, wait a shorter time.
                    else await Task.Delay(dropRate); // Otherwise, wait the normal drop rate.
                    gameController.TranslateTetrominoDown(); // Move the tetromino down by one row.
                }
            }
            finally
            {
                _isTetrominoTaskRunning = false; // Reset the drop task flag.
                await HandleGameOver(); // Handle game over actions.
            }
        }

        // DrawTetromino renders a tetromino at a specified position, optionally ignoring its offset.
        private void DrawTetromino(Tetromino tetromino, int x, int y, bool ignore_offset = false)
        {
            if (tetromino == null) return; // Exit if there is no tetromino.
            foreach (Position p in tetromino.BlockPositions(ignore_offset)) // Iterate over each block position.
            {
                if (p.Row > 1 || ignore_offset) // Only draw blocks in visible rows or if offset is ignored.
                    spriteBatch.Draw(tetrominoTextures[tetromino.TetrominoID], new Rectangle(x + (p.Column * tileSize), y + (p.Row * tileSize), tileSize, tileSize), Color.White); // Draw each tetromino block.
            }
        }

        // DrawGhostTetromino renders a translucent version of the tetromino at its drop destination.
        private void DrawGhostTetromino(Tetromino tetromino, int x, int y)
        {
            if (tetromino == null) return; // Exit if there is no tetromino.
            int dropDistance = gameController.TetrominoDropDistance(); // Compute how far the tetromino will drop.
            foreach (Position p in tetromino.BlockPositions()) // Iterate over each block position.
            {
                if (p.Row + dropDistance > 1) // Only draw if the block is visible after dropping.
                    spriteBatch.Draw(tetrominoTextures[tetromino.TetrominoID], new Rectangle(x + (p.Column * tileSize), y + ((p.Row + dropDistance) * tileSize), tileSize, tileSize), new Color(64, 64, 64, 64)); // Draw ghost block with transparency.
            }
        }

        // DrawPreviewTetromino renders the next tetromino preview in a designated area.
        private void DrawPreviewTetromino(TetrominoQueue tetrominoQueue)
        {
            Tetromino next = tetrominoQueue.NextTetromino; // Get the next tetromino.
            spriteBatch.DrawString(App.titleFont, "Next", new Vector2(480, 500), Color.White); // Draw "Next" label.
            spriteBatch.Draw(App.baseTexture, new Rectangle(480, 530, 105, 100), Color.Black); // Draw preview background.
            DrawTetromino(next, 490, 545, true); // Draw the next tetromino with offsets ignored.
        }

        // DrawHeldTetromino renders the tetromino that the player is holding.
        private void DrawHeldTetromino()
        {
            spriteBatch.DrawString(App.titleFont, "Hold", new Vector2(480, 670), Color.White); // Draw "Hold" label.
            spriteBatch.Draw(App.baseTexture, new Rectangle(480, 700, 115, 100), Color.Black); // Draw hold box background.
            if (gameController.HeldTetromino == null) return; // Exit if no tetromino is held.
            DrawTetromino(gameController.HeldTetromino, 490, 730, true); // Draw the held tetromino.
        }

        // HandleGameOver sends a loss message if no match result has been determined.
        private async Task HandleGameOver()
        {
            if (MatchResult == "") // If match result is empty...
            {
                string message = $"lose{gameController.Score}"; // Build a loss message with the current score.
                await Client.SendMessageAsync(App._stream, message); // Send the loss message to the server.
            }
        }

        // AddFallingBlockToGrid creates a deep copy of the grid and adds the falling tetromino, in order to be able to send the grid over to the other player.
        private int[][] AddFallingBlockToGrid(int[][] grid, Tetromino tetromino)
        {
            int[][] tempGrid = new int[grid.Length][]; // Create a new grid array.
            for (int i = 0; i < grid.Length; i++) // For each row in the grid...
            {
                tempGrid[i] = new int[grid[i].Length]; // Initialize a new row.
                Array.Copy(grid[i], tempGrid[i], grid[i].Length); // Copy the row's data.
            }
            if (tetromino != null) // If there is a falling tetromino...
            {
                foreach (Position p in tetromino.BlockPositions()) // For each block in the tetromino...
                {
                    tempGrid[p.Row][p.Column] = tetromino.TetrominoID; // Add the block's ID to the grid copy.
                }
            }
            return tempGrid; // Return the modified grid copy.
        }

        // SendGridUpdatesAsync sends periodic grid updates over the network stream.
        private async Task SendGridUpdatesAsync(NetworkStream stream)
        {
            while (true) // Continue sending updates indefinitely.
            {
                try
                {
                    int[][] gridWithFallingBlocks = AddFallingBlockToGrid(gameController.GameBoard.grid, gameController.CurrentTetromino); // Create a grid copy with the current tetromino.
                    string gridJson = JsonSerializer.Serialize(gridWithFallingBlocks); // Serialize the grid copy to JSON.
                    string message = $"match:{gridJson}:{gameController.Level}:{gameController.Score}"; // Combine grid JSON with level and score into one message.
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message); // Convert the message to UTF8 bytes.
                    await stream.WriteAsync(messageBytes, 0, messageBytes.Length); // Write the message to the network stream.
                    Console.WriteLine("Sent grid update"); // Log that the update was sent.
                    await Task.Delay(dropRate); // Wait for the drop rate duration.
                    if (gameController.GameOver) break; // Break the loop if the game is over.
                }
                catch (Exception ex) // On exception...
                {
                    Console.WriteLine("Error sending grid update: " + ex.Message); // Log the error.
                    break; // Exit the loop.
                }
            }
        }

        // ListenForGridUpdatesAsync listens for incoming grid updates from the opponent.
        private async Task ListenForGridUpdatesAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[4096]; // Create a buffer for incoming data.
            while (true) // Loop indefinitely.
            {
                int bytesRead = 0; // Initialize the number of bytes read.
                try
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // Read from the network stream.
                    if (bytesRead == 0) // If no bytes were read...
                    {
                        Console.WriteLine("Disconnected from server."); // Log disconnection.
                        break; // Exit the loop.
                    }
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim(); // Convert bytes to string and trim.
                    if (message.StartsWith("GRID_UPDATE:")) // If the message is a grid update...
                    {
                        string[] args = message.Substring("GRID_UPDATE:".Length).Split(':'); // Split the message into arguments.
                        try
                        {
                            enemyGrid = JsonSerializer.Deserialize<int[][]>(args[0]); // Deserialize enemy grid from JSON.
                            enemyLevel = args[1]; // Update enemy level.
                            enemyScore = args[2]; // Update enemy score.
                            Console.WriteLine("Received match update."); // Log reception of update.
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error deserializing grid data: " + ex.Message); // Log deserialization error.
                        }
                    }
                    else if (message.StartsWith("MATCH_WIN")) // If message indicates a win...
                    {
                        RatingAdjustment = int.Parse(message.Split(':')[1]); // Parse rating adjustment.
                        MatchResult = "You win!"; // Set match result to win.
                        gameController.GameOver = true; // Mark the game as over.
                        if (!ScoreSent) // If score hasn't been sent yet...
                        {
                            await Client.SendMessageAsync(stream, $"score{gameController.Score}"); // Send the score.
                            ScoreSent = true; // Mark score as sent.
                        }
                        break; // Exit the loop.
                    }
                    else if (message.StartsWith("MATCH_LOSE")) // If message indicates a loss...
                    {
                        MatchResult = "You lose!"; // Set match result to lose.
                        RatingAdjustment = int.Parse(message.Split(':')[1]) * -1; // Parse and negate rating adjustment.
                        break; // Exit the loop.
                    }
                    else if (message.StartsWith("MATCH_TIE")) // If message indicates a tie...
                    {
                        MatchResult = "Tied!"; // Set match result to tied.
                        RatingAdjustment = int.Parse(message.Split(':')[1]); // Parse rating adjustment.
                        if (message.Substring(10, 4) == "LOSE") RatingAdjustment *= -1; // Negate rating adjustment if tie indicates a loss.
                        gameController.GameOver = true; // Mark the game as over.
                        break; // Exit the loop.
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error receiving grid update: " + ex.Message); // Log error receiving update.
                    break; // Exit the loop.
                }
            }
            App.Rating += RatingAdjustment; // Adjust the player's rating based on match outcome.
        }

        // TimerTaskAsync runs the match timer and ends the match when time expires.
        private async Task TimerTaskAsync()
        {
            while (true) // Loop indefinitely.
            {
                TimeSpan elapsedTime = DateTime.UtcNow - matchStartTime; // Calculate elapsed time since match start.
                if (elapsedTime >= matchDuration) // If elapsed time exceeds the match duration...
                {
                    gameController.GameOver = true; // Set game over flag.
                    await Client.SendMessageAsync(App._stream, $"time{gameController.Score}"); // Send time-based message with score.
                    ScoreSent = true; // Mark score as sent.
                    break; // Exit the loop.
                }
                await Task.Delay(1000); // Wait for 1 second before checking again.
            }
        }

        // DrawTimer renders the remaining match time at a specified screen position.
        private void DrawTimer()
        {
            TimeSpan remainingTime = matchDuration - (DateTime.UtcNow - matchStartTime); // Calculate remaining time.
            string timerText = remainingTime.ToString(@"mm\:ss"); // Format remaining time as mm:ss.
            spriteBatch.DrawString(App.titleFont, timerText, Cogs.centreTextPos(App.titleFont, timerText, 960, 50), Color.White); // Draw the timer text.
        }
    }
}
