using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer.Helpers;
using TrainningCenter_DataAccessLayer;
using TrainningCenter_Entities;

namespace TrainingCenter_BusinessLayer
{
    public class Session
    {
        public enum enMode { AddNew = 0, Update = 1 }
        public enum enSessionType
        {
            Project,
            Exam,
            Workshop,
            Practical,
            Theory
        }
        public enum enStatus
        {
            Postponed,
            Cancelled,
            Completed,
            Scheduled
        }
        private enMode _Mode = enMode.AddNew;

        #region Properties
        // Principale Informations
        public int? SessionID { get; set; }
        public DateTime SessionDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public enSessionType SessionType { get; set; }
        public string Location { get; set; }
        public string Topic { get; set; }
        public enStatus SessionStatus { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedByUserID { get; set; }

        // Group Informations
        public int? GroupID { get; set; }
        public int? SpecializationID { get; set; }
        public string GroupName { get; set; }

        // Trainer Information
        public int? TrainerID { get; set; }
        public string TrainerFirstName { get; set; }
        public string TrainerLastName { get; set; }
        public string TrainerFullName => $"{TrainerFirstName} {TrainerLastName}";

        // Inforamtions for Every Session.
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }

        // خاصية محسوبة (اختيارية)
        public decimal AttendancePercentage =>
            TotalStudents > 0 ? (PresentCount * 100m / TotalStudents) : 0;

        public string SessionStatusName => SessionStatus.ToString();
        public string SessionTypeName => SessionType.ToString();
        #endregion

        #region Constractors
        public Session()
        {
            SessionID = null;
            StartTime = TimeSpan.Zero;
            EndTime = TimeSpan.Zero;
            SessionType = enSessionType.Project;
            Location = null;
            Topic = null;
            SessionStatus = enStatus.Scheduled;
            Notes = null;
            CreatedAt = DateTime.Now;
            CreatedByUserID = null;
            IsActive = true;

            GroupID = null;
            GroupName = null;
            SpecializationID = null;

            TrainerID = null;
            TrainerFirstName = null;
            TrainerLastName = null;

            TotalStudents = 0;
            PresentCount = 0;
            AbsentCount = 0;
            LateCount = 0;
            ExcusedCount = 0;

            _Mode = enMode.AddNew;
        }
        public Session(SessionDTO dto)
        {
            if(dto is null)
                throw new ArgumentNullException(nameof(dto));

            SessionID = dto.SessionID;
            StartTime = dto.StartTime;
            EndTime = dto.EndTime;
            SessionType = GetSesstionTypeEnum(dto.SessionType);
            Location = dto.Location;
            Topic = dto.Topic;
            SessionStatus = GetStatusEnum(dto.Status);
            Notes = dto.Notes;
            CreatedAt = dto.CreatedAt;
            CreatedByUserID = dto.CreatedByUserID;
            IsActive = dto.IsActive;

            GroupID = dto.GroupID;
            GroupName = dto.GroupName;
            SpecializationID = dto.SpecializationID;

            TrainerID = dto.TrainerID;
            TrainerFirstName = dto.TrainerFirstName;
            TrainerLastName = dto.TrainerLastName;

            TotalStudents = dto.TotalStudents;
            PresentCount = dto.PresentCount;
            AbsentCount = dto.AbsentCount;
            LateCount = dto.LateCount;
            ExcusedCount = dto.ExcusedCount;
            SessionDate = dto.SessionDate;

            _Mode = enMode.Update;
        }

        #endregion

        #region Factor Methodes
        public static async Task<SessionsInfoDTO> GetAllSessions()
        {
            return await clsSession.GetAll();
        }

        public static async Task<Session> Find(int SessionID)
        {
            if (SessionID <= 0)
                throw new ArgumentException(
                    $"Session ID must be a positive integer. Provided value: {SessionID}",
                    nameof(SessionID)
                );
            try
            {
                var TargetSession = await clsSession.GetSessionByID(SessionID);

                if (TargetSession == null)
                    return null;

                return new Session(TargetSession);
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"An Error Ocured When Try To Retieve the Data Of Session ID :{SessionID}", ex);
                throw;
            }
        }

