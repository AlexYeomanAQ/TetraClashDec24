using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using TetraClashDec24;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System;

public class CreateAccountState : GameState
{

    private InputButton usernameBox;
    private InputButton passwordBox;
    private Button submitButton;

    private string username = "";
    private string password = "";

    private SpriteFont font;
    private string inputTexturePath = @"base";

    private KeyboardState keyboard;
    private MouseState mouse;

    private KeyboardState currentKeyboardState;
    private KeyboardState prevKeyboardState;
    ButtonState prevClickState;

    public CreateAccountState(Game1 game, ButtonState clickState) : base(game)
    {
        prevClickState = clickState;
    }

    public override void LoadContent()
    {
        usernameBox = new InputButton(inputTexturePath, 380, 490, 200, 100, Color.White);
        usernameBox.LoadContent(Game.Content);

        passwordBox = new InputButton(inputTexturePath, 1340, 490, 200, 100, Color.White);
        passwordBox.LoadContent(Game.Content);

        submitButton = new Button(inputTexturePath, 860, 760, 200, 100, Color.White, "Submit!");
        submitButton.LoadContent(Game.Content);
    }

    private enum InputField { None, Username, Password}
    private InputField focusedField = InputField.None;
    public override void Update(GameTime gameTime)
    {
        prevKeyboardState = keyboard;
        mouse = Mouse.GetState();
        keyboard = Keyboard.GetState();
        
        if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
        {
            Point mousePosition = new Point(mouse.X, mouse.Y);
            if (usernameBox.Box.Contains(mousePosition))
            {
                focusedField = InputField.Username;
            }
            else if (passwordBox.Box.Contains(mousePosition))
            {
                focusedField = InputField.Password;
            }
            else if (submitButton.Box.Contains(mousePosition))
            {
                Game.ChangeState(new MainMenuState(Game, mouse.LeftButton));
            }
            else
            {
                focusedField = InputField.None;
            }
        }

        if (focusedField == InputField.Username)
        { 
            username = HandleInput(username, keyboard);
            usernameBox.Text = username;
            usernameBox.highlighted = true;
        }
        else if (focusedField == InputField.Password)
        {
            password = HandleInput(password, keyboard);
            passwordBox.Text = new string('*', password.Length);
            passwordBox.highlighted = true;
        }
        if (focusedField != InputField.Username)
        {
            usernameBox.highlighted = false;
        }
        if (focusedField != InputField.Password)
        {
            passwordBox.highlighted = false;
        }
        prevClickState = mouse.LeftButton;
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.CornflowerBlue);
        SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        spriteBatch.Begin();
        usernameBox.Draw(spriteBatch);
        passwordBox.Draw(spriteBatch);
        submitButton.Draw(spriteBatch);
        spriteBatch.End();
    }

    private string HandleInput(string currentText, KeyboardState keyboard)
    {
        Keys[] pressedKeys = keyboard.GetPressedKeys();
        foreach (Keys key in pressedKeys)
        {
            if (prevKeyboardState.IsKeyUp(key))
            {
                if (key == Keys.Back && currentText.Length > 0)
                {
                    currentText = currentText.Remove(currentText.Length - 1);
                }
                else if (key == Keys.Space)
                {
                    currentText += " ";
                }
                else if (key >= Keys.A && key <= Keys.Z)
                {
                    currentText += key.ToString();
                }
                else if (key >= Keys.D0 && key <= Keys.D9)
                {
                    currentText += (char)('0' + (key - Keys.D0));
                }
            }
            
        }
        
        return currentText;
    }
}
