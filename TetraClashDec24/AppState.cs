using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    public abstract class AppState
    {
        protected App App { get; }

        protected AppState(App app)
        {
            App = app;
        }

        public abstract void LoadContent();

        public virtual void Update(GameTime gameTime)
        {
            RunUpdateAsync(gameTime);
        }

        protected virtual Task RunUpdateAsync(GameTime gameTime) => Task.CompletedTask;

        public abstract void Draw(GameTime gameTime);
    }
}