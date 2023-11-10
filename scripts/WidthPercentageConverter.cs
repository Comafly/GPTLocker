using System;
using System.Globalization;
using System.Windows.Data;

namespace GPTLocker
{
    /// <summary>
    /// A converter class that adjusts a width value based on a percentage,
    /// subject to a maximum width constraint.
    /// Implements the IValueConverter interface.
    /// </summary>
    public class WidthPercentageConverter : IValueConverter
    {
        // Maximum allowed width
        private const double MaxWidth = 800;

        /// <summary>
        /// Convert a width value based on a given percentage, capped at MaxWidth.
        /// </summary>
        /// <param name="value">The original width value.</param>
        /// <param name="targetType">The target type of the binding target property.</param>
        /// <param name="parameter">The percentage as a string.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The converted width value, or the original value if conversion fails.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Validate input value and parameter.
            if (value is double width && parameter is string percentage)
            {
                if (double.TryParse(percentage, out var ratio))
                {
                    // Get the minimum value between the two.
                    var calculatedWidth = width * ratio;
                    return Math.Min(calculatedWidth, MaxWidth);
                }
            }
            return value;
        }

        /// <summary>
        /// Reverse the conversion performed by Convert.
        /// </summary>
        /// <param name="value"> The converted width value. </param>
        /// <param name="targetType"> The type of the binding target property. </param>
        /// <param name="parameter"> The original percentage as a string. </param>
        /// <param name="culture"> The culture to use in the converter. </param>
        /// <returns> The original width value, or the given value if reversal fails. </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Validate the converted width and the original percentage parameter
            if (value is double convertedWidth && parameter is string percentage)
            {
                // Try to parse the original percentage to a double ratio
                if (double.TryParse(percentage, out var ratio))
                {
                    // Reverse-calculate the original width based on the ratio
                    var originalWidth = convertedWidth / ratio;

                    // Ensure the original width does not exceed the maximum allowed value
                    return originalWidth > MaxWidth ? MaxWidth : originalWidth;
                }
            }

            return value;
        }
    }
}
