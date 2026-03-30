using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TrainningCenter.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts bool → Visibility
        /// </summary>
        /// <param name="value">The boolean value</param>
        /// <param name="targetType">Target type (ignored)</param>
        /// <param name="parameter">Use "inverse" to reverse logic</param>
        /// <param name="culture">Culture (ignored)</param>
        /// <returns>Visibility.Visible or Visibility.Collapsed</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Handle null or non-boolean values
            if (value is bool boolValue == false)
            {
                return Visibility.Collapsed;
            }

            // Check if inverse parameter is provided
            bool isInverted = parameter is string strParam &&
                             strParam.Trim().ToLower() == "inverse";

            // Apply inversion if needed
            if (isInverted)
            {
                boolValue = !boolValue;
            }

            // Return appropriate Visibility
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts Visibility → bool (for TwoWay binding)
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool isVisible = visibility == Visibility.Visible;

                // Check if inverse parameter is provided
                bool isInverted = parameter is string strParam &&
                                 strParam.Trim().ToLower() == "inverse";

                return isInverted ? !isVisible : isVisible;
            }

            return false;
        }
    }
}
