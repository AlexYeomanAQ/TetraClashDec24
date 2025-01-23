using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using TetraClashDec24;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System;
using System.Net.Cache;
using System.Net.Security;
using SharpDX.Direct3D9;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using SharpDX.Direct2D1.Effects;

public class CreateAccountState : AppState
{

    private InputButton usernameBox;
    private InputButton passwordBox;

    private Button submitButton;

    private Texture2D titleTexture;

    private SpriteFont font;
    private SpriteFont titleFont;

    private string ErrorString = "";
    private string UBDefaultString = "Enter Username";
    private string PBDefaultString = "Enter Password";

    private string username = "";
    private string password = "";

    private string basePath = @"base";

    private KeyboardState keyboard;
    private MouseState mouse;

    private KeyboardState prevKeyboardState;
    ButtonState prevClickState;

    bool isCapsLockOn = false;
    private enum InputField {None, Username, Password}
    private InputField focusedField = InputField.None; 

    public CreateAccountState(App1 app, ButtonState clickState) : base(app)
    {
        prevClickState = clickState;
    }

    public override void LoadContent()
    {
        usernameBox = new InputButton(basePath, 835, 600, 250, 50, Color.White, UBDefaultString);
        usernameBox.LoadContent(App.Content);

        passwordBox = new InputButton(basePath, 835, 700, 250, 50, Color.White, PBDefaultString);
        passwordBox.LoadContent(App.Content);

        submitButton = new Button(basePath, 885, 800, 150, 100, Color.White, "Submit!");
        submitButton.LoadContent(App.Content);

        titleTexture = App.Content.Load<Texture2D>(@"tempLogo");

        font = App.Content.Load<SpriteFont>(@"myFont");
        titleFont = App.Content.Load<SpriteFont>(@"titleFont");
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
                string salt = Security.GenerateSalt();
                string hash = Security.GenerateHash(password, salt);
                string message = $"create{username}:{hash}:{salt}";
                string response = Client.sendMessage(message);

                if (response == "Success")
                {
                    using (StreamWriter writer = new StreamWriter("cache.txt"))
                    {
                        writer.Write(username);
                    }
                    App.Username = username;
                    App.ChangeState(new MainMenuState(App, mouse.LeftButton));
                }
                else if (response == "Player Exists")
                {
                    ErrorString = "Username already exists, please select a new one or log in.";
                }
                else
                {
                    ErrorString = $"Unknown Error: {response}";
                }
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
                passwordBox.Text = PBDefaultString;
            }
        }
        prevClickState = mouse.LeftButton;
    }

    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        spriteBatch.Begin();

        spriteBatch.DrawString(titleFont, "Create an Account", Cogs.centreTextPos(titleFont, "Create an Account", 960, 475), Color.White);

        spriteBatch.Draw(titleTexture, new Rectangle(760, 0, 400, 400), Color.White);

        usernameBox.Draw(spriteBatch);
        passwordBox.Draw(spriteBatch);
        submitButton.Draw(spriteBatch);

        if (ErrorString != "")
        {
            Vector2 textPos = Cogs.centreTextPos(font, ErrorString, 960, 1000);
            spriteBatch.DrawString(font, ErrorString, textPos, Color.Red);
        }

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
