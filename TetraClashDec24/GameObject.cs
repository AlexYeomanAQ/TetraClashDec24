using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace TetraClashDec24
{
    class GameObject
    {
        public Vector2 Location;
        protected Texture2D Texture;
        public SpriteFont GameFont;
        public virtual void LoadContent(ContentManager Content)
        {
        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Draw(SpriteBatch SpriteBatch)
        {
            SpriteBatch.Draw(Texture, Location, Color.White);
        }
    }
}
