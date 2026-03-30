using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter_Entities;

namespace TrainningCenter.ViewModel
{
    public class SessionVM : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

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

        #region Private Fields

        private Session _model;

        private int? _sessionID;
        private int _groupID;
        private DateTime _sessionDate;
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        //private string _sessionType;
        private Session.enSessionType _sessionType;
        private string _location;
        private string _topic;
        private int? _trainerID;
        private Session.enStatus _statusEnum;
        private string _status;
        private string _notes;
        private bool _isActive;

        private string _groupName;
        private string _trainerName;
        private int _totalStudents;
        private int _presentCount;
        private int _lateCount;
        private int _absentCount;
        private int _excusedCount;

        private static readonly Brush _scheduledColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
        private static readonly Brush _completedColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745"));
        private static readonly Brush _cancelledColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC3545"));
        private static readonly Brush _postponedColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107"));

        #endregion

        #region Constractor

        public SessionVM(Session model)
        {
            _model = model;

            MapFromModel(model);
        }
        static SessionVM()
        {
            _scheduledColor.Freeze();
            _completedColor.Freeze();
            _cancelledColor.Freeze();
            _postponedColor.Freeze();
        }
        public SessionVM(SessionDTO dto)
        {
            _model = new Session(dto);

            MapFromModel(_model);
        }
        public SessionVM()
        {
            _model = new Session();

            SessionDate = DateTime.Today;
            StartTime = new TimeSpan(9, 0, 0);
            EndTime = new TimeSpan(11, 0, 0);
            StatusEnum = Session.enStatus.Scheduled;
            IsActive = true;
        }

        #endregion

        #region Properties

        public int? SessionID
        {
            get => _sessionID;
            set => SetProperty(ref _sessionID, value);
        }

        public int GroupID
        {
            get => _groupID;
            set
            {
                if (SetProperty(ref _groupID, value))
                    _model.GroupID = value;
            }
        }

        public DateTime SessionDate
        {
            get => _sessionDate;
            set
            {
                if (SetProperty(ref _sessionDate, value))
                {
                    OnPropertyChanged(nameof(FormattedDate));
                    OnPropertyChanged(nameof(IsToday));
                    OnPropertyChanged(nameof(IsPast));
                    OnPropertyChanged(nameof(IsFuture));
                    _model.SessionDate = value;
                }
            }
        }

        public TimeSpan StartTime
        {
            get => _startTime;
            set
            {
                if (SetProperty(ref _startTime, value))
                {
                    OnPropertyChanged(nameof(Duration));
                    _model.StartTime = value;
                }
            }
        }

        public TimeSpan EndTime
        {
            get => _endTime;
            set
            {
                if (SetProperty(ref _endTime, value))
                {
                    OnPropertyChanged(nameof(Duration));
                    _model.EndTime = value;
                }
            }
        }

        public Session.enSessionType SessionType
        {
            get => _sessionType;
            set
            {
                if (SetProperty(ref _sessionType, value))
                    _model.SessionType = value;
            }
        }

        public string Location
        {
            get => _location;
            set
            {
                if(SetProperty(ref _location, value))
                    _model.Location = value;
            }
        }

        public string Topic
        {
            get => _topic;
            set
            {
                if (SetProperty(ref _topic, value))
                {
                    _model.Topic = value;
                }
            }
        }

        public int? TrainerID
        {
            get => _trainerID;
            set 
            { 
                if(SetProperty(ref _trainerID, value))
                    _model.TrainerID = value;
            }
        }

