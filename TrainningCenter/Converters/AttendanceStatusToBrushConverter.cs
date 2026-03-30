using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using static TrainningCenter.ViewModel.AttendanceRecordVM;

namespace TrainningCenter.Converters
{
    public class AttendanceStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not enAttendanceStatus status)
                return Brushes.Gray;

            return status switch
            {
                enAttendanceStatus.Present => Brushes.Green,
                enAttendanceStatus.Late => Brushes.Orange,
                enAttendanceStatus.Absent => Brushes.Red,
                enAttendanceStatus.Excused => Brushes.Blue,
                enAttendanceStatus.Canceled => Brushes.Gray,
                _ => Brushes.Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value && parameter is string target)
                return Enum.Parse(typeof(enAttendanceStatus), target);

            return Binding.DoNothing;
        }
    }
}
