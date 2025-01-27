using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TetraClashDec24
{
    public class MainGameState : AppState
    {
        private Texture2D[] blockTextures;
        private Texture2D gridTexture;

        private int tileSize;
        private int gridX;
        private int gridY;

        private GameState gameState;

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

            foreach (Keys key in pressedKeys)
            {
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
                    else if (key == Keys.Down)
                    {
                        gameState.MoveBlockDown();
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
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(App.GraphicsDevice);

            spriteBatch.Begin();
            spriteBatch.Draw(gridTexture, new Rectangle(gridX, gridY, gameState.GameGrid.Collumns * tileSize, gameState.GameGrid.Rows * tileSize), Color.Black);
            DrawGrid(spriteBatch, gameState.GameGrid);
            DrawBlock(spriteBatch, gameState.CurrentBlock);

            spriteBatch.End();
        }

        private void DrawGrid(SpriteBatch spriteBatch, GameGrid grid)
        {
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Collumns; c++)
                {
                    int id = grid[r, c];
                    if (id != 0)
                    {
                        spriteBatch.Draw(blockTextures[id - 1], new Rectangle(gridX + (c * tileSize), gridY + (r * tileSize), tileSize, tileSize), Color.White);
                    }
                }
            }
        }

        private void DrawBlock(SpriteBatch spriteBatch, Block block)
        {
            foreach (Position p in block.TilePositions())
            {
                spriteBatch.Draw(blockTextures[block.Id - 1], new Rectangle(gridX + (p.Column * tileSize), gridY + (p.Row * tileSize), tileSize, tileSize), Color.White);
            }
        }
    }
}