using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer;
using TrainningCenter_DataAccessLayer;
using TrainningCenter_Entities;

namespace TrainningCenter.ViewModel
{
    public class AttendanceRecordVM : INotifyPropertyChanged
    {

        public enum enAttendanceStatus
        {
            Present,
            Absent,
            Late,
            Excused,
            Canceled
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        #region Private Properties

        private int _attendanceID;
        private int _studentID;
        private int _sessionID;
        private string _fullName;
        private string _studentNumber;
        private string _initials;
        private enAttendanceStatus _attendanceStatus = enAttendanceStatus.Present;
        private TimeSpan? _arrivalTime;
        private string _notes;
        private int? _recordedBy;
        private DateTime? _recordedAt;
        private string _email;
        private string _phoneNumber;

        #endregion

        #region Public Properties

        // Primary Key
        public int AttendanceID
        {
            get => _attendanceID;
            set => SetProperty(ref _attendanceID, value);
        }

        // Foreign Keys
        public int StudentID
        {
            get => _studentID;
            set => SetProperty(ref _studentID, value);
        }

        public int SessionID
        {
            get => _sessionID;
            set => SetProperty(ref _sessionID, value);
        }

        // Student Info
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string StudentNumber
        {
            get => _studentNumber;
            set => SetProperty(ref _studentNumber, value);
        }

        public string Initials
        {
            get => _initials;
            set => SetProperty(ref _initials, value);
        }

        // Attendance Info
        public enAttendanceStatus AttendanceStatus
        {
            get => _attendanceStatus;
            set
            {
                if (SetProperty(ref _attendanceStatus, value))
                {
                    OnPropertyChanged(nameof(IsLate));
                    OnPropertyChanged(nameof(IsPresent));
                }
            }
        }

        public TimeSpan? ArrivalTime
        {
            get => _arrivalTime;
            set => SetProperty(ref _arrivalTime, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        // Audit Info
        public int? RecordedBy
        {
            get => _recordedBy;
            set => SetProperty(ref _recordedBy, value);
        }

        public DateTime? RecordedAt
        {
            get => _recordedAt;
            set => SetProperty(ref _recordedAt, value);
        }
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        #endregion

        #region Computed Properties

        public bool IsLate => AttendanceStatus == enAttendanceStatus.Late;

        public bool IsPresent => AttendanceStatus == enAttendanceStatus.Present || AttendanceStatus == enAttendanceStatus.Late;

        public bool ShowArrivalTime => AttendanceStatus == enAttendanceStatus.Present || AttendanceStatus == enAttendanceStatus.Late;
        public bool CanEdit => true;
        #endregion

        #region Constructors

        public AttendanceRecordVM() {
        
        }

        public AttendanceRecordVM(AttendanceDetailDTO dto)
        {
            if (dto == null) return;

            AttendanceID = dto.AttendanceID;
            StudentID = dto.StudentID;
            SessionID = dto.SessionID;
            FullName = dto.StudentFullName;
            StudentNumber = dto.StudentNumber;
            Initials = GetInitials(dto.StudentFullName);
            AttendanceStatus = ParseStatus(dto.AttendanceStatus);
            ArrivalTime = dto.ArrivalTime;
            Notes = dto.Notes;
            RecordedBy = dto.RecordedBy;
            RecordedAt = dto.RecordedAt;
        }

        public AttendanceRecordVM(AttendanceSession.AttendanceSessionStudent student)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            StudentID = student.StudentID;
            StudentNumber = student.StudentNumber;
            FullName = student.FullName;
            Initials = GetInitials(student.FullName);

            AttendanceID = student.AttendanceID ?? 0;
            AttendanceStatus = ParseStatus(student.AttendanceStatus);

            Email = student.Email;
            PhoneNumber = student.PhoneNumber;
        }

        public static AttendanceRecordVM FromMember(MemberVM member, int sessionId)
        {
            return new AttendanceRecordVM
            {
                StudentID = member.StudentID,
                SessionID = sessionId,
                FullName = member.MemberName,
                StudentNumber = member.StudentID.ToString(),
                Initials = member.Initials,
                Email = member.Email,
                PhoneNumber = member.PhoneNumber,
                AttendanceStatus = enAttendanceStatus.Absent
            };
        }
        #endregion

        #region Helper Methods

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return "??";

            var names = fullName.Split(' ');
            if (names.Length == 0) return "??";

            if(names.Length == 1)
                return names[0].Length >= 2 ? names[0].Substring(0, 2).ToUpper() : names[0].ToUpper();

            return (names[0][0].ToString() + names[names.Length - 1][0].ToString()).ToUpper();
        }

        #endregion

        #region Validation

        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (StudentID <= 0)
            {
                errorMessage = "Invalid Student ID";
                return false;
            }

            if (SessionID <= 0)
            {
                errorMessage = "Invalid Session ID";
                return false;
            }

            //if(string.IsNullOrWhiteSpace(AttendanceStatus))
            //{
            //    errorMessage = "Attendance Status is required";
            //    return false;
            //}

            // ArrivalTime only makes sense for Present/Late
            if (ArrivalTime.HasValue &&
                (AttendanceStatus == enAttendanceStatus.Absent || AttendanceStatus == enAttendanceStatus.Canceled))
            {
                errorMessage = $"Cannot have arrival time for status: {AttendanceStatus}";
                return false;
            }

            return true;
        }

        #endregion

        #region Convert to DTO
        private static enAttendanceStatus ParseStatus(string status)
        {
            // Handle different casing
            string normalized = status.Trim();

            return normalized.ToLower() switch
            {
                "present" => enAttendanceStatus.Present,
                "absent" => enAttendanceStatus.Absent,
                "late" => enAttendanceStatus.Late,
                "excused" => enAttendanceStatus.Excused,
                "canceled" or "cancelled" => enAttendanceStatus.Canceled,
                _ => enAttendanceStatus.Absent
            };
        }

        public CreateUpdateAttendanceDTO ToCreateUpdateDTO()
        {
            return new CreateUpdateAttendanceDTO
            {
                AttendanceID = this.AttendanceID > 0 ? (int?)this.AttendanceID : null,
                StudentID = this.StudentID,
                SessionID = this.SessionID,
                AttendanceStatus = this.AttendanceStatus.ToString(),
                ArrivalTime = this.ArrivalTime,
                Notes = this.Notes,
                RecordedBy = this.RecordedBy,
                RecordedAt = this.RecordedAt
            };
        }

        #endregion
    }
}
