using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace TetraClashDec24
{
    class GameObject
    {
        public Vector2 startLocation;
        protected Texture2D baseTexture;
        public SpriteFont baseFont;
        public virtual void LoadContent(ContentManager Content)
        {
        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Draw(SpriteBatch SpriteBatch)
        {
        }
    }
}
