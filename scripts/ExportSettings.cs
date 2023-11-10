using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GPTLocker
{
    /// <summary>
    /// Provides functionality to export a chat log to a file in JSON or plain text format.
    /// </summary>
    internal class ExportSettings
    {
        /// <summary>
        /// Serializes the chat log to a JSON string.
        /// </summary>
        /// <returns>A JSON string representation of the chat log.</returns>
        private string ExportChatLogToJson(List<ChatEntry> chatLog)
        {
            return JsonConvert.SerializeObject(chatLog, Formatting.Indented);
        }

        /// <summary>
        /// Converts the chat log to a plain text string.
        /// </summary>
        /// <returns>A plain text representation of the chat log.</returns>
        private string ExportChatLogToPlainText(List<ChatEntry> chatLog)
        {
            var stringBuilder = new StringBuilder();

            foreach (var entry in chatLog)
            {
                stringBuilder.AppendLine(entry.username);
                stringBuilder.AppendLine(entry.message);

                // Adds a line of space between messages
                stringBuilder.AppendLine();
            }

            // Returns the string with the last empty line removed.
            return stringBuilder.ToString().TrimEnd();
        }

        /// <summary>
        /// Opens a Save File Dialog allowing the user to save the chat log to a file in either JSON or plain text format.
        /// </summary>
        public void SaveChatLogToFile(List<ChatEntry> chatLog)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|Text files (*.txt)|*.txt",
                FileName = "ChatLog_" + DateTime.Now.ToString("yy-MM-dd_HH-mm-ss")
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileContent = string.Empty;
                    string extension = Path.GetExtension(saveFileDialog.FileName);

                    // Serialize chat log based on selected file type
                    if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        fileContent = ExportChatLogToJson(chatLog);
                    }
                    else if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        fileContent = ExportChatLogToPlainText(chatLog);
                    }

                    // Write to file
                    File.WriteAllText(saveFileDialog.FileName, fileContent);
                }
                catch (Exception ex)
                {
                    // Handle exceptions such as unauthorized access, path too long, etc.
                    // Depending on the application requirements, show a message to the user or log the exception.
                    throw new InvalidOperationException("Error while saving the file.", ex);
                }
            }
        }
    }
}
