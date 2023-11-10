using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;

namespace GPTLocker
{
    /// <summary>
    /// Represents the application settings for the application.
    /// </summary>
    internal class AppSettings
    {
        /// <summary> The path to the settings.json file.  </summary>
        private string settingsPath;

        /// <summary> The Secret Key to use when connecting to the API.  </summary>
        public string APIKey;

        /// <summary> The amount of messages to store in context for ChatGPT. </summary>
        public int ContextLimit;

        /// <summary> Custom instructions appended to all prompts given to GPT. </summary>
        public string CustomRole;

        /// <summary> The endpoint for ChatGPT completions. </summary>
        public string APIEndpoint;

        /// <summary> The model of ChatGPT to use. </summary>
        public string ChatModel;

        /// <summary> Whether Ctrl+Enter should be used to submit a message. </summary>
        public bool SubmitShortcutState;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettings"/> class with specified or default values.
        /// </summary>
        public AppSettings(
            string newAPIkey = "No API key.",
            int contextLimit = 3,
            string customRole = "You are a helpful assistant.",
            string apiEndpoint = "https://api.openai.com/v1/chat/completions",
            string chatModel = "gpt-3.5-turbo",
            bool submitShortcutState = true)
        {
            APIKey = newAPIkey;
            ContextLimit = contextLimit;
            CustomRole = customRole;
            APIEndpoint = apiEndpoint;
            ChatModel = chatModel;
            SubmitShortcutState = submitShortcutState;

            // Load the settings from the settings file, and pass them to ChatQuery.
            // Use AppDomain.CurrentDomain.BaseDirectory to get the directory of the executable.
            settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        }

        /// <summary>
        /// Updates the API Key.
        /// </summary>
        /// <param name="newAPIKey"> The new API key. </param>
        public void SetAPIKey(string newAPIKey)
        {
            APIKey = newAPIKey;
        }

        /// <summary>
        /// Updates the custom role for GPT.
        /// </summary>
        /// <param name="newCustomRole"> The new custom role. </param>
        public void SetCustomRole(string newCustomRole)
        {
            CustomRole = newCustomRole;
        }

        /// <summary>
        /// Updates the context limit for ChatGPT.
        /// </summary>
        /// <param name="newContextLimitString"> The new context limit as a string. </param>

        public void SetContextLimit(string newContextLimitString = "3")
        {
            if (int.TryParse(newContextLimitString, out int newContextLimitInt))
            {
                // Clamp the value between 1 and 25.
                ContextLimit = Math.Clamp(newContextLimitInt, 1, 25);
            }
            else
            {
                MessageBox.Show("Invalid Context Amount.");
            }
        }

        /// <summary>
        /// Sets the API endpoint URL.
        /// </summary>
        /// <param name="newAPIEndpoint"> The new endpoint URL for the API. The default is ChatGPT 3.5 Turbo. </param>
        public void SetEndpoint(string newAPIEndpoint = "https://api.openai.com/v1/chat/completions")
        {
            APIEndpoint = newAPIEndpoint;
        }

        /// <summary>
        /// Sets the Submit keyboard shortcut state.
        /// </summary>
        /// <param name="newState"> The new state of the keyboard shortcut. </param>

        public void SetSubmitShortcutState(bool newState)
        {
            SubmitShortcutState = newState;
        }

        /// <summary>
        /// Saves the given AppSettings object to a JSON file.
        /// </summary>
        /// <param name="settings"> The AppSettings object to save. </param>
        public void SaveSettingsToJSON()
        {
            string jsonString = JsonConvert.SerializeObject(this);
            File.WriteAllText(settingsPath, jsonString);
        }

        /// <summary>
        /// Loads AppSettings from a JSON file. If the file doesn't exist, creates a new AppSettings object with a default API key.
        /// </summary>
        /// <returns> The loaded AppSettings object. </returns>
        public void LoadSettingsFromJSON()
        {
            AppSettings loadedSettings;

            if (File.Exists(settingsPath))
            {
                string jsonString = File.ReadAllText(settingsPath);
                loadedSettings = JsonConvert.DeserializeObject<AppSettings>(jsonString)!;
            }
            else
            {
                loadedSettings = new AppSettings();
                SaveSettingsToJSON();
            }

            SetAPIKey(loadedSettings.APIKey);
            SetCustomRole(loadedSettings.CustomRole);
            SetContextLimit(loadedSettings.ContextLimit.ToString());
            SetSubmitShortcutState(loadedSettings.SubmitShortcutState);
        }
    }
}
