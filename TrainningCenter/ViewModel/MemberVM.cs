using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;

namespace TrainningCenter.ViewModel
{
    public class MemberVM : INotifyPropertyChanged
    {
        #region Private Fields

        private Member _model;

        private int _studentID;
        private int? _subscriptionID;
        private int _groupID;

        private string _memberName;
        private string _email;
        private string _phoneNumber;

        private DateTime _dateOfSubscription;
        private string _status;
        private bool _isActive;

        private decimal _amount;
        private decimal? _grade;
        private DateTime? _completionDate;

        private string _initials;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for Enrolling a New Student
        /// </summary>
        public MemberVM()
        {
            _model = new Member();

            // Defaults for new enrollment
            DateOfSubscription = DateTime.Now;
            Status = "Active";
            IsActive = true;
            Amount = 0; // Default to Pending Payment
        }

        /// <summary>
        /// Constructor for Editing an Existing Member
        /// </summary>
        public MemberVM(Member member)
        {
            _model = member;

            // Map Data
            StudentID = member.StudentID;
            SubscriptionID = member.SubscriptionID;
            GroupID = member.GroupID;

            MemberName = member.MemberName;
            Email = member.Email;
            PhoneNumber = member.PhoneNumber;

            DateOfSubscription = member.DateOfSubscription;
            Status = member.Status;
            IsActive = member.IsActive;

            Amount = member.Amount;
            Grade = member.Grade;
            CompletionDate = member.CompletionDate;
            

            Initials = GetInitialsFromName(MemberName);
        }

        #endregion

        #region Public Properties

        public int StudentID
        {
            get => _studentID;
            set => SetProperty(ref _studentID, value);
        }

        public int? SubscriptionID
        {
            get => _subscriptionID;
            set => SetProperty(ref _subscriptionID, value);
        }

        public int GroupID
        {
            get => _groupID;
            set => SetProperty(ref _groupID, value);
        }

        public string MemberName
        {
            get => _memberName;
            set
            {
                if (SetProperty(ref _memberName, value))
                    Initials = GetInitialsFromName(value);
            }
        }

        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string PhoneNumber { get => _phoneNumber; set => SetProperty(ref _phoneNumber, value); }

        public DateTime DateOfSubscription { get => _dateOfSubscription; set => SetProperty(ref _dateOfSubscription, value); }

        public string Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    OnPropertyChanged(nameof(StatusColor));
                    // Auto-update IsActive based on Status text
                    IsActive = (value != "Dropped" && value != "Cancelled");
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                if (SetProperty(ref _amount, value))
                    OnPropertyChanged(nameof(PaymentStatusText));
            }
        }

        public decimal? Grade
        {
            get => _grade;
            set
            {
                if (SetProperty(ref _grade, value))
                    OnPropertyChanged(nameof(GradeText));
            }
        }

        public DateTime? CompletionDate { get => _completionDate; set => SetProperty(ref _completionDate, value); }

        public string Initials { get => _initials; set => SetProperty(ref _initials, value); }

        // --- Computed Properties for UI ---

        public string StatusColor
        {
            get
            {
                return (Status?.ToLower()) switch
                {
                    "active" => "#28A745",      // Green
                    "completed" => "#2196F3",   // Blue
                    "dropped" => "#DC3545",     // Red
                    "cancelled" => "#6C757D",   // Gray
                    _ => "#999999"
                };
            }
        }

        public string PaymentStatusText => Amount > 0 ? "Paid" : "Pending";

        public string PaymentColor => Amount > 0 ? "#28A745" : "#FF9800"; // Green vs Orange

        public string GradeText => Grade.HasValue ? $"{Grade.Value:0.00}" : "-";

        #endregion

        #region Actions

        public async Task<bool> SaveAsync()
        {
            // 1. Map VM properties back to Model
            _model.StudentID = this.StudentID;
            _model.GroupID = this.GroupID;
            _model.DateOfSubscription = this.DateOfSubscription;
            _model.Status = this.Status;
            _model.Amount = this.Amount;
            _model.Grade = this.Grade;
            _model.CompletionDate = this.CompletionDate;
            _model.IsActive = this.IsActive;

            // 2. Call BLL
            return await _model.Save(Globals.CurrentUser.UserId);
        }

        public async Task<bool> DeleteAsync()
        {
            if (_model.SubscriptionID.HasValue)
            {
                return await _model.Delete();
            }
            return false;
        }

        #endregion

        #region Helper Methods

        private string GetInitialsFromName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "??";
            var parts = fullName.Trim().Split(' ');
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }

        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
