using System.Globalization;
using System.Windows.Data;
using System;

namespace GPTLocker
{
    /// <summary>
    /// A converter class that converts a height value to a percentage of its original value.
    /// </summary>
    public class HeightPercentageConverter : IValueConverter
    {
        /// <summary>
        /// Convert a height value to its percentage.
        /// </summary>
        /// <param name="value"> The original height value. </param>
        /// <param name="targetType"> The target type of the binding target property. </param>
        /// <param name="parameter"> The percentage to convert to, as a string. </param>
        /// <param name="culture"> The culture to use in the converter. </param>
        /// <returns> The converted height value, or the original value if conversion fails. </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if the value is a double and if the parameter can be parsed to a double.
            if (value is double height && parameter is string percentageString && double.TryParse(percentageString, out double percentage))
            {
                // Calculate the height as a percentage of the original.
                return height * percentage;
            }

            return value;
        }

        /// <summary>
        /// Reverse the conversion performed by Convert.
        /// </summary>
        /// <param name="value"> The converted height value. </param>
        /// <param name="targetType"> The type of the binding target property. </param>
        /// <param name="parameter"> The original percentage as a string. </param>
        /// <param name="culture"> The culture to use in the converter. </param>
        /// <returns> The original height value, or the given value if reversal fails. </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Validate the converted height and the original percentage parameter
            if (value is double convertedHeight && parameter is string percentageString)
            {
                // Try to parse the original percentage to a double ratio
                if (double.TryParse(percentageString, out double originalPercentage))
                {
                    // Reverse-calculate the original height based on the ratio
                    var originalHeight = convertedHeight / originalPercentage;

                    return originalHeight;
                }
            }

            return value;
        }
    }
}
