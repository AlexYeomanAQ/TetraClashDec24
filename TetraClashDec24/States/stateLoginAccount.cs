using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using System.IO;
using System;
using SharpDX.Win32;
using System.Net.Sockets;

namespace TetraClashDec24
{
    public class LoginState : AppState
    {
        App App;

        private InputButton usernameBox;
        private InputButton passwordBox;

        private Button submitButton;
        private Button createAccountButton;

        private Texture2D titleTexture;

        private string ErrorString;
        private string UBDefaultString = "Enter Username";
        private string PBDefaultString = "Enter Password";

        private string username = "";
        private string password = "";

        private KeyboardState keyboard;
        private MouseState mouse;

        private KeyboardState prevKeyboardState;
        ButtonState prevClickState;

        bool isCapsLockOn = false;
        private enum InputField { None, Username, Password }
        private InputField focusedField = InputField.None;

        public LoginState(App app, ButtonState clickState, string cache_username = "", string error_string = "") : base(app)
        {
            App = app;
            prevClickState = clickState;
            username = cache_username;
            ErrorString = error_string;
        }

        public override void LoadContent()
        {
            usernameBox = new InputButton(App, 835, 600, 250, 50, Color.White, username);

            passwordBox = new InputButton(App, 835, 675, 250, 50, Color.White, PBDefaultString);

            submitButton = new Button(App, 885, 750, 150, 100, Color.White, "Submit!");

            createAccountButton = new Button(App, 885, 875, 150, 100, Color.White, "Create new account");

            titleTexture = App.Content.Load<Texture2D>(@"tempLogo");
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
                else if (createAccountButton.Box.Contains(mousePosition))
                {
                    App.ChangeState(new CreateAccountState(App, mouse.LeftButton));
                }
                else if (submitButton.Box.Contains(mousePosition))
                {
                    LoginAsync();
                }
                else
                {
                    focusedField = InputField.None;
                }
            }

            string input = HandleInput(keyboard, prevKeyboardState, ref isCapsLockOn);

            if (focusedField == InputField.Username)
            {
                username = input;
                usernameBox.Text = username;
                usernameBox.highlighted = true;
            }
            else if (focusedField == InputField.Password)
            {
                password = input;
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
            SpriteBatch spriteBatch = new SpriteBatch(App.GraphicsDevice);
            spriteBatch.Begin();
            usernameBox.Draw(spriteBatch);
            passwordBox.Draw(spriteBatch);
            submitButton.Draw(spriteBatch);
            createAccountButton.Draw(spriteBatch);

            spriteBatch.DrawString(App.titleFont, "Login", Cogs.centreTextPos(App.titleFont, "Login", 960, 475), Color.White);
            spriteBatch.Draw(titleTexture, new Rectangle(760, 0, 400, 400), Color.White);

            if (ErrorString != "")
            {
                Vector2 textSize = App.font.MeasureString(ErrorString);
                float textX = 700 - textSize.X / 2;
                float textY = 700 - textSize.Y / 2;
                spriteBatch.DrawString(App.font, ErrorString, new Vector2(textX, textY), Color.Red);
            }
            spriteBatch.End();
        }

        private string HandleInput(KeyboardState keyboard, KeyboardState prevKeyboardState, ref bool isCapsLockOn)
        {
            string text = "";

            if (focusedField == InputField.Username)
            {
                text = username;
            }
            else if (focusedField == InputField.Password)
            {
                text = password;
            }

            Keys[] pressedKeys = keyboard.GetPressedKeys();
            bool isShiftPressed = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

            foreach (Keys key in pressedKeys)
            {
                // Process only if the key was not pressed in the previous state
                if (prevKeyboardState.IsKeyUp(key))
                {
                    if (key == Keys.Tab)
                    {
                        // Handle Tab
                        text = CycleInputField();
                    }
                    else if (key == Keys.Back && text.Length > 0)
                    {
                        // Handle backspace
                        text = text.Remove(text.Length - 1);
                    }
                    else if (key == Keys.Space)
                    {
                        // Handle space
                        text += " ";
                    }
                    else if (key == Keys.CapsLock)
                    {
                        // Toggle Caps Lock state
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
                        text += letter;
                    }
                    else if (key >= Keys.D0 && key <= Keys.D9)
                    {
                        // Handle numbers and special characters
                        if (isShiftPressed)
                        {
                            string shiftedNumbers = ")!@#$%^&*(";
                            text += shiftedNumbers[key - Keys.D0];
                        }
                        else
                        {
                            text += (char)('0' + (key - Keys.D0));
                        }
                    }
                    else if (key == Keys.Enter)
                    {
                        LoginAsync();
                    }
                }
            }
            return text;
        }

        private string CycleInputField()
        {
            focusedField = (InputField)(((int)focusedField + 1) % Enum.GetValues(typeof(InputField)).Length);
            if (focusedField == InputField.Username)
            {
                return username;
            }
            else if (focusedField == InputField.Password)
            {
                return password;
            }
            return "";
        }

        private async void LoginAsync()
        {
            App._client = new TcpClient("localhost", 5000);
            App._stream = App._client.GetStream();
            string salt = await FetchSalt();
            if (salt == "Username")
            {
                ErrorString = "Error: Username could not be found.";
                username = "";
                password = "";
            }
            else if (salt.Contains(' '))
            {
                ErrorString = $"Unknown error: {salt}";
            }
            else
            {
                string hash = Security.GenerateHash(password, salt);
                string message = $"login{username}:{hash}";
                string response = await Client.SendMessageAsync(App._stream, message);
                if (response == "Success")
                {
                    App.Username = username;
                    App.ChangeState(new MainMenuState(App, mouse.LeftButton));
                }
                else if (response == "Password")
                {
                    ErrorString = "Error: Password is incorrect, please retry.";
                    password = "";
                }
                else
                {
                    ErrorString = $"Unknown error: {response}";
                }
            }
        }

        private async Task<string> FetchSalt()
        {
            if (username == App.Username)
            {
                return File.ReadAllLines(App.CachePath)[1];
            }
            else
            {
                return await Task.Run(() => Client.SendMessageAsync(App._stream, $"salt{username}"));
            }
        }
    }
}