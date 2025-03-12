using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Net.Sockets;

namespace TetraClashDec24
{
    // The LoginState class handles the user interface and logic for logging into an existing account.
    // It is part of the state machine of the game and allows users to input credentials, switch to account creation,
    // and perform authentication with the server.
    public class LoginState : AppState
    {
        // UI elements: input boxes for username and password, buttons for submitting login or creating a new account.
        private Button usernameBox;
        private Button passwordBox;
        private Button submitButton;
        private Button createAccountButton;
        private Texture2D titleTexture; // Image used as the logo.

        // Strings for error messages and default placeholder texts.
        private string ErrorString;
        private string UBDefaultString = "Enter Username";
        private string PBDefaultString = "Enter Password";

        // Variables to store user input.
        private string username = "";
        private string password = "";

        // Keyboard and mouse state variables for handling user input.
        private KeyboardState keyboard;
        private MouseState mouse;
        private KeyboardState prevKeyboardState;
        ButtonState prevClickState; // Stores previous mouse button state.

        // Flag for tracking Caps Lock state during text input.
        bool isCapsLockOn = false;
        // Enum to determine which input field is currently focused.
        private enum InputField { None, Username, Password }
        private InputField focusedField = InputField.None;

        // Constructor initializes the LoginState with optional cached username and error message.
        public LoginState(App app, ButtonState clickState, string cache_username = "", string error_string = "") : base(app)
        {
            prevClickState = clickState;
            username = cache_username; // Prepopulate the username if provided.
            ErrorString = error_string;
        }

        // LoadContent sets up the UI elements and loads necessary assets.
        public override void LoadContent()
        {
            // Initialize the username input box with the cached username (if available).
            usernameBox = new Button(App, 835, 600, 250, 50, Color.White, username);

            // Initialize the password input box with a default placeholder.
            passwordBox = new Button(App, 835, 675, 250, 50, Color.White, PBDefaultString);

            // Create the submit button for logging in.
            submitButton = new Button(App, 885, 750, 150, 100, Color.White, "Submit!");

            // Create the button that lets the user switch to the account creation state.
            createAccountButton = new Button(App, 885, 875, 150, 100, Color.White, "Create new account");

            // Load the title/logo texture from the content pipeline.
            titleTexture = App.Content.Load<Texture2D>(@"Logo");
        }

        // Update method processes input and updates the login state every frame.
        public override void Update(GameTime gameTime)
        {
            // Store the previous keyboard state before updating.
            prevKeyboardState = keyboard;
            // Get the current mouse and keyboard states.
            mouse = Mouse.GetState();
            keyboard = Keyboard.GetState();

            // Process mouse clicks when the left button is pressed and its state has changed.
            if (mouse.LeftButton == ButtonState.Pressed && prevClickState != mouse.LeftButton)
            {
                // Create a Point object for the current mouse coordinates.
                Point mousePosition = new Point(mouse.X, mouse.Y);

                // Check if the username input box was clicked.
                if (usernameBox.Box.Contains(mousePosition))
                {
                    usernameBox.PlaySound();
                    focusedField = InputField.Username; // Set focus to the username field.
                }
                // Check if the password input box was clicked.
                else if (passwordBox.Box.Contains(mousePosition))
                {
                    focusedField = InputField.Password; // Set focus to the password field.
                    passwordBox.PlaySound();
                }
                // If the user clicks on the "Create new account" button, switch to the account creation state.
                else if (createAccountButton.Box.Contains(mousePosition))
                {
                    createAccountButton.PlaySound();
                    App.ChangeState(new CreateAccountState(App, mouse.LeftButton));
                    focusedField = InputField.None; // Clear input focus.
                }
                // If the user clicks the submit button, attempt to log in.
                else if (submitButton.Box.Contains(mousePosition))
                {
                    submitButton.PlaySound();
                    LoginAsync(); // Perform the login asynchronously.
                    focusedField = InputField.None; // Clear input focus.
                }
                else
                {
                    // Click outside of interactive elements clears the focus.
                    focusedField = InputField.None;
                }
            }

            // Process keyboard input for the focused field.
            string input = HandleInput(keyboard, prevKeyboardState, ref isCapsLockOn);

            // Determine which field is currently focused.
            bool isUsernameFocused = (focusedField == InputField.Username);
            bool isPasswordFocused = (focusedField == InputField.Password);

            // Update the username field if it is focused.
            if (isUsernameFocused)
            {
                username = input;
                usernameBox.Text = username;
            }
            usernameBox.Highlighted = isUsernameFocused;
            if (!isUsernameFocused && string.IsNullOrEmpty(username))
            {
                // Display the default placeholder if no username is entered.
                usernameBox.Text = UBDefaultString;
            }

            // Update the password field; mask the input with asterisks for security.
            if (isPasswordFocused)
            {
                password = input;
                passwordBox.Text = new string('*', password.Length);
            }
            passwordBox.Highlighted = isPasswordFocused;
            if (!isPasswordFocused && string.IsNullOrEmpty(password))
            {
                // Display the default placeholder if no password is entered.
                passwordBox.Text = PBDefaultString;
            }

            // Update the previous click state for next frame comparison.
            prevClickState = mouse.LeftButton;
        }

        // Draw method renders the login UI elements on the screen.
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = new SpriteBatch(App.GraphicsDevice);
            spriteBatch.Begin();

            // Draw the input boxes and buttons.
            usernameBox.Draw(spriteBatch);
            passwordBox.Draw(spriteBatch);
            submitButton.Draw(spriteBatch);
            createAccountButton.Draw(spriteBatch);

