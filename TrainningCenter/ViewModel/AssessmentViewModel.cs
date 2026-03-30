using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Diagnostics;
using System.Windows.Input;
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter_Entities;

namespace TrainningCenter.ViewModel
{
    /// <summary>
    /// ViewModel for Assessment Library management
    /// </summary>
    public class AssessmentViewModel : INotifyPropertyChanged
    {
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

        #region Constructors
        public AssessmentViewModel()
        {
            _model = new Assessment();
        }
        public AssessmentViewModel(Assessment model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            AssessmentID = model.AssessmentID;
            SpecializationID = model.SpecializationID ?? -1;
            SpecializationName = model.SpecializationName;
            AssessmentName = model.AssessmentName;
            AssessmentType = model.AssessmentType;
            TotalMarks = model.TotalMark;
            PassingMark = model.PassingMark;
            Duration = model.Duration;
            Instructions = model.Instructions;
            IsActive = model.IsActive;
            CreatedByUserID = model.CreatedBy;
            CreatedByUserName = model.CreatedByName;
            CreatedAt = model.CreatedAt;
        }
        #endregion

        #region Properties
        private Assessment _model;

        private int? _assessmentID;
        public int? AssessmentID
        {
            get => _assessmentID;
            set => SetProperty(ref _assessmentID, value);
        }

        private int _specializationID;
        public int SpecializationID
        {
            get => _specializationID;
            set => SetProperty(ref _specializationID, value);
        }

        private string _specializationName;
        public string SpecializationName
        {
            get => _specializationName;
            set => SetProperty(ref _specializationName, value);
        }

        private string _assessmentName = string.Empty;
        public string AssessmentName
        {
            get => _assessmentName;
            set => SetProperty(ref _assessmentName, value);
        }

        private enAssessmentType? _assessmentType;
        public enAssessmentType? AssessmentType
        {
            get => _assessmentType;
            set
            {
                SetProperty(ref _assessmentType, value);
                OnPropertyChanged();
            }
        }

        public string TypeDisplay => AssessmentType.ToString();

        private decimal? _totalMark;
        public decimal? TotalMarks
        {
            get => _totalMark;
            set
            {
                SetProperty(ref _totalMark, value);
                OnPropertyChanged();
            }
        }

        private decimal? _passingMark;
        public decimal? PassingMark
        {
            get => _passingMark;
            set
            {
                SetProperty(ref _passingMark, value);
                OnPropertyChanged();
            }
        }

        private string _instructions;
        public string Instructions
        {
            get => _instructions;
            set
            {
                SetProperty(ref _instructions, value);
                OnPropertyChanged();
            }
        }

        public decimal? PassingPercentage =>
            TotalMarks == 0 ? 0 : Math.Round(Convert.ToDecimal((PassingMark / TotalMarks)) * 100, 2);

        private TimeSpan? _duration;
        public TimeSpan? Duration
        {
            get => _duration;
            set
            {
                SetProperty(ref _duration, value);
                OnPropertyChanged();
            }
        }

        public string DurationDisplay =>
            Duration.HasValue ? Duration.Value.ToString(@"hh\:mm") : "N/A";

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                SetProperty(ref _isActive, value);
                OnPropertyChanged();
            }
        }

        public string StatusDisplay => IsActive ? "Active" : "Inactive";

        private int? _createdByUserID;
        public int? CreatedByUserID
        {
            get => _createdByUserID;
            set => SetProperty(ref _createdByUserID, value);
        }

        private string _createdByUserName;
        public string CreatedByUserName
        {
            get => _createdByUserName;
            set => SetProperty(ref _createdByUserName, value);
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }
        #endregion

        #region Methods
        public async Task<(bool success, string message)> SaveAsync()
        {
            if (!IsValid(out string validationError))
            {
                return (false, validationError);
            }

            try
            {
                var specialization = await Specialization.Find(SpecializationID);
                if (specialization is null)
                    return (false, $"Selected SpecializationID {SpecializationID} does not exist.");

                _model.AssessmentID = AssessmentID;
                if (!AssessmentID.HasValue)
                {
                    _model.CreatedBy = Globals.CurrentUser.UserId;
                    _model.CreatedByName = Globals.CurrentUser.Username;
                }
                _model.SpecializationID = SpecializationID;
                _model.SpecializationName = SpecializationName;
                _model.AssessmentName = AssessmentName;
                _model.AssessmentType = AssessmentType;
                _model.TotalMark = TotalMarks;
                _model.PassingMark = PassingMark;
                _model.Duration = Duration;
                _model.IsActive = IsActive;
                _model.Instructions = Instructions;
                _model.IsActive = IsActive;

                return await _model.SaveAsync(Globals.CurrentUser.UserId);
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"An error occurred while saving the assessment:", ex);
                return (false, ex.Message);
            }
        }
        public async Task<(bool success, string message)> DeleteAsync()
        {
            try
            {
                if (_model.AssessmentID.HasValue)
                    return await _model.DeactivateAsync();

                return (false, "Assessment ID is not set. Cannot Deactivate.");
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"An error occurred while deleting the assessment:", ex);
                return (false, $"An error occurred while deleting the assessment: {AssessmentID}");
            }
        }
        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(AssessmentName))
            {
                errorMessage = "Assessment Name is required.";
                return false;
            }

            if (TotalMarks <= 0)
            {
                errorMessage = "Total Mark must be greater than zero.";
                return false;
            }

            if (PassingMark <= 0)
            {
                errorMessage = "Passing Mark must be greater than zero.";
                return false;
            }

            if (PassingMark > TotalMarks)
            {
                errorMessage = "Passing Mark cannot be greater than Total Mark.";
                return false;
            }

            if (Duration.HasValue && Duration.Value.TotalMinutes <= 0)
            {
                errorMessage = "Duration must be greater than zero.";
                return false;
            }

            if (AssessmentType < enAssessmentType.Presentation || AssessmentType > enAssessmentType.TheoryExam)
            {
                errorMessage = "Invalid Assessment Type.";
                return false;
            }

            return true;
        }
        #endregion
    }
}
