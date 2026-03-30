using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer;
using TrainningCenter_Entities;

namespace TrainingCenter_BusinessLayer
{
    public class Attendance
    {
        public enum enAttendanceStatus
        {
            Present,
            Absent,
            Late,
            Excused,
            Canceled
        }

        #region Services

        public static async Task<AttendanceResultDTO> GetSessionAttendance(int SessionID)
        {
            return await clsAttendance.GetAllAttendanceBySessionID(SessionID);
        }
        public static async Task<StudentAttendanceResultDTO> GetStudentAttendance(int StudentID)
        {
            return await clsAttendance.GetAllAttendanceByStudentID(StudentID);
        }
        public static async Task<AttendanceDetailDTO> GetAttendance(int StudentID, int SessionID)
        {
            return await clsAttendance.GetAttendanceBySessionAndStudent(StudentID, SessionID);
        }

        #endregion

        #region Records

        public static async Task<AttendanceUpsertResultDTO> RecordAttendance(
            int StudentID,
            int SessionID,
            enAttendanceStatus status,
            TimeSpan? ArrivalTime,
            int RecordedBy
        )
        {
            if (StudentID <= 0 || SessionID <= 0)
                throw new ArgumentException("ID must be greater than 0");

            //if (!IsValidStatus(status))  // دالة مساعدة
            //    throw new ArgumentException($"Invalid status: {status}. Use: Present, Absent, Late, Excused");

            var dto = new CreateUpdateAttendanceDTO
            {
                StudentID = StudentID,
                SessionID = SessionID,
                AttendanceStatus = status.ToString(),
                ArrivalTime = ArrivalTime,
                RecordedBy = RecordedBy
            };

            return await clsAttendance.UpsertAttendance(dto);
        }

        #endregion

        #region Attendance Statistics

        public static async Task<AttendanceStatisticsDTO> GetAttendanceStat(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? groupID = null
        )
        {
            if (groupID <= 0)
                throw new ArgumentException("ID must be greater than 0 {groupID}", nameof(groupID));

            return await clsAttendance.GetAttendanceStatistics(startDate, endDate, groupID);
        }

        public static async Task<StudentAttendanceRateDTO> GetStudentAttendanceRate(int StudentID)
        {
            if (StudentID <= 0)
                throw new ArgumentException("ID must be greater than 0 {StudentID}", nameof(StudentID));

            return await clsAttendance.GetStudentAttendanceRate(StudentID);
        }

        #endregion
    }
}
