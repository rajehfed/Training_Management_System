using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TrainingCenter_BusinessLayer;

namespace TrainningCenter.ViewModel
{
    public class TrainerVM : INotifyPropertyChanged
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

        #region Private Field

        // The wrapped model
        private Trainer _model;

        // VM Fields
        private int _trainerID;
        private int _personID;
        private string _fullName;
        private string _email;
        private string _phone;
        private string _address;
        private DateTime? _dateOfBirth;
        private string _gender;
        private string _qualifications;
        private string _specialization;
        private DateTime _hireDate;
        private decimal _salary;
        private decimal _rating;
        private string _biography;
        private bool _isActive;
        private int? _centerID;
        private string _centerName;
        private string _initials;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for Adding a New Trainer
        /// </summary>
        public TrainerVM()
        {
            _model = new Trainer();
            // Defaults
            HireDate = DateTime.Now;
            IsActive = true;
            Rating = 5.0m;
        }

        /// <summary>
        /// Constructor for Editing an Existing Trainer
        /// </summary>
        public TrainerVM(Trainer trainer)
        {
            _model = trainer;

            // Map Trainer Data
            TrainerID = trainer.TrainerID ?? 0;
            PersonID = trainer.CurrentPerson.Id;
            ExperienceYears = trainer.ExperienceYears;
            Qualifications = trainer.Qualifications;
            Specialization = trainer.Specialization;
            HireDate = trainer.HireDate;
            Salary = trainer.Salary;
            Rating = trainer.Rating;
            Biography = trainer.Biography;
            CenterID = trainer.CenterID;
            CenterName = trainer.CenterName;
            IsActive = trainer.IsActive;

            // Map Person Data (Nested)
            if (trainer.CurrentPerson != null)
            {
                FullName = $"{trainer.CurrentPerson.FirstName} {trainer.CurrentPerson.LastName}";
                Email = trainer.CurrentPerson.Email;
                Phone = trainer.CurrentPerson.PhoneNumber;
                Address = trainer.CurrentPerson.Address;
                DateOfBirth = trainer.CurrentPerson.DateOfBirth;
                Gender = trainer.CurrentPerson.Gender == 0 ? "Male" : "Female";
            }

            Initials = GetInitialsFromName(FullName);
        }

        /// <summary>
        /// Creates a Trainer from TrainerVM (for saving new/updated trainers)
        /// </summary>
        public static Trainer ToTrainer(TrainerVM vm)
        {
            if (vm == null) return null;

            var trainer = new Trainer();

            // Map basic trainer properties
            trainer.TrainerID = vm.TrainerID > 0 ? vm.TrainerID : (int?)null;
            trainer.PersonID = vm.PersonID;
            trainer.ExperienceYears = vm.ExperienceYears;
            trainer.Qualifications = vm.Qualifications;
            trainer.Specialization = vm.Specialization;
            trainer.HireDate = vm.HireDate;
            trainer.Salary = vm.Salary;
            trainer.Rating = vm.Rating;
            trainer.Biography = vm.Biography;
            trainer.CenterID = vm.CenterID;
            trainer.IsActive = vm.IsActive;

            // If Person data is being updated through trainer
            if (trainer.CurrentPerson != null)
            {
                // Parse FullName back to First/Last
                var nameParts = vm.FullName?.Split(' ') ?? new[] { "", "" };
                trainer.CurrentPerson.FirstName = nameParts[0];
                trainer.CurrentPerson.LastName = nameParts.Length > 1 ? nameParts[1] : "";
                trainer.CurrentPerson.Email = vm.Email;
                trainer.CurrentPerson.PhoneNumber = vm.Phone;
                trainer.CurrentPerson.Address = vm.Address;
                trainer.CurrentPerson.DateOfBirth = vm.DateOfBirth.Value;
                trainer.CurrentPerson.Gender = vm.Gender == "Male"
                    ? Person.enGender.Male
                    : Person.enGender.Female;
            }

            return trainer;
        }

        #endregion

        #region Public Properties

        public int TrainerID
        {
            get => _trainerID;
            set => SetProperty(ref _trainerID, value);
        }

        public int PersonID
        {
            get => _personID;
            set => SetProperty(ref _personID, value);
        }

