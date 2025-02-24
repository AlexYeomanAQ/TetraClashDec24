﻿using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;

namespace TetraClashDec24
{
    public class MainGameState : AppState
    {
        private Texture2D[] blockTextures;
        private Texture2D gridTexture;

        private int tileSize = 25;
        private int PlayerGridX;
        private int PlayerGridY;
        private int EnemyGridX;
        private int EnemyGridY;

        private int dropTimer;
        private int dropRate;
        private bool fastDrop;

        private GameState gameState;

        private int MatchID;
        private int[][] enemyGrid;
        private string enemyLevel = "0";
        private string enemyScore = "0";

        private bool _isBlockTaskRunning = false;

        private KeyboardState keyboard;
        private MouseState mouse;

        private KeyboardState prevKeyboardState;
        private ButtonState prevClickState;

        // Store the TcpClient so that its NetworkStream remains valid.
        private TcpClient _client;
        private NetworkStream _stream;

        // Modified constructor: accepts TcpClient instead of a NetworkStream.
        public MainGameState(App app, ButtonState clickState, TcpClient client, int matchID, int seed) : base(app)
        {
            prevClickState = clickState;

            // Keep the client alive for the lifetime of MainGameState.
            _client = client;
            _stream = _client.GetStream();

            blockTextures = new Texture2D[8];

            gameState = new GameState(seed);

            PlayerGridX = 1920 / 4 - (gameState.GameGrid.Collumns * tileSize / 2);
            PlayerGridY = 1080 / 2 - ((gameState.GameGrid.Rows - 2) * tileSize / 2);
            EnemyGridX = 1920 * 3 / 4 - (gameState.GameGrid.Collumns * tileSize / 2);
            EnemyGridY = PlayerGridY;

            dropTimer = 0;
            dropRate = Cogs.getDropRate(gameState.Level);

            MatchID = matchID;

            enemyGrid = new int[22][];
            for (int i = 0; i < 22; i++)
            {
                enemyGrid[i] = new int[10];
            }

            // Start a task to continuously send grid updates.
            _ = Task.Run(() => SendGridUpdatesAsync(_stream));

            // Listen for incoming grid updates from the opponent.
            _ = Task.Run(() => ListenForGridUpdatesAsync(_stream));
        }

        public override void LoadContent()
        {
            for (int i = 0; i < blockTextures.Length+1; i++)
            {
                blockTextures[i] = App.Content.Load<Texture2D>(@$"{i}");
            }

            gridTexture = App.Content.Load<Texture2D>(@"base");
        }

        public override void Update(GameTime gameTime)
        {
            prevKeyboardState = keyboard;
            mouse = Mouse.GetState();
            keyboard = Keyboard.GetState();

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
                        gameState.MoveBlockLeft();
                    }
                    else if (key == Keys.Right)
                    {
                        gameState.MoveBlockRight();
                    }
                    else if (key == Keys.Up)
                    {
                        gameState.DropBlock();
                    }
                    else if (key == Keys.Z)
                    {
                        gameState.RotateBlockCCW();
                    }
                    else if (key == Keys.X)
                    {
                        gameState.RotateBlockCW();
                    }
                }
            }

            fastDrop = fastSwitch;

            if (!_isBlockTaskRunning)
            {
                DropBlock();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(App.GraphicsDevice);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            DrawGrid(spriteBatch, gameState.GameGrid.grid, PlayerGridX, PlayerGridY);
            DrawGrid(spriteBatch, enemyGrid, EnemyGridX, EnemyGridY);
            DrawBlock(spriteBatch, gameState.CurrentBlock, PlayerGridX, PlayerGridY);
            DrawGhostBlock(spriteBatch, gameState.CurrentBlock, PlayerGridX, PlayerGridY);
            spriteBatch.DrawString(App.titleFont, "Level", Cogs.centreTextPos(App.titleFont, "Level", 960, 520), Color.White);
            spriteBatch.DrawString(App.titleFont, gameState.Level.ToString(), Cogs.centreTextPos(App.font, gameState.Level.ToString(), 960, 580), Color.White);
            spriteBatch.DrawString(App.titleFont, "Lines Cleared", Cogs.centreTextPos(App.titleFont, "Lines Cleared", 960, 640), Color.White);
            spriteBatch.DrawString(App.titleFont, gameState.TotalLinesCleared.ToString(), Cogs.centreTextPos(App.font, gameState.TotalLinesCleared.ToString(), 960, 700), Color.White);
            spriteBatch.DrawString(App.titleFont, "Score", Cogs.centreTextPos(App.titleFont, "Score", 960, 760), Color.White);
            spriteBatch.DrawString(App.titleFont, gameState.Score.ToString(), Cogs.centreTextPos(App.font, gameState.Score.ToString(), 960, 820), Color.White);
            spriteBatch.DrawString(App.titleFont, "Level", Cogs.centreTextPos(App.titleFont, "Level", 1650, 520), Color.White);
            spriteBatch.DrawString(App.titleFont, enemyLevel, Cogs.centreTextPos(App.font, enemyLevel, 1650, 580), Color.White);
            spriteBatch.DrawString(App.titleFont, "Score", Cogs.centreTextPos(App.titleFont, "Score", 1650, 640), Color.White);
            spriteBatch.DrawString(App.titleFont, enemyScore, Cogs.centreTextPos(App.font, enemyScore, 1650, 700), Color.White);
            spriteBatch.End();
        }

        private void DrawGrid(SpriteBatch spriteBatch, int[][] grid, int x, int y)
        {
            int Rows = grid.Length;
            int Columns = grid[0].Length; // Assumes at least one row exists.

            for (int r = 2; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    int id = grid[r][c];
                    spriteBatch.Draw(blockTextures[id], new Rectangle(x + (c * tileSize), y + (r * tileSize), tileSize, tileSize), Color.White);
                }
            }
        }

        private async void DropBlock()
        {
            _isBlockTaskRunning = true;

            try
            {
                while (!gameState.GameOver)
                {
                    dropRate = Cogs.getDropRate(gameState.Level);
                    if (fastDrop)
                    {
                        await Task.Delay(dropRate / 2);
                    }
                    else
                    {
                        await Task.Delay(dropRate);
                    }

                    gameState.MoveBlockDown();
                }
            }
            finally
            {
                _isBlockTaskRunning = false;
            }
        }

        private void DrawBlock(SpriteBatch spriteBatch, Block block, int x, int y)
        {
            foreach (Position p in block.TilePositions())
            {
                spriteBatch.Draw(blockTextures[block.Id], new Rectangle(x + (p.Column * tileSize), y + (p.Row * tileSize), tileSize, tileSize), Color.White);
            }
        }

        private void DrawGhostBlock(SpriteBatch spriteBatch, Block block, int x, int y)
        {
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position p in block.TilePositions())
            {
                spriteBatch.Draw(blockTextures[block.Id], new Rectangle(x + (p.Column * tileSize), y + ((p.Row + dropDistance) * tileSize), tileSize, tileSize), new Color(64, 64, 64, 64));
            }
        }

        private async Task SendGridUpdatesAsync(NetworkStream stream)
        {
            while (true)
            {
                //GameState tempGameState = gameState;
                //tempGameState.PlaceBlock();
                string gridJson = JsonSerializer.Serialize(gameState.GameGrid.grid);
                string message = $"match:{gridJson}:{gameState.Level}:{gameState.Score}";
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
                        enemyLevel = args[1];
                        enemyScore = args[2];
                        Console.WriteLine("Received match update.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error deserializing grid data: " + ex.Message);
                    }
                }
            }
        }
    }
}