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
    public class GroupVM : INotifyPropertyChanged
    {
        // ----------------------------------------------------
        // 1. INotifyPropertyChanged Implementation
        // ----------------------------------------------------

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Call this method to raise the PropertyChanged event.
        /// The [CallerMemberName] attribute automatically supplies the property name.
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// A generic setter method to simplify updating backing fields and raising PropertyChanged.
        /// </summary>
        protected bool SetProperty<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, newValue))
            {
                return false;
            }

            backingField = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        #region Properties
        public int GroupID { get; set; }
        public int SpecializationID { get; set; }
        public int TrainerID { get; set; }
        public int CenterID { get; set; }

        private string _groupName;
        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }
        private int _studentCount;
        public int StudentCount
        {
            get => _studentCount;
            set => SetProperty(ref _studentCount, value);
        }
        private int _maxTrainees;
        public int MaxTrainees
        {
            get => _maxTrainees;
            set => SetProperty(ref _maxTrainees, value);
        }
        private string _trainerName;
        public string TrainerName
        {
            get => _trainerName;
            set => SetProperty(ref _trainerName, value);
        }
        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }
        private string _statusName;
        /// <summary>
        /// The textual representation of the group status (e.g., "Active", "Suspended").
        /// </summary>
        public string StatusName
        {
            get => _statusName;
            set => SetProperty(ref _statusName, value);
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        private string _schedule;
        public string Schedule
        {
            get => _schedule;
            set => SetProperty(ref _schedule, value);
        }

        private string _room;
        public string Room
        {
            get => _room;
            set => SetProperty(ref _room, value);
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        private DateTime _lastUpdateAt;
        public DateTime LastUpdateAt
        {
            get => _lastUpdateAt;
            set => SetProperty(ref _lastUpdateAt, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        private decimal _baseFee;
        public decimal BaseFee
        {
            get => _baseFee;
            set => SetProperty(ref _baseFee, value);
        }
        #endregion

        #region ComboBox Data Sources

        private IReadOnlyList<Center> _centersList;
        public IReadOnlyList<Center> CentersList
        {
            get => _centersList;
            set => SetProperty(ref _centersList, value);
        }



        private List<Specialization> _specializationsList;
        public List<Specialization> SpecializationsList
        {
            get => _specializationsList;
            set => SetProperty(ref _specializationsList, value);
        }

        private List<Trainer> _trainersList;
        public List<Trainer> TrainersList
        {
            get => _trainersList;
            set => SetProperty(ref _trainersList, value);
        }

        #endregion

        public static async Task<List<GroupVM>> LoadAllGroups()
        {
            List<Group> groups = await Group.GetAllGroups();

            return groups.Select(g => new GroupVM
            {
                GroupID = g.GroupID ?? 0,
                SpecializationID = g.SpecializationID,
                TrainerID = g.TrainerID,
                CenterID = g.CenterID,
                GroupName = g.GroupName,
                StudentCount = g.CurrentTrainees,
                MaxTrainees = g.MaxTrainees,
                StartDate = g.StartDate,
                EndDate = g.EndDate,
                StatusName = g.StatusName,
                IsActive = g.IsActive,
                Schedule = g.Schedule,
                Room = g.Room,
                CreatedAt = g.CreatedAt,
                LastUpdateAt = g.LastUpdateAt,
                BaseFee = g.BaseFee
            }).ToList();
        }
        public async Task LoadComboBoxSourcesAsync()
        {
            CentersList = await Center.GetAllCenters();
            SpecializationsList = await Specialization.GetAllSpecializations();
            TrainersList = await Trainer.GetAllTrianers(); // Note: Your method name has a typo, should be GetAllTrainers
        }

        public async Task<Group> MapFromViewModel()
        {
            Group group;

            // ----------------------------------------------------------------------------------
            // Note on GroupID: The BLL's GroupID is nullable (int?). Setting it to null 
            // (or -1, which triggers 'AddNew' in the BLL's constructor) correctly handles Add/Edit mode.
            // We use the BLL object's Status conversion method.
            // ----------------------------------------------------------------------------------

            if (this.GroupID == 0)
            {
                // ADD MODE: Create a new, empty Group object.
                group = new Group();
                group.CreatedAt = DateTime.Now; // Set creation date only for new groups
            }
            else
            {
                group = await Group.Find(this.GroupID);
                if (group == null)
                {
                    // This is a safety check in case the group was deleted by another user.
                    throw new InvalidOperationException($"Group with ID {this.GroupID} not found.");
                }
            }

            group.SpecializationID = this.SpecializationID;
            group.CenterID = this.CenterID;
            group.TrainerID = this.TrainerID;
            group.GroupName = this.GroupName;
            group.CurrentTrainees = this.StudentCount;
            group.MaxTrainees = this.MaxTrainees;
            group.StartDate = this.StartDate;
            group.EndDate = this.EndDate;
            group.Status = group.ConvertStatusStringToEnum(this.StatusName);
            group.Schedule = this.Schedule;
            group.Room = this.Room;
            group.IsActive = this.IsActive;
            group.LastUpdateAt = DateTime.Now; // Always set the last update timestamp

            return group; // <-- Final return statement

        }

        public override string ToString()
        {
            return GroupName;
        }
    }
}