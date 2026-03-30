using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TrainningCenter.Converters
{
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        private readonly BoolToVisibilityConverter _inner = new BoolToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _inner.Convert(value, targetType, "inverse", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _inner.ConvertBack(value, targetType, "inverse", culture);
        }
    }
}