        public static async Task<IReadOnlyList<Session>> GetSessinosByGroupID(int GroupID)
        {
            if (GroupID <= 0)
                throw new ArgumentException(
                    $"Group ID must be a positive integer. Provided value: {GroupID}",
                    nameof(GroupID)
                );

            try
            {
                var TargetSessions = await clsSession.GetSessionsByGroupID(GroupID);

                if (TargetSessions.Count == 0)
                    return new List<Session>().AsReadOnly();

                return TargetSessions.Select(s => new Session(s)).ToList().AsReadOnly();
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"An Error Ocured When Try To Retieve the Session Data of Group ID :{GroupID}", ex);
                throw;
            }
        }
        #endregion

        #region Helpers And Mappers

        private SessionDTO _MapFromCurrentObject()
        {

            // التحقق من القيم المطلوبة
            if (!this.GroupID.HasValue || this.GroupID <= 0)
                throw new InvalidOperationException("Valid GroupID is required for session.");

            if (!this.SpecializationID.HasValue || this.SpecializationID <= 0)
                throw new InvalidOperationException("Valid SpecializationID is required for session.");


            // SessionID قد يكون null في حالة الإنشاء الجديد
            int? sessionID = this.SessionID.HasValue ? this.SessionID.Value : (int?)null;

            return new SessionDTO
            {
                SessionID = sessionID,
                GroupID = this.GroupID.Value,
                SpecializationID = this.SpecializationID.Value,
                SessionDate = this.SessionDate,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                SessionType = this.SessionType.ToString(),
                Location = this.Location,
                Topic = this.Topic,
                Status = this.SessionStatus.ToString(),
                Notes = this.Notes,
                IsActive = this.IsActive,
                CreatedAt = this.CreatedAt,
                GroupName = this.GroupName,
                TrainerID = this.TrainerID,
                TrainerFirstName = this.TrainerFirstName,
                TrainerLastName = this.TrainerLastName,
                TotalStudents = this.TotalStudents,
                PresentCount = this.PresentCount,
                AbsentCount = this.AbsentCount,
                LateCount = this.LateCount,
                ExcusedCount = this.ExcusedCount
            };
        }
        private bool Validate()
        {
            if (!GroupID.HasValue || GroupID <= 0)
                throw new InvalidOperationException("Group is required");

            if (StartTime >= EndTime)
                throw new InvalidOperationException("Start time must be before end time");

            if (SessionDate.Date < DateTime.Today)
                throw new InvalidOperationException("Session date cannot be in the past");

            if (string.IsNullOrWhiteSpace(Topic))
                throw new InvalidOperationException("Topic is required");

            return true;
        }

        public enStatus GetStatusEnum(string status)
        {
            if (Enum.TryParse<enStatus>(status, true, out var result))
                return result;

            throw new ArgumentException($"Invalid status: {status}");
        }
        public enSessionType GetSesstionTypeEnum(string sessionType)
        {
            if (Enum.TryParse<enSessionType>(sessionType, true, out var result))
            {
                return result;
            }

            throw new ArgumentException($"Invalid session type: {sessionType}");
        }

        #endregion

        #region CRUD Operations

        private async Task<bool> _AddNewSession()
        {
            try
            {
                int? newSessionID = await clsSession.AddNewSession(this._MapFromCurrentObject());

                if (newSessionID != null)
                {
                    this.SessionID = newSessionID;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                EventLogger.LogError("An Error Ocured When Try To Add New Session", ex);
                throw;
            }
        }
        private async Task<bool> _UpdateSession()
        {
            try
            {
                var resultSet = await clsSession.UpdateSession(this._MapFromCurrentObject());

                if (resultSet.Success)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"An Error Ocured When Try To Update the Data Of Session ID :{this.SessionID}", ex);
                throw;
            }
        }
        public async Task<bool> Save()
        {
            Validate();

            switch (_Mode)
            {
                case enMode.AddNew:
                    if (await this._AddNewSession())
                    {
                        CreatedAt = DateTime.Now;
                        _Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return await _UpdateSession();
            }

            return false;
        }

        public async Task<bool> CancelSession(int? CancelledByUser = null, string CancellationReason = null)
        {
            if(!this.SessionID.HasValue)
                throw new InvalidOperationException("Session must be saved first.");

            var resultFromDB = await clsSession.CancelSession(this.SessionID.Value, CancelledByUser, CancellationReason);

            if (resultFromDB.Success)
            {
                SessionStatus = enStatus.Cancelled;
                IsActive = false;
                return true;
            }

            return false;
        }
        #endregion
    }
}
