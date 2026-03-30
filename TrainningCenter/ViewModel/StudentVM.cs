using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using TrainingCenter_BusinessLayer;

namespace TrainningCenter.ViewModel
{
    public class StudentVM : INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor that wraps an existing Business Object
        /// </summary>
        public StudentVM(Student student)
        {
            _model = student;

            // Map Properties
            StudentID = student.StudentID ?? 0;
            PersonID = student.PersonID;
            StudentNumber = student.StudentNumber;
            AdmissionDate = student.AdmissionDate;
            StudentStatus = student.StatusName;
            EmergencyContact = student.EmergencyContact;
            IsActive = student.IsActive;
            CreatedAt = student.CreatedDate;
            UpdatedAt = student.UpdatedAt;

            if (student.PersonInfo != null)
            {
                FullName = $"{student.PersonInfo.FirstName} {student.PersonInfo.LastName}";
                Email = student.PersonInfo.Email;
                Phone = student.PersonInfo.PhoneNumber;
                Address = student.PersonInfo.Address;
                DateOfBirth = student.PersonInfo.DateOfBirth;
                Gender = student.PersonInfo.Gender == 0 ? "Male" : "Female";
            }

            Initials = GetInitialsFromName(FullName);
        }
        public StudentVM()
        {
            // FIX: Initialize the model to prevent NullReferenceException on Save
            _model = new Student();

            // Set Defaults
            AdmissionDate = DateTime.Now;
            IsActive = true;
            StudentStatus = "Active";
            _model.Status = Student.enStatus.eActive; // Sync model default
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

        #region Private Fields

        // Hold a reference to the original Business Object
        private Student _model;

        private int _studentID;
        private int _personID;
        private string _studentNumber;
        private DateTime _admissionDate;
        private string _studentStatus;
        private string _emergencyContact;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime? _updatedAt;
        private string _fullName;
        private string _email;
        private string _phone;
        private string _address;
        private DateTime? _dateOfBirth;
        private string _gender;
        private string _initials;
        private bool _isSelected;

        #endregion

        #region Public Properties

        // Primary Key
        public int StudentID
        {
            get => _studentID;
            set => SetProperty(ref _studentID, value);
        }

        // Foreign Key to Person
        public int PersonID
        {
            get => _personID;
            set => SetProperty(ref _personID, value);
        }

        // Student-Specific Fields
        public string StudentNumber
        {
            get => _studentNumber;
            set => SetProperty(ref _studentNumber, value);
        }

        public DateTime AdmissionDate
        {
            get => _admissionDate;
            set
            {
                if (SetProperty(ref _admissionDate, value))
                {
                    OnPropertyChanged(nameof(FormattedAdmissionDate));
                    OnPropertyChanged(nameof(DaysSinceAdmission));
                    OnPropertyChanged(nameof(EnrollmentDuration));
                }
            }
        }

        public string StudentStatus
        {
            get => _studentStatus;
            set
            {
                if (SetProperty(ref _studentStatus, value))
                {
                    // Auto-sync IsActive based on text
                    if (value?.ToLower() == "inactive" || value?.ToLower() == "dropped")
                        IsActive = false;
                    else
                        IsActive = true;

                    OnPropertyChanged(nameof(DisplayStatus));
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }

        public string EmergencyContact
        {
            get => _emergencyContact;
            set => SetProperty(ref _emergencyContact, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        public DateTime? UpdatedAt
        {
            get => _updatedAt;
            set => SetProperty(ref _updatedAt, value);
        }

        // Person Information (from Person table)
        public string FullName
        {
            get => _fullName;
            set
            {
                if (SetProperty(ref _fullName, value))
                {
                    // Auto-update initials when name changes
                    Initials = GetInitialsFromName(value);
                }
            }
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Phone
        {
            get => _phone;
            set
            {
                if (SetProperty(ref _phone, value))
                {
                    OnPropertyChanged(nameof(FormattedPhone));
                }
            }
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value);
        }

        public string Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        // UI Helper Properties
        public string Initials
        {
            get => _initials;
            set => SetProperty(ref _initials, value);
        }

        public string StatusColor => GetStatusColor();

        public string DisplayStatus
        {
            get
            {
                // Priority: If IsActive is false, show "Inactive" regardless of the status text
                if (!IsActive) return "INACTIVE";

                return StudentStatus?.ToUpper() ?? "UNKNOWN";
            }
        }

        public string FormattedAdmissionDate => AdmissionDate.ToString("dd MMM yyyy");

        public string FormattedPhone => FormatPhoneNumber(Phone);

        // Computed Properties
        public int DaysSinceAdmission => (DateTime.Now - AdmissionDate).Days;

        public string EnrollmentDuration
        {
            get
            {
                var days = DaysSinceAdmission;
                if (days < 30) return $"{days} days";
                if (days < 365) return $"{days / 30} months";
                return $"{days / 365} years";
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        public async Task<bool> SaveAsync()
        {
            // 1. Update the Model
            _model.PersonID = this.PersonID;
            _model.StudentNumber = this.StudentNumber;
            _model.AdmissionDate = this.AdmissionDate;
            _model.StatusName = this.StudentStatus;
            _model.EmergencyContact = this.EmergencyContact;
            _model.IsActive = this.IsActive;

            // 2. Call BL Save
            return await _model.Save();
        }

        public async Task<bool> DeleteAsync()
        {
            if (_model.StudentID.HasValue)
            {
                // We are calling the static delete method from DAL directly 
                // OR if you added a Delete() method to Student.cs, call _model.Delete()
                return await _model.Delete();
            }

            return false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Maps ViewModel back to Business Layer object for database operations
        /// </summary>
        public async Task<Student> MapFromViewModel()
        {
            try
            {
                var student = await Student.Find(this.StudentID);

                if (student == null)
                {
                    return null;
                }

                // Update student properties
                student.StudentNumber = this.StudentNumber;
                student.AdmissionDate = this.AdmissionDate;
                student.StatusName = this.StudentStatus;
                student.EmergencyContact = this.EmergencyContact;
                student.IsActive = this.IsActive;

                return student;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error mapping StudentVM to Student: {ex.Message}", ex);
            }
        }

        //public async Task<bool> Delete()
        //{
        //    return true;
        //}

        /// <summary>
        /// Creates a new Student object from ViewModel
        /// </summary>
        public Student CreateNewStudent(int createdByUserId = 0)
        {
            return new Student
            {
                PersonID = this.PersonID,
                StudentNumber = this.StudentNumber,
                AdmissionDate = this.AdmissionDate,
                StatusName = this.StudentStatus,
                EmergencyContact = this.EmergencyContact,
                IsActive = this.IsActive,
                CreatedDate = DateTime.Now,
                //CreatedByUserID = createdByUserId
            };
        }

        /// <summary>
        /// Gets initials from full name for avatar
        /// </summary>
        private string GetInitialsFromName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "??";

            var parts = fullName.Trim().Split(' ');
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }

        /// <summary>
        /// Formats phone number for display
        /// </summary>
        private string FormatPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return "N/A";

            // Remove all non-numeric characters
            string cleaned = new string(phone.Where(char.IsDigit).ToArray());

            // Format based on length
            if (cleaned.Length == 10)
                return $"({cleaned.Substring(0, 3)}) {cleaned.Substring(3, 3)}-{cleaned.Substring(6)}";

            return phone; // Return original if can't format
        }

        /// <summary>
        /// Gets the appropriate color for the student status
        /// </summary>
        private string GetStatusColor()
        {
            // Priority: If IsActive is false, return Red
            if (!IsActive) return "#DC3545"; // Red

            return (StudentStatus?.ToLower()) switch
            {
                "active" => "#28A745",      // Green
                "inactive" => "#DC3545",    // Red
                "suspended" => "#FF9800",   // Orange
                "graduated" => "#2196F3",   // Blue
                _ => "#999999"              // Gray
            };
        }

        /// <summary>
        /// Validates the student data
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(StudentNumber))
            {
                errorMessage = "Student number is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(FullName))
            {
                errorMessage = "Full name is required";
                return false;
            }

            if (AdmissionDate > DateTime.Now)
            {
                errorMessage = "Admission date cannot be in the future";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                errorMessage = "Email is required";
                return false;
            }

            // Email validation (basic)
            if (!Email.Contains("@") || !Email.Contains("."))
            {
                errorMessage = "Invalid email format";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a copy of this student
        /// </summary>
        public StudentVM Clone()
        {
            return new StudentVM(_model);
        }

        public override string ToString()
        {
            return $"{StudentNumber} - {FullName} ({StudentStatus})";
        }

        /// <summary>
        /// Reloads the ViewModel properties from the Model after an edit.
        /// </summary>
        public void RefreshFromModel()
        {
            // Re-map all properties from _model to VM
            StudentID = _model.StudentID ?? 0;
            PersonID = _model.PersonID;
            StudentNumber = _model.StudentNumber;
            AdmissionDate = _model.AdmissionDate;
            StudentStatus = _model.StatusName;
            EmergencyContact = _model.EmergencyContact;
            IsActive = _model.IsActive;
            UpdatedAt = _model.UpdatedAt;

            // Refresh Person Info if available
            if (_model.PersonInfo != null)
            {
                FullName = $"{_model.PersonInfo.FirstName} {_model.PersonInfo.LastName}";
                Email = _model.PersonInfo.Email;
                Phone = _model.PersonInfo.PhoneNumber;
                Address = _model.PersonInfo.Address;
                DateOfBirth = _model.PersonInfo.DateOfBirth;
                Gender = _model.PersonInfo.Gender == 0 ? "Male" : "Female";
            }

            // Trigger Property Changes for computed values
            OnPropertyChanged(nameof(Initials));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(DisplayStatus));
        }

        public async Task ReloadAsync()
        {
            // 1. Get fresh data from Database
            var freshModel = await Student.Find(this.StudentID);

            if (freshModel != null)
            {
                // 2. Replace the internal model
                _model = freshModel;

                // 3. Update all UI properties
                RefreshFromModel();
            }
        }

        #endregion
    }
}
