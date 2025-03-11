using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace TetraClashDec24
{
    public class MainGameState : AppState
    {
        private SpriteBatch spriteBatch;
        private Texture2D[] tetrominoTextures;
        private Texture2D gridTexture;

        private int tileSize = 25;
        private int PlayerGridX;
        private int PlayerGridY;
        private int EnemyGridX;
        private int EnemyGridY;

        private int dropTimer;
        private int dropRate;
        private bool fastDrop;

        private GameController gameController;

        private long MatchID;

        private string enemyUsername;
        private int[][] enemyGrid;
        private string enemyLevel = "0";
        private string enemyScore = "0";
        private int enemyDropDistance = 0;

        private bool ScoreSent = false;
        private bool hasPlayedResultSound = false;
        private string MatchResult = "";
        private int RatingAdjustment;

        private Button returnToMenu_Button;

        private bool _isTetrominoTaskRunning = false;

        private KeyboardState keyboard;
        private MouseState mouse;

        private KeyboardState prevKeyboardState;
        private ButtonState prevClickState;

        private DateTime matchStartTime;
        private TimeSpan matchDuration = TimeSpan.FromMinutes(1);

        // Store the TcpClient so that its NetworkStream remains valid.

        // Modified constructor: accepts TcpClient instead of a NetworkStream.
        public MainGameState(App app, ButtonState clickState, long matchID, int seed, string enemy_username) : base(app)
        {
            prevClickState = clickState;

            spriteBatch = new SpriteBatch(App.GraphicsDevice);

            tetrominoTextures = new Texture2D[8];

            gameController = new GameController(seed);

            gameController.BlockLanded += OnBlockLanded;
            gameController.LinesCleared += OnLinesCleared;

            PlayerGridX = 1920 / 4 - (gameController.GameBoard.Collumns * tileSize / 2) - 200;
            PlayerGridY = 1080 / 2 - ((gameController.GameBoard.Rows - 2) * tileSize / 2);
            enemyUsername = enemy_username;
            EnemyGridX = 1920 * 3 / 4 - (gameController.GameBoard.Collumns * tileSize / 2) - 200;
            EnemyGridY = PlayerGridY;

            dropTimer = 0;
            dropRate = Cogs.getDropRate(gameController.Level);

            MatchID = matchID;
            matchStartTime = DateTimeOffset.FromUnixTimeSeconds(MatchID).UtcDateTime;
            Console.WriteLine(matchStartTime);

            enemyGrid = new int[22][];
            for (int i = 0; i < 22; i++)
            {
                enemyGrid[i] = new int[10];
            }

            returnToMenu_Button = new Button(App, 860, 700, 200, 100, Color.White, "Return to Menu");

            // Start a task to continuously send grid updates.
            _ = Task.Run(() => SendGridUpdatesAsync(App._stream));

            // Listen for incoming grid updates from the opponent.
            _ = Task.Run(() => ListenForGridUpdatesAsync(App._stream));

            // Start the match timer
            _ = Task.Run(() => TimerTaskAsync());
        }

        public override void LoadContent()
        {
            for (int i = 0; i < tetrominoTextures.Length + 1; i++)
            {
                tetrominoTextures[i] = App.Content.Load<Texture2D>(@$"{i}");
            }

            MediaPlayer.IsRepeating = true; // Loops the music
            MediaPlayer.Volume = 0.5f;
        }

        public override void Update(GameTime gameTime)
        {
            prevKeyboardState = keyboard;
            mouse = Mouse.GetState();
            keyboard = Keyboard.GetState();

            if (MediaPlayer.State != MediaState.Playing && App.gameMusic != null)
                MediaPlayer.Play(App.gameMusic);

            if (MatchResult == "")
            {
                Keys[] pressedKeys = keyboard.GetPressedKeys();

                bool fastSwitch = false;

                foreach (Keys key in pressedKeys)
                {
                    if (key == Keys.Down)
                    {
                        fastSwitch = true;
                    }
                    else if (key != Keys.Down && !fastSwitch)
                    {
                        fastSwitch = false;
                    }

                    if (prevKeyboardState.IsKeyUp(key))
                    {
                        if (key == Keys.Left)
                        {
                            gameController.TranslateTetrominoLeft();
                        }
                        else if (key == Keys.Right)
                        {
                            gameController.TranslateTetrominoRight();
                        }
                        else if (key == Keys.Up)
                        {
                            gameController.DropTetromino();
                        }
                        else if (key == Keys.Z)
                        {
                            gameController.RotateTetrominoCCW();
                        }
                        else if (key == Keys.X)
                        {
                            gameController.RotateTetrominoCW();
                        }
                        else if (key == Keys.C)
                        {
                            gameController.HoldTetromino();
                        }
                    }
                }

                fastDrop = fastSwitch;

                if (!_isTetrominoTaskRunning && MatchResult == "")
                {
                    DropTetromino();
                }
            }
            else
            {
                MediaPlayer.Stop();
                if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
                {
                    Point mousePosition = new Point(mouse.X, mouse.Y);
                    if (returnToMenu_Button.Box.Contains(mousePosition))
                    {
                        App.ChangeState(new MainMenuState(App, mouse.LeftButton));
                    }
                }
            }
            prevClickState = mouse.LeftButton;
        }

        private void OnBlockLanded(object sender, EventArgs e)
        {
            App.sound_TetrominoLand.Play();
        }

        private void OnLinesCleared(object sender, int numLines)
        {
            App.sound_LineClear.Play();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Player side
            DrawGrid(gameController.GameBoard.grid, PlayerGridX, PlayerGridY);
            DrawTetromino(gameController.CurrentTetromino, PlayerGridX, PlayerGridY);
            DrawGhostTetromino(gameController.CurrentTetromino, PlayerGridX, PlayerGridY);
            DrawPreviewTetromino(gameController.TetrominoQueue);

            if (gameController.HeldTetromino != null)
            {
                DrawHeldTetromino();
            }
            // Enemy side
            DrawGrid(enemyGrid, EnemyGridX, EnemyGridY);



            spriteBatch.DrawString(App.titleFont, "Level", Cogs.centreTextPos(App.titleFont, "Level", 960, 520), Color.White);
            spriteBatch.DrawString(App.titleFont, gameController.Level.ToString(), Cogs.centreTextPos(App.font, gameController.Level.ToString(), 960, 580), Color.White);
            spriteBatch.DrawString(App.titleFont, "Lines Cleared", Cogs.centreTextPos(App.titleFont, "Lines Cleared", 960, 640), Color.White);
            spriteBatch.DrawString(App.titleFont, gameController.TotalLinesCleared.ToString(), Cogs.centreTextPos(App.font, gameController.TotalLinesCleared.ToString(), 960, 700), Color.White);
            spriteBatch.DrawString(App.titleFont, "Score", Cogs.centreTextPos(App.titleFont, "Score", 960, 760), Color.White);
            spriteBatch.DrawString(App.titleFont, gameController.Score.ToString(), Cogs.centreTextPos(App.font, gameController.Score.ToString(), 960, 820), Color.White);
            spriteBatch.DrawString(App.titleFont, enemyUsername, Cogs.centreTextPos(App.titleFont, enemyUsername, EnemyGridX+75, EnemyGridY - 30), Color.White);
            spriteBatch.DrawString(App.titleFont, "Level", Cogs.centreTextPos(App.titleFont, "Level", 1650, 520), Color.White);
            spriteBatch.DrawString(App.titleFont, enemyLevel, Cogs.centreTextPos(App.font, enemyLevel, 1650, 580), Color.White);
            spriteBatch.DrawString(App.titleFont, "Score", Cogs.centreTextPos(App.titleFont, "Score", 1650, 640), Color.White);
            spriteBatch.DrawString(App.titleFont, enemyScore, Cogs.centreTextPos(App.font, enemyScore, 1650, 700), Color.White);

            if (gameController.GameOver)
            {
                spriteBatch.Draw(App.baseTexture, new Rectangle(0, 0, 1920, 1080), new Color(64, 64, 64, 64));
                spriteBatch.DrawString(App.titleFont, MatchResult, Cogs.centreTextPos(App.titleFont, MatchResult, 960, 540), Color.Black);
                if (RatingAdjustment != null)
                {
                    if (!hasPlayedResultSound)
                    {
                        if (RatingAdjustment >= 0)
                        {
                            App.sound_Win.Play(0.5f, 0, 0);
                        }
                        else
                        {
                            App.sound_Lose.Play(0.5f, 0, 0);
                        }
                        hasPlayedResultSound = true;
                    }
                    string ratingAdjustmentString = $"Rating Change: {RatingAdjustment}";
                    spriteBatch.DrawString(App.titleFont, ratingAdjustmentString, Cogs.centreTextPos(App.titleFont, ratingAdjustmentString, 960, 640), Color.Black);
                }
                returnToMenu_Button.Draw(spriteBatch);
            }
            else
            {
                DrawTimer();
            }

            spriteBatch.End();
        }

        private void DrawGrid(int[][] grid, int x, int y)
        {
            int Rows = grid.Length;
            int Columns = grid[0].Length; // Assumes at least one row exists.

            for (int r = 2; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    int id = grid[r][c];
                    spriteBatch.Draw(tetrominoTextures[id], new Rectangle(x + (c * tileSize), y + (r * tileSize), tileSize, tileSize), Color.White);
                }
            }
        }

        private async void DropTetromino()
        {
            _isTetrominoTaskRunning = true;

            try
            {
                while (!gameController.GameOver)
                {
                    dropRate = Cogs.getDropRate(gameController.Level);
                    if (fastDrop)
                    {
                        await Task.Delay(Math.Min(100, dropRate));
                    }
                    else
                    {
                        await Task.Delay(dropRate);
                    }
                    gameController.TranslateTetrominoDown();
                }
            }
            finally
            {
                _isTetrominoTaskRunning = false;
                await HandleGameOver();
            }
        }

        private void DrawTetromino(Tetromino tetromino, int x, int y, bool ignore_offset = false)
        {
            if (tetromino == null) return;

            foreach (Position p in tetromino.BlockPositions(ignore_offset))
            {
                if (p.Row > 1 || ignore_offset)
                {
                    spriteBatch.Draw(tetrominoTextures[tetromino.TetrominoID], new Rectangle(x + (p.Column * tileSize), y + (p.Row * tileSize), tileSize, tileSize), Color.White);
                }
            }
        }

        private void DrawGhostTetromino(Tetromino tetromino, int x, int y)
        {
            if (tetromino == null) return;

            int dropDistance = gameController.TetrominoDropDistance();

            foreach (Position p in tetromino.BlockPositions())
            {
                if (p.Row + dropDistance > 1)
                {
                    spriteBatch.Draw(tetrominoTextures[tetromino.TetrominoID], new Rectangle(x + (p.Column * tileSize), y + ((p.Row + dropDistance) * tileSize), tileSize, tileSize), new Color(64, 64, 64, 64));
                }
            }
        }

        private void DrawPreviewTetromino(TetrominoQueue tetrominoQueue)
        {
            Tetromino next = tetrominoQueue.NextTetromino;
            spriteBatch.DrawString(App.titleFont, "Next Tetromino", new Vector2(640, 500), Color.White);
            spriteBatch.Draw(App.baseTexture, new Rectangle(640, 530, 100, 100), Color.Black);
            DrawTetromino(next, 650, 540, true);
        }

        private void DrawHeldTetromino()
        {
            if (gameController.HeldTetromino == null) return;
            DrawTetromino(gameController.HeldTetromino, 650, 740, true);
        }

        private async Task HandleGameOver()
        {
            if (MatchResult == "")
            {
                string message = $"lose{gameController.Score}";
                await Client.SendMessageAsync(App._stream, message);
            }
        }

        private int[][] AddFallingBlockToGrid(int[][] grid, Tetromino tetromino)
        {
            // Create a deep copy of the grid
            int[][] tempGrid = new int[grid.Length][];
            for (int i = 0; i < grid.Length; i++)
            {
                tempGrid[i] = new int[grid[i].Length];
                Array.Copy(grid[i], tempGrid[i], grid[i].Length);
            }

            // Add the tetromino to the copy
            if (tetromino != null)
            {
                foreach (Position p in tetromino.BlockPositions())
                {
                    tempGrid[p.Row][p.Column] = tetromino.TetrominoID;
                }
            }
            return tempGrid;
        }

        private async Task SendGridUpdatesAsync(NetworkStream stream)
        {
            while (true)
            {
                try
                {
                    // Create a deep copy of the grid with falling blocks
                    int[][] gridWithFallingBlocks = AddFallingBlockToGrid(gameController.GameBoard.grid, gameController.CurrentTetromino);

                    // Serialize the simplified data
                    string gridJson = JsonSerializer.Serialize(gridWithFallingBlocks);
                    Console.WriteLine(gridJson);

                    // Combine all data into a single message
                    string message = $"match:{gridJson}:{gameController.Level}:{gameController.Score}";
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                    await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                    Console.WriteLine("Sent grid update");

                    await Task.Delay(dropRate);

                    if (gameController.GameOver)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending grid update: " + ex.Message);
                    break;
                }
            }
        }

        // Listens for grid updates from the opponent and prints them.
        private async Task ListenForGridUpdatesAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[4096];
            while (true)
            {
                int bytesRead = 0;
                try
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Disconnected from server.");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    if (message.StartsWith("GRID_UPDATE:"))
                    {
                        string[] args = message.Substring("GRID_UPDATE:".Length).Split(':');
                        try
                        {
                                enemyGrid = JsonSerializer.Deserialize<int[][]>(args[0]);
                                enemyLevel = args[1];
                                enemyScore = args[2];
                                Console.WriteLine("Received match update.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error deserializing grid data: " + ex.Message);
                        }
                    }
                    else if (message.StartsWith("MATCH_WIN"))
                    {
                        RatingAdjustment = int.Parse(message.Split(':')[1]);
                        MatchResult = "You win!";
                        gameController.GameOver = true;
                        if (!ScoreSent)
                        {
                            await Client.SendMessageAsync(stream, $"score{gameController.Score}");
                            ScoreSent = true;
                        }

                        break;
                    }
                    else if (message.StartsWith("MATCH_LOSE"))
                    {
                        MatchResult = "You lose!";
                        RatingAdjustment = int.Parse(message.Split(':')[1]) * -1;
                        break;
                    }
                    else if (message.StartsWith("MATCH_TIE"))
                    {
                        MatchResult = "Tied!";
                        RatingAdjustment = int.Parse(message.Split(':')[1]);
                        if (message.Substring(10, 4) == "LOSE")
                        {
                            RatingAdjustment *= -1;
                        }
                        gameController.GameOver = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error receiving grid update: " + ex.Message);
                    break;
                }
            }
            App.Rating += RatingAdjustment;
        }

        private async Task TimerTaskAsync()
        {
            while (true)
            {
                TimeSpan elapsedTime = DateTime.UtcNow - matchStartTime;
                Console.WriteLine(elapsedTime.TotalSeconds);
                if (elapsedTime >= matchDuration)
                {
                    Console.WriteLine("Test");
                    gameController.GameOver = true;
                    await Client.SendMessageAsync(App._stream, $"time{gameController.Score}");
                    ScoreSent = true;
                    break;
                }
                await Task.Delay(1000);
            }
        }

        private void DrawTimer()
        {
            TimeSpan remainingTime = matchDuration - (DateTime.UtcNow - matchStartTime);
            string timerText = remainingTime.ToString(@"mm\:ss");
            spriteBatch.DrawString(App.titleFont, timerText, Cogs.centreTextPos(App.titleFont, timerText, 960, 50), Color.White);
        }
    }
}