        public string FullName
        {
            get => _fullName;
            set { if (SetProperty(ref _fullName, value)) Initials = GetInitialsFromName(value); }
        }

        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Phone { get => _phone; set { if (SetProperty(ref _phone, value)) OnPropertyChanged(nameof(FormattedPhone)); } }
        public string Address { get => _address; set => SetProperty(ref _address, value); }
        public DateTime? DateOfBirth { get => _dateOfBirth; set { if (SetProperty(ref _dateOfBirth, value)) OnPropertyChanged(nameof(Age)); } }
        public string Gender { get => _gender; set => SetProperty(ref _gender, value); }

        public int ExperienceYears { get; set; } // Auto-property is fine if no logic needed in setter
        public string Qualifications { get => _qualifications; set => SetProperty(ref _qualifications, value); }
        public string Specialization { get => _specialization; set => SetProperty(ref _specialization, value); }

        public DateTime HireDate
        {
            get => _hireDate;
            set
            {
                if (SetProperty(ref _hireDate, value))
                {
                    OnPropertyChanged(nameof(FormattedHireDate));
                    OnPropertyChanged(nameof(ExperienceText));
                }
            }
        }

        public decimal Salary { get => _salary; set => SetProperty(ref _salary, value); }
        public decimal Rating { get => _rating; set => SetProperty(ref _rating, value); }
        public string Biography { get => _biography; set => SetProperty(ref _biography, value); }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (SetProperty(ref _isActive, value))
                {
                    OnPropertyChanged(nameof(StatusColor));
                    OnPropertyChanged(nameof(DisplayStatus));
                }
            }
        }

        public int? CenterID { get => _centerID; set => SetProperty(ref _centerID, value); }
        public string CenterName { get => _centerName; set => SetProperty(ref _centerName, value); }
        public string Initials { get => _initials; set => SetProperty(ref _initials, value); }

        // Computed
        public string DisplayStatus => IsActive ? "ACTIVE" : "INACTIVE";
        public string StatusColor => IsActive ? "#28A745" : "#DC3545"; // Green / Red
        public string FormattedHireDate => HireDate.ToString("dd MMM yyyy");
        public string FormattedPhone => FormatPhoneNumber(Phone);

        public int? Age
        {
            get
            {
                if (!DateOfBirth.HasValue) return null;
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Value.Year;
                if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        public string ExperienceText
        {
            get
            {
                var years = (DateTime.Now - HireDate).TotalDays / 365.25;
                int y = (int)Math.Floor(years);
                if (y <= 0) return "Less than 1 year";
                return $"{y} year{(y > 1 ? "s" : "")}";
            }
        }

        #endregion

        #region Actions

        public async Task<bool> SaveAsync()
        {
            // 1. Update Model
            _model.PersonID = this.PersonID;
            _model.ExperienceYears = this.ExperienceYears;
            _model.Qualifications = this.Qualifications;
            _model.Specialization = this.Specialization;
            _model.HireDate = this.HireDate;
            _model.Salary = this.Salary;
            _model.Rating = this.Rating;
            _model.Biography = this.Biography;
            _model.CenterID = this.CenterID;
            _model.IsActive = this.IsActive;

            // 2. Call Save
            return await _model.Save();
        }

        public async Task<bool> DeleteAsync()
        {
            if (_model.TrainerID.HasValue)
            {
                return await _model.Delete();
            }
            return false;
        }

        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;
            if (PersonID <= 0) { errorMessage = "Person must be selected"; return false; }
            if (string.IsNullOrWhiteSpace(Specialization)) { errorMessage = "Specialization is required"; return false; }
            if (HireDate > DateTime.Now) { errorMessage = "Hire date cannot be in future"; return false; }
            if (Salary < 0) { errorMessage = "Salary cannot be negative"; return false; }
            return true;
        }

        #endregion

        #region Helper Methods (Private)

        private string GetInitialsFromName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "??";
            var parts = fullName.Trim().Split(' ');
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }

        private string FormatPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "N/A";
            string cleaned = new string(phone.Where(char.IsDigit).ToArray());
            if (cleaned.Length == 10) return $"({cleaned.Substring(0, 3)}) {cleaned.Substring(3, 3)}-{cleaned.Substring(6)}";
            return phone;
        }

        #endregion

        public override string ToString()
        {
            return FullName; // This is what the ComboBox will show by default
        }
    }
}
