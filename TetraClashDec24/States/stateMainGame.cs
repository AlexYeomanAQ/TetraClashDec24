using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;

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
        private Tetromino enemyFallingTetromino;


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

        }

        public override void Update(GameTime gameTime)
        {
            prevKeyboardState = keyboard;
            mouse = Mouse.GetState();
            keyboard = Keyboard.GetState();

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

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            DrawGrid(gameController.GameBoard.grid, PlayerGridX, PlayerGridY);
            DrawGrid(enemyGrid, EnemyGridX, EnemyGridY);
            DrawTetromino(gameController.CurrentTetromino, PlayerGridX, PlayerGridY);
            DrawGhostTetromino(gameController.CurrentTetromino, PlayerGridX, PlayerGridY);
            DrawPreviewTetromino(gameController.TetrominoQueue);

            spriteBatch.DrawString(App.titleFont, "Next Tetromino", new Vector2(640, 700), Color.White);
            spriteBatch.Draw(App.baseTexture, new Rectangle(640, 730, 100, 100), Color.Black);
            if (gameController.HeldTetromino != null)
            {
                DrawHeldTetromino();
            }

            spriteBatch.DrawString(App.titleFont, "Level", Cogs.centreTextPos(App.titleFont, "Level", 960, 520), Color.White);
            spriteBatch.DrawString(App.titleFont, gameController.Level.ToString(), Cogs.centreTextPos(App.font, gameController.Level.ToString(), 960, 580), Color.White);
            spriteBatch.DrawString(App.titleFont, "Lines Cleared", Cogs.centreTextPos(App.titleFont, "Lines Cleared", 960, 640), Color.White);
            spriteBatch.DrawString(App.titleFont, gameController.TotalLinesCleared.ToString(), Cogs.centreTextPos(App.font, gameController.TotalLinesCleared.ToString(), 960, 700), Color.White);
            spriteBatch.DrawString(App.titleFont, "Score", Cogs.centreTextPos(App.titleFont, "Score", 960, 760), Color.White);
            spriteBatch.DrawString(App.titleFont, gameController.Score.ToString(), Cogs.centreTextPos(App.font, gameController.Score.ToString(), 960, 820), Color.White);
            spriteBatch.DrawString(App.titleFont, enemyUsername, Cogs.centreTextPos(App.titleFont, enemyUsername, EnemyGridX, EnemyGridY - 100), Color.White);
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

        private async Task SendGridUpdatesAsync(NetworkStream stream)
        {
            while (true)
            {
                string gridJson = JsonSerializer.Serialize(gameController.GameBoard.grid);
                string fallingTetrominoJson = JsonSerializer.Serialize(gameController.CurrentTetromino);
                string previewTetrominoJson = JsonSerializer.Serialize(gameController.TetrominoQueue.NextTetromino);
                string holdTetrominoJson = JsonSerializer.Serialize(gameController.HeldTetromino);
                string message = $"match:{gridJson}:{fallingTetrominoJson}:{previewTetrominoJson}:{holdTetrominoJson}:{gameController.Level}:{gameController.Score}";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                try
                {
                    await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                    Console.WriteLine("Sent grid update");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending grid update: " + ex.Message);
                    break;
                }
                await Task.Delay(dropRate); // Delay before sending the next update.
                if (gameController.GameOver)
                {
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error receiving grid update: " + ex.Message);
                    break;
                }
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
                        enemyFallingTetromino = JsonSerializer.Deserialize<Tetromino>(args[1]);
                        enemyPreviewTetromino = JsonSerializer.Deserialize<Tetromino>(args[2]);
                        enemyHeldTetromino = JsonSerializer.Deserialize<Tetromino>(args[3]);
                        enemyLevel = args[4];
                        enemyScore = args[5];
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
                    await Client.SendMessageAsync(stream, $"score{gameController.Score}");
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