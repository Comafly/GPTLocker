using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GPTLocker
{
    /// <summary>
    /// Manages the queries and interactions with the ChatGPT API.
    /// </summary>
    class ChatQuery
    {
        /// <summary> The Application Settings. </summary>
        private AppSettings appSettings;

        /// <summary> A HttpClient instance for making API requests. </summary>
        private HttpClient httpClient;

        /// <summary> Messages stored in context for ChatGPT.  </summary>
        private List<Dictionary<string, string>> messages;

        /// <summary> Holds the up-to-date text of the current query stream. </summary>
        private StringBuilder CurrentQueryStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatQuery"/> class.
        /// </summary>
        public ChatQuery(AppSettings newSettings)
        {
            appSettings = newSettings;
            messages = new List<Dictionary<string, string>>();
            CurrentQueryStream = new StringBuilder();

            // Initialize the HttpClient and set the Authorization header.
            httpClient = new HttpClient();
            UpdateAuthorizationHeader(appSettings.APIKey);
        }

        /// <summary>
        /// Updates the Authorization header in the HTTP client with a new API key.
        /// </summary>
        /// <param name="newApiKey"> The new API key to set in the header. </param>
        public void UpdateAuthorizationHeader(string newApiKey)
        {
            // Remove the existing Authorization header if it exists.
            if (httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                httpClient.DefaultRequestHeaders.Remove("Authorization");
            }

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {newApiKey}");
        }

        /// <summary>
        /// Sends a POST request to ChatGPT, and live-updates the chat area as ChatGPT streams its response back.
        /// </summary>
        /// <param name="userInput"> The user's query string. </param>
        /// <param name="messageTextBox"> TextBox for the chat response. </param>
        /// <returns> A Task representing the async operation, with the ChatGPT's response. </returns>
        public async Task<string> GetStreamingChatGPTResponseAsync(string userInput, TextBox messageTextBox)
        {
            // Initialize or reset the CurrentQueryStream.
            CurrentQueryStream = new StringBuilder();

            KeepMessageCountWithinContextLimit();

            // Add role and user input to message payload.
            AddMessageToPayload(Role.System, appSettings.CustomRole);
            AddMessageToPayload(Role.User, userInput);

            // Serialize the payload.
            object contextPayload = new { model = appSettings.ChatModel, messages };
            StringContent contextSerialized = SerializeResponseToJson(contextPayload);

            Debug.WriteLine("Payload:");
            Debug.WriteLine(contextPayload);

            try
            {
                var response = await httpClient.PostAsync(appSettings.APIEndpoint, contextSerialized);
                //Debug.WriteLine($"Response: \n {response}");

                //var errorContent = await response.Content.ReadAsStringAsync();
                //Debug.WriteLine($"Error content: {errorContent}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"HTTP Error: {response.StatusCode}");


                    MessageBox.Show(
                    "Unauthorized: The saved API Key is not valid. Please check the settings.",
                    "Invalid API Key", MessageBoxButton.OK, MessageBoxImage.Warning
                );
                    return $"HTTP Error: {response.StatusCode}";
                }

                return await ProcessStreamingResponseAsync(response, messageTextBox);
            }
            catch (Exception ex)
            {
                return $"An exception occurred: {ex.Message}";
            }
        }

        /// <summary>
        /// Processes the streaming API response and updates the TextBox in real-time.
        /// </summary>
        /// <param name="response"> The HTTP response message. </param>
        /// <param name="messageTextBox"> TextBox for the chat response. </param>
        /// <returns> A Task representing the async operation, with the ChatGPT's response. </returns>
        private async Task<string> ProcessStreamingResponseAsync(HttpResponseMessage response, TextBox messageTextBox)
        {
            // Convert the response content to a stream.
            var responseStream = await response.Content.ReadAsStreamAsync();

            // The pattern to look for in the response content.
            string pattern = "\"content\": \"";
            bool capturing = false;
            bool isEscaping = false;
            char[] buffer = new char[1];

            using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
            {
                StringBuilder tempPattern = new StringBuilder();

                // Read the stream character by character.
                while (await reader.ReadAsync(buffer, 0, 1) > 0)
                {
                    char currentChar = buffer[0];

                    // Check for JSON escape character.
                    if (currentChar == '\\' && !isEscaping)
                    {
                        isEscaping = true;

                        // Add the escape character if we are already capturing.
                        if (capturing) 
                        { 
                            CurrentQueryStream.Append(currentChar); 
                        }
                        continue;
                    }

                    if (capturing)
                    {
                        if (currentChar == '\"' && !isEscaping)
                        {
                            // End of content value.
                            string contentValue = CurrentQueryStream.ToString();

                            // Unescape JSON specific characters.
                            string unescapedContentValue = UnescapeJsonString(contentValue);

                            // Update the TextBox on the UI thread.
                            await messageTextBox.Dispatcher.InvokeAsync(() => {
                                messageTextBox.Text = unescapedContentValue;
                            }, System.Windows.Threading.DispatcherPriority.Background);
                            
                            return unescapedContentValue;
                        }
                        else
                        {
                            // Append the character to the query stream.
                            CurrentQueryStream.Append(currentChar);
                        }
                    }
                    else
                    {
                        // Capture the character for pattern matching.
                        tempPattern.Append(currentChar);

                        // Start capturing once the pattern is found.
                        if (tempPattern.ToString().EndsWith(pattern))
                        {
                            capturing = true;
                            // Clear the temporary pattern StringBuilder for future use if needed.
                            tempPattern.Clear();
                        }
                    }

                    // Reset the escaping state if it was set.
                    if (isEscaping) isEscaping = false;
                }
            }

            return "Pattern not found.";
        }

        /// <summary>
        /// Utility method to unescape JSON specific characters.
        /// </summary>
        private string UnescapeJsonString(string jsonString)
        {
            return jsonString.Replace("\\\"", "\"")
                             .Replace("\\\\", "\\")
                             .Replace("\\/", "/")
                             .Replace("\\b", "\b")
                             .Replace("\\f", "\f")
                             .Replace("\\n", "\n")
                             .Replace("\\r", "\r")
                             .Replace("\\t", "\t");
        }

        /// <summary>
        /// Adds a message to the query payload. Each message is used by ChatGPT for context about the conversation.
        /// </summary>
        /// <param name="role"> The role of the entity sending the message, either System or User. </param>
        /// <param name="newMessage"> The content of the message being added. </param>
        public void AddMessageToPayload(Role role, string newMessage)
        {
            Dictionary<string, string> message = new Dictionary<string, string>();
            message["role"] = role.ToString().ToLower();
            message["content"] = newMessage;
            messages.Add(message);
        }

        /// <summary>
        /// Serializes an object to JSON and returns it.
        /// </summary>
        /// <param name="payload"> The object to serialize. </param>
        /// <returns> JSON String - The object serialized as a JSON string </returns>
        private StringContent SerializeResponseToJson(object payload)
        {
            string payloadString = JsonConvert.SerializeObject(payload);
            return new StringContent(payloadString, Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// Removes the oldest message if the context limit has been reached.
        /// </summary>
        private void KeepMessageCountWithinContextLimit()
        {
            if (messages.Count + 1 > appSettings.ContextLimit)
            {
                messages.RemoveAt(0);
            }
        }
    }
}
