using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    // Abstract base class representing a state or screen in the application.
    public abstract class AppState
    {
        // Provides derived states with access to the main application.
        protected App App { get; }

        // Constructor accepting the main application instance.
        protected AppState(App app)
        {
            App = app;
        }

        // Abstract method for loading content specific to the state.
        // Must be implemented by derived classes to load textures, sounds, etc.
        public abstract void LoadContent();

        // Virtual update method called every frame.
        // It can be overridden by derived classes, but by default it calls the asynchronous update method.
        public virtual void Update(GameTime gameTime)
        {
            // Trigger the asynchronous update routine.
            RunUpdateAsync(gameTime);
        }

        // Protected virtual method for asynchronous update logic.
        // Derived classes can override this to perform asynchronous operations during updates.
        // By default, it returns a completed task.
        protected virtual Task RunUpdateAsync(GameTime gameTime) => Task.CompletedTask;

        // Abstract method for drawing the state.
        // Derived classes must implement their own rendering logic.
        public abstract void Draw(GameTime gameTime);
    }
}
