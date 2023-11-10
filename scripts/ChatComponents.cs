using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Collections.Generic;

namespace GPTLocker
{
    /// <summary>
    /// Manages and constructs chat components such as user requests and responses.
    /// </summary>
    class ChatComponents
    {
        /// <summary> The area of the window that displays the chat between the user and ChatGPT.  </summary>
        private readonly StackPanel chatArea;

        /// <summary> The scrollview area that contains the chat instance.  </summary>
        private readonly ScrollViewer chatAreaScrollViewer;

        /// <summary> A log of all current chat entries.  </summary>
        private readonly List<ChatEntry> chatLog;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatComponents"/> class.
        /// </summary>
        /// <param name="chatArea"> The main chat area. </param>
        /// <param name="chatAreaScrollViewer"> The ScrollViewer for the chat area. </param>
        public ChatComponents(StackPanel chatArea, ScrollViewer chatAreaScrollViewer, List<ChatEntry> chatLog) 
        {
            this.chatArea = chatArea;
            this.chatAreaScrollViewer = chatAreaScrollViewer;
            this.chatLog = chatLog;
        }

        /// <summary>
        /// Creates a Grid to hold chat content.
        /// </summary>
        /// <param name="backgroundColor"> The background color for the grid. </param>
        /// <returns> The constructed Grid. </returns>
        public Grid CreateHolderGrid(SolidColorBrush backgroundColor)
        {
            Grid holderGrid = new Grid
            {
                Background = backgroundColor
            };

            // Define column layout.
            holderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            holderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            holderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            return holderGrid;
        }

        /// <summary>
        /// Adds a user request message to the chat area.
        /// </summary>
        /// <param name="userInput"> The user's input message. </param>
        public void AddUserRequest(string userInput)
        {
            Color userColor = Color.FromArgb(255, 52, 53, 65);

            // Add the chat entry to the log.
            ChatEntry newChatEntry = new ChatEntry { username = "User", message = userInput };

            AddChatEntry(newChatEntry, new SolidColorBrush(userColor));
            chatLog.Add(newChatEntry);
        }

        /// <summary>
        /// Adds a ChatGPT response message to the chat area.
        /// </summary>
        /// <param name="chatGPTResponse"> ChatGPT's response message. </param>
        /// <returns> The TextBox containing the chat response. </returns>
        public ChatEntry AddChatResponse(string chatGPTResponse = "")
        {
            Color chatGPTColor = Color.FromArgb(255, 68, 70, 84);

            // Add the chat entry to the log.
            ChatEntry newChatEntry = new ChatEntry { username = "ChatGPT", message = chatGPTResponse };

            return AddChatEntry(newChatEntry, new SolidColorBrush(chatGPTColor));
        }

        /// <summary>
        /// Adds a chat entry to the chat area, from either the User or ChatGPT.
        /// </summary>
        /// <param name="username"> The username. </param>
        /// <param name="message"> The chat message. </param>
        /// <param name="backgroundColor"> The background color for the chat entry. </param>
        /// <returns> The TextBox containing the chat entry. </returns>
        public ChatEntry AddChatEntry(ChatEntry chatEntry, SolidColorBrush backgroundColor)
        {
            // Create TextBoxes for username and message.
            TextBox userTypeTextBox = CreateUsernameDisplayTextBox(chatEntry.username);
            TextBox messageTextBox = CreateMessageTextBox(chatEntry.message);
            Button copyButton = CreateCopyButton(messageTextBox);

            // Create content StackPanel to hold the TextBoxes.
            StackPanel contentStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(15, 20, 15, 20)
            };
            contentStack.Children.Add(userTypeTextBox);
            contentStack.Children.Add(messageTextBox);

            // Create container for contentStack and copyButton.
            StackPanel contentAndButtonStack = new StackPanel { Orientation = Orientation.Horizontal };
            contentAndButtonStack.Children.Add(contentStack);

            // Add the Copy button only to ChatGPT's responses.
            contentAndButtonStack.Children.Add(copyButton);

            // Create a holder Grid to center-align the content.
            Grid holderGrid = CreateHolderGrid(backgroundColor);
            Grid.SetColumn(contentAndButtonStack, 1);
            holderGrid.Children.Add(contentAndButtonStack);
            
            // Add the holder Grid to the ChatArea.
            chatArea.Children.Add(holderGrid);

            // Set the message box component for the chatEntry.
            chatEntry.SetMessageTextBox(messageTextBox);

            chatAreaScrollViewer.ScrollToEnd();

            return chatEntry;
        }

        /// <summary>
        /// Creates and returns a TextBox to display the username in the chat.
        /// </summary>
        /// <param name="nameDisplay"> The username to be displayed. </param>
        /// <returns> A TextBox configured to display the username. </returns>
        public TextBox CreateUsernameDisplayTextBox(string nameDisplay)
        {
            TextBox nameDisplayTextBox = new TextBox
            {
                Text = nameDisplay + ":",
                FontWeight = FontWeights.Bold,
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Margin = new Thickness(10, 5, 0, 0),
                Foreground = Brushes.White,
                FontSize = 15,
                Width = 600,
                TextAlignment = TextAlignment.Left
            };

            return nameDisplayTextBox;
        }

        /// <summary>
        /// Creates and returns a TextBox to display a message in the chat.
        /// </summary>
        /// <param name="message"> The message to be displayed. </param>
        /// <returns> A TextBox configured to display the message. </returns>
        public TextBox CreateMessageTextBox(string message)
        {
            TextBox messageTextBox = new TextBox
            {
                AcceptsReturn = true,
                Text = message,
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                Margin = new Thickness(15, 20, 0, 20),
                FontSize = 15,
                TextWrapping = TextWrapping.Wrap,
                Width = 600,
                TextAlignment = TextAlignment.Left
            };

            return messageTextBox;
        }

        /// <summary>
        /// Creates and returns a Button to copy a message text to clipboard.
        /// </summary>
        /// <param name="messageText"> The message text to be copied when the button is clicked. </param>
        /// <returns> A Button configured for the copy-to-clipboard action. </returns>
        public Button CreateCopyButton(TextBox message)
        {
            Button copyButton = new Button
            {
                Margin = new Thickness(6, 0, 6, 0),
                Width = 25,
                Height = 25,
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                Style = (Style)Application.Current.Resources["RoundedButtonStyle"]
            };

            Image copyImage = new Image
            {
                Source = new BitmapImage(new Uri("/images/icon-clipboard-32.png", UriKind.Relative)),
                Width = 18,
                Height = 18,
                Stretch = Stretch.Uniform
            };

            copyButton.Content = copyImage;
            copyButton.Click += (sender, e) =>
            {
                Clipboard.SetText(message.Text);
            };

            return copyButton;
        }
    }
}