            // Draw the "Login" title text, centered using a helper function.
            spriteBatch.DrawString(App.titleFont, "Login", Cogs.centreTextPos(App.titleFont, "Login", 960, 475), Color.White);
            // Draw the title/logo image.
            spriteBatch.Draw(titleTexture, new Rectangle(580, 100, 745, 329), Color.White);

            // If there's an error message, draw it in red.
            if (ErrorString != "")
            {
                Vector2 textSize = App.font.MeasureString(ErrorString);
                float textX = 700 - textSize.X / 2;
                float textY = 700 - textSize.Y / 2;
                spriteBatch.DrawString(App.font, ErrorString, new Vector2(textX, textY), Color.Red);
            }
            spriteBatch.End();
        }

        // HandleInput processes the keyboard input for the currently focused field.
        // It returns the updated text string based on key presses.
        private string HandleInput(KeyboardState keyboard, KeyboardState prevKeyboardState, ref bool isCapsLockOn)
        {
            string text = "";
            // Start with the text already present in the focused field.
            if (focusedField == InputField.Username)
            {
                text = username;
            }
            else if (focusedField == InputField.Password)
            {
                text = password;
            }

            // Retrieve all keys currently pressed.
            Keys[] pressedKeys = keyboard.GetPressedKeys();
            // Determine if either Shift key is pressed for case sensitivity.
            bool isShiftPressed = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

            // Process each key that is pressed and was not pressed in the previous state.
            foreach (Keys key in pressedKeys)
            {
                if (prevKeyboardState.IsKeyUp(key))
                {
                    if (key == Keys.Tab)
                    {
                        // Cycle focus to the next input field when Tab is pressed.
                        text = CycleInputField();
                    }
                    else if (key == Keys.Back && text.Length > 0)
                    {
                        // Remove the last character if Backspace is pressed.
                        text = text.Remove(text.Length - 1);
                    }
                    else if (key == Keys.Space)
                    {
                        // Append a space character.
                        text += " ";
                    }
                    else if (key == Keys.CapsLock)
                    {
                        // Toggle the Caps Lock state.
                        isCapsLockOn = !isCapsLockOn;
                    }
                    else if (key >= Keys.A && key <= Keys.Z)
                    {
                        // Process alphabetical keys.
                        char letter = key.ToString()[0];
                        // Use XOR to determine if the letter should be uppercase.
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
                        // Process numeric keys.
                        if (isShiftPressed)
                        {
                            // If Shift is pressed, use corresponding special characters.
                            string shiftedNumbers = ")!@#$%^&*(";
                            text += shiftedNumbers[key - Keys.D0];
                        }
                        else
                        {
                            // Otherwise, add the digit.
                            text += (char)('0' + (key - Keys.D0));
                        }
                    }
                    else if (key == Keys.Enter)
                    {
                        // If Enter is pressed, attempt to log in.
                        LoginAsync();
                    }
                }
            }
            return text;
        }

        // CycleInputField changes focus between the username and password fields.
        // It returns the text of the newly focused field.
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

        // LoginAsync handles the login process by connecting to the server and sending credentials.
        private async void LoginAsync()
        {
            try
            {
                // Attempt to connect to the server on localhost at port 5000.
                App._client = new TcpClient("localhost", 5000);
                App._stream = App._client.GetStream();
            }
            catch (Exception ex)
            {
                // If connection fails, set an error message.
                ErrorString = $"Error Connecting to server: {ex}";
            }
            // Fetch the salt for the username from either cache or the server.
            string salt = await FetchSalt();
            if (salt == "Username")
            {
                // Indicates that the username could not be found.
                ErrorString = "Error: Username could not be found.";
                username = "";
                password = "";
            }
            else if (salt.Contains(' '))
            {
                // Handle other errors related to the salt response.
                ErrorString = $"Unknown error: {salt}";
            }
            else
            {
                // Generate a secure hash of the entered password with the retrieved salt.
                string hash = Security.GenerateHash(password, salt);
                // Create the login message in the expected format.
                string message = $"login{username}:{hash}";
                // Send the login message and wait for the server's response.
                string response = await Client.SendMessageAsync(App._stream, message, true);
                if (response.StartsWith("Success"))
                {
                    // On successful login, set the global username and save credentials to cache.
                    App.Username = username;
                    await Cogs.saveCache(username, salt);
                    // Parse the player's rating from the server response (after "Success").
                    App.Rating = int.Parse(response.Substring(7));
                    // Transition to the main menu state.
                    App.ChangeState(new MainMenuState(App, mouse.LeftButton));
                }
                else if (response == "Password")
                {
                    // If the server indicates an incorrect password.
                    ErrorString = "Error: Password is incorrect, please retry.";
                    password = "";
                }
                else if (response == "Logged In")
                {
                    // If the user is already logged in on another device.
                    ErrorString = "Error: User is already logged in on another device.";
                }
                else
                {
                    // Handle any other unknown error responses.
                    ErrorString = $"Unknown error: {response}";
                }
            }
        }

        // FetchSalt retrieves the salt value for the given username.
        // If the username matches the cached username, it reads from the local cache file.
        // Otherwise, it sends a request to the server to get the salt.
        private async Task<string> FetchSalt()
        {
            if (username == App.Username)
            {
                // Read the salt from the cache file; assume it's on the second line.
                return File.ReadAllLines(App.CachePath)[1];
            }
            else
            {
                // Request the salt from the server using the "salt" command.
                return await Task.Run(() => Client.SendMessageAsync(App._stream, $"salt{username}", true));
            }
        }
    }
}
