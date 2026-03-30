using MahApps.Metro.IconPacks;
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
    public class AttendanceStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not enAttendanceStatus status)
                return PackIconMaterialKind.HelpCircleOutline;

            return status switch
            {
                enAttendanceStatus.Present => PackIconMaterialKind.CheckCircle,
                enAttendanceStatus.Late => PackIconMaterialKind.ClockOutline,
                enAttendanceStatus.Absent => PackIconMaterialKind.CloseCircleOutline,
                enAttendanceStatus.Excused => PackIconMaterialKind.InformationOutline,
                enAttendanceStatus.Canceled => PackIconMaterialKind.Cancel,
                _ => PackIconMaterialKind.HelpCircleOutline
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
    }
}
