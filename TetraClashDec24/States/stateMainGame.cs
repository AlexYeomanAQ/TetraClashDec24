﻿using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading;

namespace TetraClashDec24
{
    public class MainGameState : AppState
    {
        private Texture2D[] blockTextures;
        private Texture2D gridTexture;

        private int tileSize;
        private int gridX;
        private int gridY;
        private int EnemyGridX;
        private int EnemyGridY;

        private int dropTimer;
        private int dropRate;
        private bool fastDrop;

        private GameState gameState;
        private GameGrid enemyGameGrid;

        private KeyboardState keyboard;
        private MouseState mouse;

        private KeyboardState prevKeyboardState;
        private ButtonState prevClickState;

        public MainGameState(App app, ButtonState clickState) : base(app)
        {
            prevClickState = clickState;

            blockTextures = new Texture2D[7];
            gameState = new GameState();

            tileSize = 25;
            gridX = 1920 / 2 - (gameState.GameGrid.Collumns * tileSize / 2);
            gridY = 1080 / 2 - ((gameState.GameGrid.Rows - 2) * tileSize / 2);

            dropTimer = 0;
            dropRate = 500;
        }

        public override void LoadContent()
        {
            for (int i = 0; i < blockTextures.Length; i++)
            {
                blockTextures[i] = App.Content.Load<Texture2D>(@$"{i + 1}");
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
                    Console.WriteLine("test");
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

            dropTimer += gameTime.ElapsedGameTime.Milliseconds;

            if (fastDrop)
            {
                if (dropTimer >= dropRate/10)
                {
                    dropTimer = 0;
                    gameState.MoveBlockDown();
                }
            }
            else
            {
                if (dropTimer >= dropRate)
                {
                    dropTimer = 0;
                    gameState.MoveBlockDown();
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(App.GraphicsDevice);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.Draw(gridTexture, new Rectangle(gridX, gridY, gameState.GameGrid.Collumns * tileSize, gameState.GameGrid.Rows * tileSize), Color.Black);
            DrawGrid(spriteBatch, gameState.GameGrid, gridX, gridY);
            DrawBlock(spriteBatch, gameState.CurrentBlock);
            DrawGhostBlock(spriteBatch, gameState.CurrentBlock);

            spriteBatch.End();
        }

        private void DrawGrid(SpriteBatch spriteBatch, GameGrid grid, int x, int y)
        {
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Collumns; c++)
                {
                    int id = grid[r, c];
                    if (id != 0)
                    {
                        spriteBatch.Draw(blockTextures[id - 1], new Rectangle(x + (c * tileSize), y + (r * tileSize), tileSize, tileSize), Color.White);
                    }
                }
            }
        }

        private void DrawBlock(SpriteBatch spriteBatch, Block block, int x, int y)
        {
            foreach (Position p in block.TilePositions())
            {
                spriteBatch.Draw(blockTextures[block.Id - 1], new Rectangle(x + (p.Column * tileSize), y + (p.Row * tileSize), tileSize, tileSize), Color.White);
            }
        }

        private void DrawGhostBlock(SpriteBatch spriteBatch, Block block)
        {
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position p in block.TilePositions())
            {
                spriteBatch.Draw(blockTextures[block.Id - 1], new Rectangle(gridX + (p.Column * tileSize), gridY + ((p.Row + dropDistance) * tileSize), tileSize, tileSize), new Color(255, 255, 255, 64));
            }
        }
    }
}