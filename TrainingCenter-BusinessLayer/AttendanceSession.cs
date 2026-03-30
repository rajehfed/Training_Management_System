using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainningCenter_Entities;

namespace TrainningCenter_DataAccessLayer
{
    public class AttendanceSession
    {
        public int SessionID { get; set; }
        public DateTime SessionDate { get; set; }
        public string Topic { get; set; }
        public List<AttendanceSessionStudent> Students { get; set; }
        public AttendanceStatistics Statistics { get; set; }
        public class AttendanceSessionStudent
        {
            public int StudentID { get; set; }
            public string StudentNumber { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public int? AttendanceID { get; set; }
            public string AttendanceStatus { get; set; }
            public bool CanBeMarked => string.IsNullOrEmpty(AttendanceStatus) || AttendanceStatus == "NotMarked";
        }

        public class AttendanceStatistics
        {
            public int TotalStudents { get; set; }
            public int PresentCount { get; set; }
            public int AbsentCount { get; set; }
            public int LateCount { get; set; }
            public int ExcusedCount { get; set; }
            public int NotMarkedCount { get; set; }
            public float AttendanceRate { get; set; }
        }

        public static async Task<AttendanceSession> GetSessionAttendance(int sessionID)
        {
            var dtoResult = await clsAttendance.GetAttendanceBySessionWithStudents(sessionID);

            if (!dtoResult.IsSuccess || dtoResult.Students is null)
                throw new ApplicationException(dtoResult.ErrorMessage ?? "Failed to load attendance data");

            var attendanceSession = new AttendanceSession
            {
                SessionID = dtoResult.AttendanceStatistics.SessionID,
                SessionDate = dtoResult.AttendanceStatistics.SessionDate,
                Students = dtoResult.Students.Select(s => new AttendanceSessionStudent
                {
                    StudentID = s.StudentID,
                    StudentNumber = s.StudentNumber,
                    FullName = s.FullName,
                    Email = s.Email,
                    PhoneNumber = s.Phone,
                    AttendanceID = s.AttendanceID,
                    AttendanceStatus = s.AttendanceStatus
                }).ToList(),
                Statistics = new AttendanceStatistics
                {
                    TotalStudents = dtoResult.AttendanceStatistics.TotalStudents,
                    PresentCount = dtoResult.AttendanceStatistics.PresentCount,
                    AbsentCount = dtoResult.AttendanceStatistics.AbsentCount,
                    LateCount = dtoResult.AttendanceStatistics.LateCount,
                    ExcusedCount = dtoResult.AttendanceStatistics.ExcusedCount,
                    NotMarkedCount = dtoResult.AttendanceStatistics.NotMarkedCount,
                    AttendanceRate = dtoResult.AttendanceStatistics.PresentPercentage
                }
            };

            return attendanceSession;
        }

        public async Task<bool> UpdateAttendance(int studentID, string status, string notes, int recordedBy)
        {
            var dto = new CreateUpdateAttendanceDTO
            {
                StudentID = studentID,
                SessionID = this.SessionID,
                AttendanceStatus = status,
                Notes = notes,
                RecordedBy = recordedBy,
                RecordedAt = DateTime.Now
            };

            var result = await clsAttendance.UpdateAttendance(dto);

            return result.Success;
        }
    }
}
