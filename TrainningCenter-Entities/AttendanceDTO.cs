using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter_Entities
{
    public class CreateUpdateAttendanceDTO
    {
        public int? AttendanceID { get; set; }
        public int StudentID { get; set; }
        public int SessionID { get; set; }
        public string AttendanceStatus { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
        public string Notes { get; set; }
        public int? RecordedBy { get; set; }
        public DateTime? RecordedAt { get; set; }
    }

    public class AttendanceListItemDTO
    {
        public int AttendanceID { get; set; }
        public string AttendanceStatus { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
        public DateTime RecordedAt { get; set; }

        // Student Info 
        public string StudentFullName { get; set; }
        public string StudentNumber { get; set; }

        // Session Info
        public DateTime SessionDate { get; set; }
        public string SessionTopic { get; set; }

        // Quick Stats
        public bool IsLate => AttendanceStatus == "Late";
        public int? LateMinutes { get; set; }
    }

    public class AttendanceDetailDTO
    {
        public int AttendanceID { get; set; }
        public string AttendanceStatus { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
        public string Notes { get; set; }
        public int? RecordedBy { get; set; }
        public DateTime? RecordedAt { get; set; }
        public string SessionType { get; set; }
        public string SessionLocation { get; set; }
        public string SessionStatus { get; set; }
        public int? TrainerID { get; set; }
        public string TrainerName { get; set; }
        public string GroupName { get; set; }
        public int? SpecializationID { get; set; }
        public int? LateMinutes { get; set; }

        // Student Details
        public int StudentID { get; set; }
        public string StudentNumber { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string StudentFullName => $"{StudentFirstName} {StudentLastName}";
        public string StudentEmail { get; set; }
        public string StudentPhone { get; set; }

        // Session Details
        public int SessionID { get; set; }
        public DateTime? SessionDate { get; set; }
        public TimeSpan? SessionStartTime { get; set; }
        public TimeSpan? SessionEndTime { get; set; }
        public string SessionTopic { get; set; }

        public bool AttendanceExists { get; set; }
    }

    public class AttendanceOperationResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? AttendanceID { get; set; }
        public string Action { get; set; } // "Created" or "Updated"
        public DateTime Timestamp { get; set; }
    }

    public class AttendanceResultDTO
    {
        public List<AttendanceDetailDTO> Attendances { get; set; }
        public int TotalAttendanceRecords { get; set; }
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
        public double AttendancePercentage { get; set; }
        public int? AverageLateMinutes { get; set; }
    }

    public class StudentAttendanceResultDTO
    {
        public int StudentID { get; set; }
        public List<AttendanceDetailDTO> Attendances { get; set; }
        public int TotalSessionsAttended { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
        public double AttendancePercentage { get; set; }
        public double? AverageLateMinutes { get; set; }
    }

    public class AttendanceUpsertResultDTO
    {
        public bool Success { get; set; }
        public int? AttendanceID { get; set; }
        public string Action { get; set; } // "Inserted" | "Updated"
        public string ErrorMessage { get; set; }
    }
    public class AttendanceDeleteResultDTO
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class AttendanceStatisticsDTO
    {
        public int TotalSessions { get; set; }
        public int TodaySessions { get; set; }
        public int CompletedSessions { get; set; }
        public int UpcomingSessions { get; set; }

        public int TotalAttendanceRecords { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLate { get; set; }
        public int TotalExcused { get; set; }

        public double AttendanceRate { get; set; }
    }

    public class StudentAttendanceRateDTO
    {
        public int StudentID { get; set; }
        public int TotalSessionsRecorded { get; set; }
        public int PresentCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public int ExcusedCount { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class SessionAttendanceStudentDTO
    {
        public int StudentID { get; set; }
        public string StudentNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // Attendance Info
        public int? AttendanceID { get; set; }
        public string AttendanceStatus { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
        public string Notes { get; set; }
        public int? RecordedBy { get; set; }
        public DateTime? RecordedAt { get; set; }

        // Session Info
        public int SessionID { get; set; }  
        public DateTime SessionDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Topic { get; set; }

        // Subscription Info
        public int? SubscriptionID { get; set; }
        public string SubscriptionStatus { get; set; }
        public DateTime? DateOfSubscription { get; set; }
        public DateTime? CompletionDate { get; set; }

        // Usefull Information
        public bool HasAttendanceRecord { get; set; }
        public int? DaysSinceSubscription { get; set; }
    }

    public class SessionAttendanceStatisticsDTO
    {
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
        public int NotMarkedCount { get; set; }

        // النسب المئوية
        public float PresentPercentage { get; set; }
        public float AbsentPercentage { get; set; }
        public float LatePercentage { get; set; }
        public float ExcusedPercentage { get; set; }
        public float NotMarkedPercentage { get; set; }

        // معلومات الجلسة
        public int SessionID { get; set; }
        public DateTime SessionDate { get; set; }
        public string SessionStatus { get; set; }
    }

    public class SessionAttendanceFullResultDTO
    {
        public List<SessionAttendanceStudentDTO> Students { get; set; }
        public SessionAttendanceStatisticsDTO AttendanceStatistics { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}
