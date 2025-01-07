using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using TetraClashDec24;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System;

public class CreateAccountState : GameState
{

    private InputButton usernameBox;
    private string UBDefaultString = "Enter Username";
    private InputButton passwordBox;
    private string PBDefaultString = "Enter Password";
    private Button submitButton;

    private string username = "";
    private string password = "";


    private SpriteFont font;
    private string inputTexturePath = @"base";

    private KeyboardState keyboard;
    private MouseState mouse;

    private KeyboardState prevKeyboardState;
    ButtonState prevClickState;

    bool isCapsLockOn = false;
    private enum InputField {None, Username, Password}
    private InputField focusedField = InputField.None; 

    public CreateAccountState(Game1 game, ButtonState clickState) : base(game)
    {
        prevClickState = clickState;
    }

    public override void LoadContent()
    {
        usernameBox = new InputButton(inputTexturePath, 380, 490, 200, 100, Color.White, UBDefaultString);
        usernameBox.LoadContent(Game.Content);

        passwordBox = new InputButton(inputTexturePath, 1340, 490, 200, 100, Color.White, UBDefaultString);
        passwordBox.LoadContent(Game.Content);

        submitButton = new Button(inputTexturePath, 860, 760, 200, 100, Color.White, "Submit!");
        submitButton.LoadContent(Game.Content);
    }


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
            username = HandleInput(username, keyboard, prevKeyboardState, ref isCapsLockOn);
            usernameBox.Text = username;
            usernameBox.highlighted = true;
        }
        else if (focusedField == InputField.Password)
        {
            password = HandleInput(password, keyboard, prevKeyboardState, ref isCapsLockOn, true);
            passwordBox.Text = new string('*', password.Length);
            passwordBox.highlighted = true;
        }
        if (focusedField != InputField.Username)
        {
            usernameBox.highlighted = false;
            if (username == "")
            {
                usernameBox.Text = UBDefaultString;
            }
        }
        if (focusedField != InputField.Password)
        {
            passwordBox.highlighted = false;
            if (password == "")
            {
                passwordBox.Text = UBDefaultString;
            }
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

    private string HandleInput(string currentText, KeyboardState keyboard, KeyboardState prevKeyboardState, ref bool isCapsLockOn, bool takeSpecialCharacters = false)
    {
        Keys[] pressedKeys = keyboard.GetPressedKeys();
        bool isShiftPressed = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

        foreach (Keys key in pressedKeys)
        {
            // Process only if the key was not pressed in the previous state
            if (prevKeyboardState.IsKeyUp(key))
            {
                if (key == Keys.Back && currentText.Length > 0)
                {
                    // Handle backspace
                    currentText = currentText.Remove(currentText.Length - 1);
                }
                else if (key == Keys.Space)
                {
                    // Handle space
                    currentText += " ";
                }
                else if (key == Keys.CapsLock)
                {
                    // Toggle Caps Lock state
                    Console.WriteLine(isCapsLockOn);
                    isCapsLockOn = !isCapsLockOn;
                }
                else if (key >= Keys.A && key <= Keys.Z)
                {
                    // Handle upper and lower case alphabet characters
                    char letter = key.ToString()[0];
                    if (isShiftPressed ^ isCapsLockOn)
                    {
                        letter = char.ToUpper(letter);
                    }
                    else
                    {
                        letter = char.ToLower(letter);
                    }
                    currentText += letter;
                }
                else if (key >= Keys.D0 && key <= Keys.D9)
                {
                    // Handle numbers and special characters
                    if (isShiftPressed && takeSpecialCharacters)
                    {
                        string shiftedNumbers = ")!@#$%^&*(";
                        currentText += shiftedNumbers[key - Keys.D0];
                    }
                    else
                    {
                        currentText += (char)('0' + (key - Keys.D0));
                    }
                }
                else if (key == Keys.Enter)
                {

                }
            }
        }
        return currentText;
    }
}
