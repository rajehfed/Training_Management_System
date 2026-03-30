using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer;

namespace TrainningCenter.ViewModel
{
    public class StudentMemberVM : INotifyPropertyChanged
    {
        // Data derived from the Student Table
        private int _studentID;
        public int StudentID
        {
            get => _studentID;
            set { _studentID = value; OnPropertyChanged(); }
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); OnPropertyChanged(nameof(Initials)); }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        private DateTime? _dateOfSubscription;
        public DateTime? DateOfSubscription
        {
            get => _dateOfSubscription;
            set { _dateOfSubscription = value; OnPropertyChanged(); }
        }

        private int? _subscriptionID;
        public int? SubscriptionID
        {
            get => _subscriptionID;
            set { _subscriptionID = value; OnPropertyChanged(); }
        }

        private string _subscriptionStatus;
        public string SubscriptionStatus
        {
            get => _subscriptionStatus;
            set { _subscriptionStatus = value; OnPropertyChanged(); }
        }

        private DateTime? _subscriptionEndDate;
        public DateTime? SubscriptionEndDate
        {
            get => _subscriptionEndDate;
            set { _subscriptionEndDate = value; OnPropertyChanged(); }
        }

        private decimal? _subscriptionAmount;
        public decimal? SubscriptionAmount
        {
            get => _subscriptionAmount;
            set { _subscriptionAmount = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Calculates the initials for the avatar border.
        /// </summary>
        /// <summary>
        /// Gets initials from full name for avatar display
        /// </summary>
        public string Initials
        {
            get
            {
                if (string.IsNullOrEmpty(FullName)) return "?";

                var parts = FullName.Split(' ');
                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[1][0]}".ToUpper();

                return FullName.Substring(0, Math.Min(2, FullName.Length)).ToUpper();
            }
        }

        // <summary>
        /// Indicates if subscription is active
        /// </summary>
        public bool IsActive => SubscriptionStatus?.ToLower() == "active";

        /// <summary>
        /// Indicates if subscription is expiring soon (within 7 days)
        /// </summary>
        public bool IsExpiringSoon
        {
            get
            {
                if (SubscriptionEndDate == null) return false;
                return (SubscriptionEndDate.Value - DateTime.Now).TotalDays <= 7;
            }
        }

        // -------------------------------------------------------------------
        // MAPPING / FACTORY METHOD
        // -------------------------------------------------------------------

        /// <summary>
        /// Creates a StudentMemberVM from the Business Layer Member object.
        /// This is the bridge between the BLL and the UI layer.
        /// </summary>
        public static StudentMemberVM CreateFromMember(Member member)
        {
            if (member == null)
            {
                return null;
            }

            return new StudentMemberVM
            {
                StudentID = member.StudentID,
                FullName = member.MemberName,
                Email = member.Email,
                Phone = member.PhoneNumber,

                DateOfSubscription = member.DateOfSubscription,
                SubscriptionID = member.SubscriptionID,
                SubscriptionStatus = member.Status,
                SubscriptionEndDate = member.CompletionDate,
                SubscriptionAmount = member.Amount,
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // INOTIFYPROPERTYCHANGED IMPLEMENTATION
        // ═══════════════════════════════════════════════════════════════
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
