using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    public class MainGameState : AppState
    {
        private Texture2D[] blockTextures;
        private Texture2D gridTexture;

        private int tileSize;
        private int PlayerGridX;
        private int PlayerGridY;
        private int EnemyGridX;
        private int EnemyGridY;

        private int dropTimer;
        private int dropRate;
        private bool fastDrop;
        private bool _isBlockTaskRunning;

        private GameState gameState;

        private GameGrid enemyGameGrid;
        private bool _isFetchTaskRunning;

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
            PlayerGridX = 1920 / 4 - (gameState.GameGrid.Collumns * tileSize / 2);
            PlayerGridY = 1080 / 2 - ((gameState.GameGrid.Rows - 2) * tileSize / 2);
            EnemyGridX = 1920 * 3 / 4 - (gameState.GameGrid.Collumns * tileSize / 2);
            EnemyGridY = PlayerGridY;

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

            //if (!_isFetchTaskRunning)
            //{
            //    enemyGameGrid = fetchEnemyGameGrid()
            //}
        }

        private async void DropBlock()
        {
            _isBlockTaskRunning = true;

            try
            {
                while (!gameState.GameOver)
                {
                    if (fastDrop)
                    {
                        await Task.Delay(dropRate / 10);
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



        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(App.GraphicsDevice);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.Draw(gridTexture, new Rectangle(PlayerGridX, PlayerGridY, gameState.GameGrid.Collumns * tileSize, gameState.GameGrid.Rows * tileSize), Color.Black);
            DrawGrid(spriteBatch, gameState.GameGrid, PlayerGridX, PlayerGridY);
            DrawGrid(spriteBatch, gameState.GameGrid, EnemyGridX, EnemyGridY);
            DrawBlock(spriteBatch, gameState.CurrentBlock, PlayerGridX, PlayerGridY);
            DrawGhostBlock(spriteBatch, gameState.CurrentBlock, PlayerGridX, PlayerGridY);
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

        private void DrawGhostBlock(SpriteBatch spriteBatch, Block block, int x, int y)
        {
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position p in block.TilePositions())
            {
                spriteBatch.Draw(blockTextures[block.Id - 1], new Rectangle(x + (p.Column * tileSize), y + ((p.Row + dropDistance) * tileSize), tileSize, tileSize), new Color(64, 64, 64, 64));
            }
        }
    }
}
