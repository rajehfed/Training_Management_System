using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TrainningCenter.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "active" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745")),
                    "inactive" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC3545")),
                    "suspended" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800")),
                    "graduated" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999"))
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
