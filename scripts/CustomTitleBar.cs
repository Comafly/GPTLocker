using System.Windows.Input;
using System.Windows;

namespace GPTLocker
{
    /// <summary>
    /// Represents a custom title bar for a Window object.
    /// </summary>
    public class CustomTitleBar
    {
        readonly Window window;

        /// <summary>
        /// Initializes a new instance of the CustomTitleBar class.
        /// </summary>
        /// <param name="window"> The Window object to which this custom title bar is attached. </param>
        public CustomTitleBar(Window window)
        {
            this.window = window;
        }

        /// <summary>
        /// Minimizes the window when the minimize button is clicked.
        /// </summary>
        public void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            window.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Maximizes or restores the window when the maximize button is clicked.
        /// </summary>
        public void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (window.WindowState == WindowState.Maximized)
                window.WindowState = WindowState.Normal;
            else
                window.WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// Closes the window when the close button is clicked.
        /// </summary>
        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            window.Close();
        }

        /// <summary>
        /// Enables dragging of the window from the title bar area.
        /// </summary>
        public void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                window.DragMove();
        }
    }
}
