using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using static TrainningCenter.ViewModel.AttendanceRecordVM;

namespace TrainningCenter.Converters
{
    public class AttendanceStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is enAttendanceStatus status)
                return status == enAttendanceStatus.Present || status == enAttendanceStatus.Late
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
    }
}
