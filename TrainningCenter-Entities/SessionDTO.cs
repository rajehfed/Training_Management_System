using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter_Entities
{
    public class SessionDTO
    {
        // الخصائص الأساسية
        public int? SessionID { get; set; }
        public int GroupID { get; set; }
        public int SpecializationID { get; set; }
        public DateTime SessionDate { get; set; }
        // CHANGED: Use TimeSpan for accurate mapping of SQL TIME type
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SessionType { get; set; }
        public string Location { get; set; }
        public string Topic { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedByUserID { get; set; }
        public string GroupName { get; set; }

        // المدرب (قد يكون NULL)
        public int? TrainerID { get; set; }
        public string TrainerFirstName { get; set; }
        public string TrainerLastName { get; set; }

        // الإحصائيات لكل جلسة
        public int TotalStudents { get; set; } // Count of attendance records, not group enrollment
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }

        // خاصية محسوبة (اختيارية)
        public decimal AttendancePercentage =>
            TotalStudents > 0 ? (PresentCount * 100m / TotalStudents) : 0;
    }

    public class SessionsInfoDTO
    {
        public List<SessionDTO> Sessions { get; set; } = new List<SessionDTO>();

        // إجماليات عامة (محسوبة)
        public int TotalSessions => Sessions?.Count ?? 0;
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }

        // إحصائيات محسوبة (اختيارية)
        public decimal OverallAttendancePercentage =>
            TotalStudents > 0 ? (PresentCount * 100m / TotalStudents) : 0;
    }
    public class SessionInfoDTO
    {
        public int SessionID { get; set; }
        public DateTime SessionDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Topic { get; set; }
        public string Status { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public int ExpectedAttendance { get; set; }
    }
}