        public Session.enStatus StatusEnum
        {
            get => _statusEnum;
            set
            {
                if (SetProperty(ref _statusEnum, value))
                {
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(StatusColor));
                    OnPropertyChanged(nameof(CanTakeAttendance));
                    OnPropertyChanged(nameof(IsExpired));
                    _model.SessionStatus = value;
                }
            }
        }
        public string Status => StatusEnum.ToString();

        public string Notes
        {
            get => _notes;
            set
            {
                if(SetProperty(ref _notes, value))
                    _model.Notes = value;
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        // Related Properties
        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        public string TrainerName
        {
            get => _trainerName;
            set => SetProperty(ref _trainerName, value);
        }

        public int TotalStudents
        {
            get => _totalStudents;
            set
            {
                if (SetProperty(ref _totalStudents, value))
                {
                    OnPropertyChanged(nameof(AttendancePercentage));
                }
            }
        }

        public int PresentCount
        {
            get => _presentCount;
            set
            {
                if (SetProperty(ref _presentCount, value))
                {
                    OnPropertyChanged(nameof(AttendancePercentage));
                }
            }
        }

        public int AbsentCount
        {
            get => _absentCount;
            set => SetProperty(ref _absentCount, value);
        }

        public int LateCount
        {
            get => _lateCount;
            set => SetProperty(ref _lateCount, value);
        }

        public int ExcusedCount
        {
            get => _excusedCount;
            set => SetProperty(ref _excusedCount, value);
        }

        #endregion

        #region Computed Properties

        public string FormattedDate => SessionDate.ToString("MMM dd, yyyy");

        public string Duration
        {
            get
            {
                var duration = EndTime - StartTime;
                if (duration.TotalHours >= 1)
                    return $"{duration.TotalHours:F1} hrs";
                else
                    return $"{duration.TotalMinutes} min";
            }
        }

        public bool IsToday => SessionDate.Date == DateTime.Today;

        public bool IsPast => SessionDate.Date < DateTime.Today;

        public bool IsFuture => SessionDate.Date > DateTime.Today;

        public bool CanTakeAttendance =>
            StatusEnum != Session.enStatus.Completed &&
            StatusEnum != Session.enStatus.Cancelled;

        public int AttendancePercentage
        {
            get
            {
                if (TotalStudents == 0) return 0;
                return (int)((double)PresentCount / TotalStudents * 100);
            }
        }

        public bool IsTimePassed =>
            DateTime.Now > SessionDate.Add(EndTime);

        public bool IsExpired =>
            StatusEnum == Session.enStatus.Completed ||
            StatusEnum == Session.enStatus.Cancelled;

        public Brush StatusColor
        {
            get
            {
                return StatusEnum switch
                {
                    Session.enStatus.Scheduled => _scheduledColor,
                    Session.enStatus.Completed => _completedColor,
                    Session.enStatus.Cancelled => _cancelledColor,
                    Session.enStatus.Postponed => _postponedColor,
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999"))
                };
            }
        }

        public string AttendanceSummary => $"{PresentCount} Present, {AbsentCount} Absent, {LateCount} Late";

        #endregion

        #region Methods

        private void MapFromModel(Session session)
        {
            SessionID = session.SessionID;
            GroupID = session.GroupID ?? 0;
            SessionDate = session.SessionDate;
            StartTime = session.StartTime;
            EndTime = session.EndTime;
            SessionType = session.SessionType;
            Location = session.Location;
            Topic = session.Topic;
            TrainerID = session.TrainerID;
            StatusEnum = session.SessionStatus;
            Notes = session.Notes;
            IsActive = session.IsActive;

            // Load related data
            //_ = LoadRelatedData();
        }

        private async Task LoadRelatedData()
        {
            try
            {
                if (GroupID > 0)
                {
                    var group = await Group.Find(GroupID);
                    GroupName = group?.GroupName ?? "N/A";
                }

                if (TrainerID.HasValue)
                {
                    var trainer = await Trainer.Find(TrainerID.Value);
                    TrainerName = trainer != null ? trainer.TrainerName : "N/A";
                }

                if (SessionID.HasValue)
                {
                    var attendanceRecords = await Attendance.GetSessionAttendance(SessionID.Value);

                    if (attendanceRecords != null)
                    {
                        TotalStudents = attendanceRecords.TotalStudents;
                        PresentCount = attendanceRecords.PresentCount;
                        AbsentCount = attendanceRecords.AbsentCount;
                        LateCount = attendanceRecords.LateCount;
                        ExcusedCount = attendanceRecords.ExcusedCount;
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error loading related data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                EventLogger.LogError($"Error loading related data: {ex.Message}");
            }
        }

        public async Task<(bool success, string error)> SaveAsync(int createdByUserID = 0)
        {
            // Here i changed the scoop of the GetSesstionTypeEnum and GetStatusEnum to public
            if (!IsValid(out string validationError))
            {
                return (false, validationError);
            }
            try
            {
                // 1. الحصول على معلومات المجموعة
                var group = await Group.Find(GroupID);
                if (group == null)
                {
                    return (false, $"Group with ID {GroupID} not found");
                }

                // 2. التحقق من أن للمجموعة SpecializationID
                if (group.SpecializationID <= 0)
                {
                    return (false, $"Selected group does not have a valid specialization");
                }

                _model.GroupID = GroupID;
                _model.SpecializationID = group.SpecializationID;
                _model.SessionDate = SessionDate;
                _model.StartTime = StartTime;
                _model.EndTime = EndTime;
                _model.SessionType = SessionType;
                _model.Location = Location;
                _model.Topic = Topic;
                _model.TrainerID = TrainerID;
                _model.SessionStatus = (Session.enStatus)StatusEnum;
                _model.Notes = Notes;
                _model.IsActive = IsActive;
                _model.CreatedByUserID = createdByUserID > 0 ? createdByUserID : Globals.CurrentUser?.UserId ?? 0;

                bool result = await _model.Save();
                return (result, string.Empty);
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error saving session: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                EventLogger.LogError($"Error saving session:", ex);
                return (false, ex.Message);
            }
        }

        public async Task<bool> DeleteAsync(string CancellationReason = "")
        {
            try
            {
                if (_model.SessionID.HasValue)
                    return await _model.CancelSession(Globals.CurrentUser.UserId, CancellationReason);

                return false;
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error deleting session: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                EventLogger.LogError($"Error deleting session", ex);
                return false;
            }
        }

        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (GroupID <= 0)
            {
                errorMessage = "Please select a group";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Topic))
            {
                errorMessage = "Topic is required";
                return false;
            }

            if (SessionDate.Date < DateTime.Today)
            {
                errorMessage = "Session date cannot be in the past";
                return false;
            }

            if (EndTime <= StartTime)
            {
                errorMessage = "End time must be after start time";
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"{Topic} - {GroupName} ({FormattedDate})";
        }

        public Task InitializeAsync() => LoadRelatedData();
        #endregion
    }
}
