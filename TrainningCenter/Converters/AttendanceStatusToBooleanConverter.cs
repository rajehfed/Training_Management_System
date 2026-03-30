using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static TrainningCenter.ViewModel.AttendanceRecordVM;

namespace TrainningCenter.Converters
{
    public class AttendanceStatusToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not enAttendanceStatus status || parameter is not string target)
                return false;

            return status.ToString() == target;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value && parameter is string target)
                return Enum.Parse(typeof(enAttendanceStatus), target);

            return Binding.DoNothing;
        }
    }
}
