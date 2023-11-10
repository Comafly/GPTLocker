using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media;

namespace GPTLocker
{
    /// <summary>
    /// Represents the main window of the ChatLocker application.
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppSettings appSettings;
        private ExportSettings exportSettings;

        private bool submitShortcutState = true;

        private CustomTitleBar customTitleBar;
        private ChatQuery chatQuery;
        private ChatComponents chatComponents; 
        private DateTime lastRequestTime = DateTime.MinValue;
        public List<ChatEntry> ChatLog { get; private set; }

        /// <summary>
        /// Initializes the main window components.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Setup the custom titlebar.
            customTitleBar = new CustomTitleBar(this);
            SetupTitleBar();

            appSettings = new AppSettings();
            appSettings.LoadSettingsFromJSON();

            exportSettings = new ExportSettings();

            ChatLog = new List<ChatEntry>();
            chatQuery = new ChatQuery(appSettings);
            chatComponents = new ChatComponents(ChatArea, ChatAreaScrollViewer, ChatLog);

            UpdateSubmitShortcutToggle(appSettings.SubmitShortcutState);
            UpdateSubmitShortcutSubtext();
        }

        /// <summary>
        /// Sets up the custom title bar by initializing the CustomTitleBar object and attaching event handlers.
        /// </summary>
        private void SetupTitleBar()
        {
            MinimizeButton.Click += customTitleBar!.MinimizeButton_Click;
            MaximizeButton.Click += customTitleBar!.MaximizeButton_Click;
            CloseButton.Click += customTitleBar!.CloseButton_Click;

            TitleBarBorder.MouseLeftButtonDown += customTitleBar!.TitleBar_MouseLeftButtonDown;
        }

        /// <summary>
        /// Checks if the API is valid.
        /// </summary>
        /// <returns></returns>
        public bool APIKeyIsValid()
        {
            if (appSettings.APIKey == "No API key.") { return false; }
            if (appSettings.APIKey == "") { return false; }
            return true;
        }

        /// <summary>
        /// Handles the submission of the user input to the chat service and displays the response.
        /// Includes request throttling based on time between requests.
        /// </summary>
        private async void Submit(object sender, RoutedEventArgs e)
        {
            // Check to see if there is an API key.
            if (!APIKeyIsValid()) {
                MessageBox.Show(
                    "The saved API Key is not valid. Please check the settings.",
                    "Invalid API Key", MessageBoxButton.OK, MessageBoxImage.Warning
                );
                return;
            }

            // Get the current time to evaluate request frequency.
            DateTime currentTime = DateTime.Now;

            // Check if the user is sending messages too quickly (less than 3 seconds since the last request).
            if ((currentTime - lastRequestTime).TotalSeconds < 3)
            {
                MessageBox.Show(
                    "You are making requests too quickly. Please wait a moment.",
                    "Request Throttled", MessageBoxButton.OK, MessageBoxImage.Warning
                );
                return;
            }

            // Disable the Submit button to prevent multi-clicks during processing.
            SubmitButton.IsEnabled = false;

            // Retrieve the user input and add it to the chat components.
            string userInput = InputArea.Text; 
            InputArea.Text = "";
            chatComponents!.AddUserRequest(userInput);

            try
            {
                // Add a text box for the chat response and await the GPT API call.
                TextBox messageTextBox = chatComponents.AddChatResponse().MessageTextBox!;
                await chatQuery!.GetStreamingChatGPTResponseAsync(userInput, messageTextBox);
            }
            catch (Exception ex)
            {
                // Show an error message if something goes wrong during API call.
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            finally
            {
                // Update the last request time, regardless of whether the API call was successful.
                lastRequestTime = currentTime;
            }

            // Re-enable the Submit button.
            SubmitButton.IsEnabled = true;
        }

        /// <summary>
        /// Wrapper for the Submit method, used as the Click event handler for the SubmitButton.
        /// </summary>
        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (appSettings.APIKey == "No API key." || appSettings.APIKey == "") { return; }

            Submit(sender, e);
        }

        /// <summary>
        /// Handles the keyboard shortcut for submitting the user input.
        /// Triggered when the user presses the Control + Enter keys.
        /// </summary>
        private void Submit_KeyboardShortcut(object sender, KeyEventArgs e)
        {
            if (appSettings.SubmitShortcutState && e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (appSettings.APIKey == "No API key." || appSettings.APIKey == "") { return; }
                Submit(sender, e);
            }
            else if (!appSettings.SubmitShortcutState && e.Key == Key.Enter)
            {
                if (appSettings.APIKey == "No API key." || appSettings.APIKey == "") { return; }
                Submit(sender, e);
            }
        }
        /// <summary>
        /// Handles the Closed event of the Settings window.
        /// </summary>
        public void SettingsClosed(object sender, EventArgs e)
        {
            UpdateSubmitShortcutToggle(appSettings.SubmitShortcutState);
        }

        /// <summary>
        /// Toggles the submit shortcut state when the associated button is clicked.
        /// </summary>
        public void ToggleSubmitShortcut_Click(object sender, RoutedEventArgs e)
        {
            if (SubmitShortcutToggle != null)
            {
                UpdateSubmitShortcutToggle(SubmitShortcutToggle.IsChecked.GetValueOrDefault());
                submitShortcutState = SubmitShortcutToggle.IsChecked ?? false;
            }
        }

        /// <summary>
        /// Updates the visual state of the SubmitShortcutToggle button.
        /// </summary>
        /// <param name="isEnabled"> Whether the shortcut toggle is enabled. </param>
        public void UpdateSubmitShortcutToggle(bool isEnabled)
        {
            SubmitShortcutToggle.Content = isEnabled ? "Enabled" : "Disabled";
            SubmitShortcutToggle.Foreground = isEnabled ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
        }

        /// <summary>
        /// Assigns the appropriate shortcut text underneath the Submit button.
        /// </summary>
        public void UpdateSubmitShortcutSubtext()
        {
            SubmitSubtext.Text = appSettings.SubmitShortcutState ? "(Ctrl + Enter)" : "(Enter)";
        }

        /// <summary>
        /// Handles the Click event for the SettingsButton. Toggles the visibility of the SettingsPopup.
        /// and populates it with the current settings.
        /// </summary>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPopup.IsOpen = !SettingsPopup.IsOpen;

            ApiKeyTextBox.Text = appSettings.APIKey;
            RoleTextBox.Text = appSettings.CustomRole;
            ContextAmountTextBox.Text = appSettings.ContextLimit.ToString();

            submitShortcutState = appSettings.SubmitShortcutState;
            SubmitShortcutToggle.IsChecked = appSettings.SubmitShortcutState;
            UpdateSubmitShortcutToggle(appSettings.SubmitShortcutState);
        }

        /// <summary>
        /// Handles the Click event for the SaveSettings button in the SettingsPopup.
        /// Saves the new settings and closes the popup.
        /// </summary>
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            appSettings.SetAPIKey(ApiKeyTextBox.Text);
            appSettings.SetCustomRole(RoleTextBox.Text);
            appSettings.SetContextLimit(ContextAmountTextBox.Text);
            appSettings.SetSubmitShortcutState(submitShortcutState);
            UpdateSubmitShortcutSubtext();

            appSettings.SaveSettingsToJSON();
            chatQuery.UpdateAuthorizationHeader(appSettings.APIKey);

            SettingsPopup.IsOpen = false;
        }

        /// <summary>
        /// Handles the Click event for the ExportChatLog button in the SettingsPopup.
        /// Opens a dialog box to save the current chat log as either a JSON or TXT file.
        /// </summary>
        private void ExportChatLog_Click(object sender, RoutedEventArgs e)
        {
            exportSettings.SaveChatLogToFile(ChatLog);
        }
    }
}