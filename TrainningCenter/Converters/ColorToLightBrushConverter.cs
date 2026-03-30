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
    public class ColorToLightBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && hex.StartsWith("#"))
            {
                try
                {
                    // إزالة علامة # إذا كانت موجودة
                    hex = hex.TrimStart('#');

                    // إضافة opacity منخفضة (30 = ~19%)
                    string hexWithOpacity = "30" + hex;

                    // تحويل hex إلى Color
                    Color color = (Color)ColorConverter.ConvertFromString("#" + hexWithOpacity);

                    // إنشاء SolidColorBrush
                    return new SolidColorBrush(color);
                }
                catch
                {
                    return Brushes.Transparent;
                }
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
