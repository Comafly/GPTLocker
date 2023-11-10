using System.Windows.Controls;
using Newtonsoft.Json;

namespace GPTLocker
{
    /// <summary>
    /// Used to track chat entries from the user and ChatGPT.
    /// Utilized for exporting the chat log to a TXT or JSON.
    /// </summary>
    public class ChatEntry
    {
        public string username;
        public string message;

        /// <summary>
        /// Holds a reference to the textbox component associated with this chat entry.
        /// </summary>
        [JsonIgnore]
        public TextBox? MessageTextBox { get; private set; }

        public ChatEntry(string username = "", string message = "")
        {
            this.username = username;
            this.message = message;
            MessageTextBox = null;
        }

        /// <summary>
        /// The Textbox that contains the text for this chat entry.
        /// </summary>
        public void SetMessageTextBox(TextBox messageTextBox)
        {
            MessageTextBox = messageTextBox;
        }
    }
}
