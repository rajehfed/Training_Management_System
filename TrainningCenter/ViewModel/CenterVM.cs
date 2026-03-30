using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;

namespace TrainningCenter.ViewModel
{
    public class CenterVM : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Helper to reduce repetitive code in Setters
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }


        #endregion

        #region Contractors

        /// <summary>
        /// Constructor for Adding a New Center
        /// </summary>
        public CenterVM()
        {
            _model = new Center();
            // Defaults
            IsActive = true;
            Capacity = 20;
        }

        /// <summary>
        /// Constructor for Editing an Existing Center
        /// </summary>
        public CenterVM(Center center)
        {
            _model = center;

            // Map Data from Model to VM
            CenterID = center.CenterID ?? 0;
            CenterName = center.CenterName;
            Address = center.Address;
            City = center.City;
            Phone = center.Phone;
            Email = center.Email;
            Manager = center.Manager;
            Capacity = center.Capacity;
            Facilities = center.Facilities;
            IsActive = center.IsActive;

            TrainersCount = center.TrainersCount;
            GroupsCount = center.GroupsCount;
        }

        #endregion

        #region Private Properties

        // 1. Wrap the Business Object
        private Center _model;

        private int _centerID;
        private string _centerName;
        private string _address;
        private string _city;
        private string _phone;
        private string _email;
        private string _manager;
        private int _capacity;
        private string _facilities;
        private bool _isActive;

        private int _trainersCount;
        private int _groupsCount;

        #endregion

        #region Public Properties

        public int CenterID
        {
            get => _centerID;
            set => SetProperty(ref _centerID, value);
        }

        public string CenterName
        {
            get => _centerName;
            set
            {
                if (SetProperty(ref _centerName, value))
                    OnPropertyChanged(nameof(Initials)); // Update Initials when name changes
            }
        }

        public string Address { get => _address; set => SetProperty(ref _address, value); }
        public string City { get => _city; set => SetProperty(ref _city, value); }
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Manager { get => _manager; set => SetProperty(ref _manager, value); }
        public int Capacity { get => _capacity; set => SetProperty(ref _capacity, value); }
        public string Facilities { get => _facilities; set => SetProperty(ref _facilities, value); }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (SetProperty(ref _isActive, value))
                {
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }

        // Computed UI Properties
        public string StatusText => IsActive ? "ACTIVE" : "INACTIVE";

        public string StatusColor => IsActive ? "#28A745" : "#DC3545"; // Green / Red

        public string DisplayLocation => $"{City} - {Address}";

        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CenterName)) return "??";
                return CenterName.Length >= 2
                    ? CenterName.Substring(0, 2).ToUpper()
                    : CenterName.ToUpper();
            }
        }

        public int TrainersCount
        {
            get => _trainersCount;
            set => SetProperty(ref _trainersCount, value);
        }

        public int GroupsCount
        {
            get => _groupsCount;
            set => SetProperty(ref _groupsCount, value);
        }

        #endregion

        #region Actions (Save / Delete)
        public async Task<bool> SaveAsync()
        {
            // 1. Map VM properties back to the Model
            _model.CenterName = this.CenterName;
            _model.Address = this.Address;
            _model.City = this.City;
            _model.Phone = this.Phone;
            _model.Email = this.Email;
            _model.Manager = this.Manager;
            _model.Capacity = this.Capacity;
            _model.Facilities = this.Facilities;
            _model.IsActive = this.IsActive;

            // 2. Call Business Layer Save
            return await _model.Save(Globals.CurrentUser.UserId);
        }

        public async Task<bool> DeleteAsync()
        {
            if (_model.CenterID.HasValue)
            {
                return await _model.Delete();
            }
            return false;
        }

        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(CenterName))
            {
                errorMessage = "Center Name is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Address))
            {
                errorMessage = "Address is required.";
                return false;
            }
            if (Capacity <= 0)
            {
                errorMessage = "Capacity must be greater than 0.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            {
                errorMessage = "Valid Email is required.";
                return false;
            }

            return true;
        }
        public override string ToString()
        {
            return CenterName;
        }
        #endregion
    }
}
