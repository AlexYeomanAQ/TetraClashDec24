using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace TetraClashDec24
{
    public class CreateAccountState : AppState
    {

        private Button usernameBox;
        private Button passwordBox;

        private Button submitButton;
        private Button loginButton;
        private Texture2D titleTexture;

        private string ErrorString = "";
        private readonly string UBDefaultString = "Enter Username";
        private readonly string PBDefaultString = "Enter Password";

        private string username = "";
        private string password = "";

        private KeyboardState keyboard;
        private MouseState mouse;

        private KeyboardState prevKeyboardState;
        private ButtonState prevClickState;

        bool isCapsLockOn = false;
        private enum InputField {None, Username, Password}
        private InputField focusedField = InputField.None;

        public CreateAccountState(App app, ButtonState clickState) : base(app)
        {
            prevClickState = clickState;
        }

        public override void LoadContent()
        {
            usernameBox = new Button(App, 835, 600, 250, 50, Color.White, UBDefaultString);

            passwordBox = new Button(App, 835, 675, 250, 50, Color.White, PBDefaultString);

            submitButton = new Button(App, 885, 750, 150, 100, Color.White, "Submit!");

            loginButton = new Button(App, 885, 875, 150, 100, Color.White, "Login");

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
                else if (submitButton.Box.Contains(mousePosition))
                {
                    SubmitAccountAsync();
                }
                else if (loginButton.Box.Contains(mousePosition))
                {
                    App.ChangeState(new LoginState(App, mouse.LeftButton));
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
                usernameBox.Highlighted= true;
            }
            else if (focusedField == InputField.Password)
            {
                password = input;
                passwordBox.Text = new string('*', password.Length);
                passwordBox.Highlighted = true;
            }
            if (focusedField != InputField.Username)
            {
                usernameBox.Highlighted = false;
                if (username == "")
                {
                    usernameBox.Text = UBDefaultString;
                }
            }
            if (focusedField != InputField.Password)
            {
                passwordBox.Highlighted = false;
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

            spriteBatch.DrawString(App.titleFont, "Create an Account", Cogs.centreTextPos(App.titleFont, "Create an Account", 960, 475), Color.White);

            spriteBatch.Draw(titleTexture, new Rectangle(760, 0, 400, 400), Color.White);

            usernameBox.Draw(spriteBatch);
            passwordBox.Draw(spriteBatch);
            submitButton.Draw(spriteBatch);
            loginButton.Draw(spriteBatch);

            if (ErrorString != "")
            {
                Vector2 textPos = Cogs.centreTextPos(App.font, ErrorString, 960, 1000);
                spriteBatch.DrawString(App.font, ErrorString, textPos, Color.Red);
            }

            spriteBatch.End();
        }

        public string HandleInput(KeyboardState keyboard, KeyboardState prevKeyboardState, ref bool isCapsLockOn)
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
                        SubmitAccountAsync();
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
        private async Task SubmitAccountAsync()
        {
            App._client = new TcpClient("localhost", 5000);
            App._stream = App._client.GetStream();
            if (!ValidCredentials())
            {

            }
            string salt = Security.GenerateSalt();
            string hash = Security.GenerateHash(password, salt);
            string message = $"create{username}:{hash}:{salt}";
            string response = await Client.SendMessageAsync(App._stream, message, true);

            if (response.StartsWith("Success"))
            {
                await Cogs.saveCache(username, salt);
                App.Username = username;
                App.Rating = 1000; //Since all new accounts
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

        private bool ValidCredentials()
        {
            if (username.Length < 5 || username.Length > 15)
            {
                return false;
            }
            string regex = @"^(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?])(?=.*[0-9]).{8,}$";
            if (!Regex.IsMatch(password, regex))
            {
                return false;
            }
            return true;
        }
    }
}