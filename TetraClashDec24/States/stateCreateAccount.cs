using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace TetraClashDec24
{
    // The CreateAccountState class manages the user interface and logic for account creation.
    // It handles user input for username and password, validates credentials,
    // and communicates with the server to create a new account.
    public class CreateAccountState : AppState
    {
        // UI elements: buttons for username and password fields, submission, and navigation to login.
        private Button usernameBox;
        private Button passwordBox;
        private Button submitButton;
        private Button loginButton;
        private Texture2D titleTexture; // Displays the game logo.

        // String to store error messages that are displayed to the user.
        private string ErrorString = "";
        // Default placeholder texts for the username and password input boxes.
        private readonly string UBDefaultString = "Enter Username";
        private readonly string PBDefaultString = "Enter Password";

        // Variables to hold the user's input.
        private string username = "";
        private string password = "";

        // Current and previous states of keyboard and mouse input.
        private KeyboardState keyboard;
        private MouseState mouse;
        private KeyboardState prevKeyboardState;
        private ButtonState prevClickState;

        // Flag to keep track of Caps Lock state for text input.
        bool isCapsLockOn = false;

        // Enum to represent which input field is currently focused.
        private enum InputField { None, Username, Password }
        // Initially, no field is focused.
        private InputField focusedField = InputField.None;

        // Constructor that receives the main application instance and initial mouse click state.
        public CreateAccountState(App app, ButtonState clickState) : base(app)
        {
            prevClickState = clickState;
        }

        // LoadContent initializes all UI components and loads required assets.
        public override void LoadContent()
        {
            // Create the username input box at a specified screen position and size.
            usernameBox = new Button(App, 835, 600, 250, 50, Color.White, UBDefaultString);
            // Create the password input box.
            passwordBox = new Button(App, 835, 675, 250, 50, Color.White, PBDefaultString);
            // Create the submit button for account creation.
            submitButton = new Button(App, 885, 750, 150, 100, Color.White, "Submit!");
            // Create the login button to switch to the login state.
            loginButton = new Button(App, 885, 875, 150, 100, Color.White, "Login");
            // Load the title/logo texture from the content pipeline.
            titleTexture = App.Content.Load<Texture2D>(@"Logo");
        }

        // Update method processes user input and updates the state accordingly.
        public override void Update(GameTime gameTime)
        {
            // Save the previous keyboard state before updating.
            prevKeyboardState = keyboard;
            // Get current mouse and keyboard states.
            mouse = Mouse.GetState();
            keyboard = Keyboard.GetState();

            // Process new mouse clicks (only when state changes from not pressed to pressed)
            if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
            {
                // Create a point for the current mouse position.
                Point mousePosition = new Point(mouse.X, mouse.Y);

                // Check if the submit button was clicked.
                if (submitButton.Box.Contains(mousePosition))
                {
                    submitButton.PlaySound(); // Play click sound.
                    SubmitAccountAsync();     // Begin submission process.
                    focusedField = InputField.None; // Remove focus from text fields.
                }
                // Check if the login button was clicked.
                else if (loginButton.Box.Contains(mousePosition))
                {
                    loginButton.PlaySound();
                    // Transition to the LoginState.
                    App.ChangeState(new LoginState(App, mouse.LeftButton));
                    focusedField = InputField.None;
                }
                // Check if the username box was clicked.
                else if (usernameBox.Box.Contains(mousePosition))
                {
                    focusedField = InputField.Username;
                    usernameBox.PlaySound();
                }
                // Check if the password box was clicked.
                else if (passwordBox.Box.Contains(mousePosition))
                {
                    focusedField = InputField.Password;
                    passwordBox.PlaySound();
                }
                else
                {
                    // Click outside of any interactive field clears focus.
                    focusedField = InputField.None;
                }
            }

            // Process keyboard input and update text for the focused field.
            string input = HandleInput(keyboard, prevKeyboardState, ref isCapsLockOn);

            // Flags to check which input field currently has focus.
            bool isUsernameFocused = focusedField == InputField.Username;
            bool isPasswordFocused = focusedField == InputField.Password;

            // Update the username input box.
            usernameBox.Highlighted = isUsernameFocused;
            if (isUsernameFocused)
            {
                // Update username with current input.
                username = input;
                usernameBox.Text = username;
            }
            else if (string.IsNullOrEmpty(username))
            {
                // Restore default placeholder if no username is entered.
                usernameBox.Text = UBDefaultString;
            }

            // Update the password input box with masked characters.
            passwordBox.Highlighted = isPasswordFocused;
            if (isPasswordFocused)
            {
                // Update password with current input.
                password = input;
                // Display asterisks instead of the actual password.
                passwordBox.Text = new string('*', password.Length);
            }
            else if (string.IsNullOrEmpty(password))
            {
                // Restore default placeholder if no password is entered.
                passwordBox.Text = PBDefaultString;
            }

            // Save current mouse click state for next frame.
            prevClickState = mouse.LeftButton;
        }

        // Draw renders all UI elements to the screen.
        public override void Draw(GameTime gameTime)
        {
            // Create a new SpriteBatch for drawing.
            SpriteBatch spriteBatch = new SpriteBatch(App.GraphicsDevice);
            spriteBatch.Begin();

            // Draw the title text "Create an Account" centered at a specific screen position.
            spriteBatch.DrawString(App.titleFont, "Create an Account",
                Cogs.centreTextPos(App.titleFont, "Create an Account", 960, 475), Color.White);

            // Draw the logo or title image.
            spriteBatch.Draw(titleTexture, new Rectangle(580, 100, 745, 329), Color.White);

            // Draw each of the buttons (username, password, submit, and login).
            usernameBox.Draw(spriteBatch);
            passwordBox.Draw(spriteBatch);
            submitButton.Draw(spriteBatch);
            loginButton.Draw(spriteBatch);

            // If there is an error message, draw it centered in red near the bottom of the screen.
            if (ErrorString != "")
            {
                Vector2 textPos = Cogs.centreTextPos(App.font, ErrorString, 960, 1000);
                spriteBatch.DrawString(App.font, ErrorString, textPos, Color.Red);
            }

            spriteBatch.End();
        }

        // HandleInput processes keyboard events and updates the input text for the currently focused field.
        public string HandleInput(KeyboardState keyboard, KeyboardState prevKeyboardState, ref bool isCapsLockOn)
        {
            // Start with the current text for the focused field.
            string text = "";
            if (focusedField == InputField.Username)
            {
                text = username;
            }
            else if (focusedField == InputField.Password)
            {
                text = password;
            }
            // Get an array of currently pressed keys.
            Keys[] pressedKeys = keyboard.GetPressedKeys();
            // Check if either Shift key is pressed.
            bool isShiftPressed = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            // Process each key pressed.
            foreach (Keys key in pressedKeys)
            {
                // Only process keys that were not pressed in the previous state.
                if (prevKeyboardState.IsKeyUp(key))
                {
                    if (key == Keys.Tab)
                    {
                        // If Tab is pressed, cycle to the next input field.
                        text = CycleInputField();
                    }
                    else if (key == Keys.Back && text.Length > 0)
                    {
                        // If Backspace is pressed, remove the last character.
                        text = text.Remove(text.Length - 1);
                    }
                    else if (key == Keys.Space)
                    {
                        // If Space is pressed, add a space character.
                        text += " ";
                    }
                    else if (key == Keys.CapsLock)
                    {
                        // Toggle the Caps Lock flag.
                        isCapsLockOn = !isCapsLockOn;
                    }
                    else if (key >= Keys.A && key <= Keys.Z)
                    {
                        // Process alphabetical keys.
                        char letter = key.ToString()[0];
                        // Use XOR to determine the correct case based on Shift and Caps Lock.
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
                        // Process numeric keys (0-9).
                        if (isShiftPressed)
                        {
                            // If Shift is pressed, use special characters instead.
                            string shiftedNumbers = ")!@#$%^&*(";
                            text += shiftedNumbers[key - Keys.D0];
                        }
                        else
                        {
                            // Otherwise, append the corresponding number.
                            text += (char)('0' + (key - Keys.D0));
                        }
                    }
                    else if (key == Keys.Enter)
                    {
                        // If Enter is pressed, attempt to submit the account details.
                        SubmitAccountAsync();
                    }
                }
            }
            // Return the updated text for the focused field.
            return text;
        }

        // CycleInputField changes focus to the next input field (Username -> Password -> None, etc.)
        private string CycleInputField()
        {
            // Cycle through the enum values.
            focusedField = (InputField)(((int)focusedField + 1) % Enum.GetValues(typeof(InputField)).Length);
            // Return the text of the newly focused field.
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

        // SubmitAccountAsync handles sending the account creation request to the server.
        private async Task SubmitAccountAsync()
        {
            // Connect to the server on localhost at port 5000.
            App._client = new TcpClient("localhost", 5000);
            App._stream = App._client.GetStream();
            // Validate the entered username and password.
            if (!ValidCredentials())
            {
                ErrorString = "Username or password is invalid";
            }
            else
            {
                // Generate a secure random salt.
                string salt = Security.GenerateSalt();
                await Console.Out.WriteLineAsync($"Salt: {salt}");
                // Generate a hash of the password combined with the salt.
                string hash = Security.GenerateHash(password, salt);
                Console.WriteLine($"Password Hash: {hash}");
                // Construct the message for account creation.
                string message = $"create{username}:{hash}:{salt}";
                // Send the account creation request and wait for the server's response.
                string response = await Client.SendMessageAsync(App._stream, message, true);

                if (response.StartsWith("Success"))
                {
                    // If successful, save the credentials locally and set initial rating.
                    await Cogs.saveCache(username, salt);
                    App.Username = username;
                    App.Rating = 1000; // Default rating for new accounts.
                    // Transition to the main menu state.
                    App.ChangeState(new MainMenuState(App, mouse.LeftButton));
                }
                else if (response == "Player Exists")
                {
                    // Display an error if the username is already in use.
                    ErrorString = "Username already exists, please select a new one or log in.";
                }
                else
                {
                    // Handle any unknown errors.
                    ErrorString = $"Unknown Error: {response}";
                }
            }
        }

        // ValidCredentials checks if the username and password meet the required criteria.
        private bool ValidCredentials()
        {
            // Username must be between 5 and 15 characters.
            if (username.Length < 5 || username.Length > 15)
            {
                return false;
            }
            // Password must have at least one special character, one digit, and be at least 8 characters long.
            string regex = @"^(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?])(?=.*[0-9]).{8,}$";
            if (!Regex.IsMatch(password, regex))
            {
                return false;
            }
            return true;
        }
    }
}
