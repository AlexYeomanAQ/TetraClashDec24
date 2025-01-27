using Microsoft.Xna.Framework;

namespace TetraClashDec24
{
    public abstract class AppState
    {
        protected App App;

        public AppState(App app)
        {
            App = app;
        }

        public abstract void LoadContent();
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
    }
}
