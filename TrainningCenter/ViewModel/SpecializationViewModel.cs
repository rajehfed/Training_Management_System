using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TrainningCenter.ViewModel
{
    public class SpecializationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _specializationName;
        public string SpecializationName
        {
            get => _specializationName;
            private set
            {
                _specializationName = value;
                OnPropertyChanged();
            }
        }
        private string _specializationDescription;
        public string SpecializationDescription
        {
            get => _specializationDescription;
            private set
            {
                _specializationDescription = value;
                OnPropertyChanged();
            }
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            private set
            {
                _createdAt = value;
                OnPropertyChanged();
            }
        }
        private int _studentCount;
        public int StudentCount
        {
            get => _studentCount;
            set { _studentCount = value; OnPropertyChanged(); }
        }

        private int _groupCount;
        public int GroupCount
        {
            get => _groupCount;
            set { _groupCount = value; OnPropertyChanged(); }
        }

        private double _completionRate;
        public double CompletionRate
        {
            get => _completionRate;
            set { _completionRate = value; OnPropertyChanged(); }
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }
        private Brush _statusForeground;
        public Brush StatusForeground
        {
            get => _statusForeground;
            set { _statusForeground = value; OnPropertyChanged(); }
        }

        private Brush _statusBackground;
        public Brush StatusBackground
        {
            get => _statusBackground;
            set { _statusBackground = value; OnPropertyChanged(); }
        }

        private string _difficultyText;
        public string DifficultyText
        {
            get => _difficultyText;
            set { _difficultyText = value; OnPropertyChanged(); }
        }

        private Brush _difficultyForeground;
        public Brush DifficultyForeground
        {
            get => _difficultyForeground;
            set { _difficultyForeground = value; OnPropertyChanged(); }
        }

        private Brush _difficultyBackground;
        public Brush DifficultyBackground
        {
            get => _difficultyBackground;
            set { _difficultyBackground = value; OnPropertyChanged(); }
        }

        private PackIconMaterialKind _difficultyIcon;
        public PackIconMaterialKind DifficultyIcon
        {
            get => _difficultyIcon;
            set { _difficultyIcon = value; OnPropertyChanged(); }
        }
        public ObservableCollection<GroupVM> StudentGourps { get; private set; } = new ObservableCollection<GroupVM>();
        public SpecializationViewModel(int specializationID)
        {
            _ = LoadSpecializationDetailsAsync(specializationID);
        }
        private async Task LoadSpecializationDetailsAsync(int specializationID)
        {
            try
            {
                var specialization = await Specialization.Find(specializationID);

                this.SpecializationName = specialization.SpecializationName;
                this.SpecializationDescription = specialization.Description;
                this.CreatedAt = specialization.CreatedAt;

                SetStatusDisplay(specialization.IsActive);
                SetDifficultyDisplay(specialization.Level);

                // load the list data
                var groups = await Group.GetGroupsBySpecialization(specialization.SpecializationID);
                foreach (var group in groups)
                {
                    this.StudentGourps.Add(
                        new GroupVM {
                            GroupName = group.GroupName,
                            StudentCount = group.CurrentTrainees
                        });
                }

                GroupCount = StudentGourps.Count;
                StudentCount = StudentGourps.Sum(s => s.StudentCount);
            }
            catch(Exception ex)
            {
                EventLogger.LogError("Error loading specialization", ex);
                MessageBox.Show(
                    $"An Error Loading Specialization: \n{ex.Message}",
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
        private void SetStatusDisplay(bool isActive)
        {
            if (isActive)
            {
                this.StatusText = "Active";
                this.StatusBackground = Utils.GetBrush("SuccessBrush") ?? Brushes.Green;
                this.StatusForeground = Utils.GetBrush("SuccessLightColor") ?? Brushes.GreenYellow;
            }
            else
            {
                this.StatusText = "Inactive";
                this.StatusBackground = Utils.GetBrush("TextDisabledBrush") ?? Brushes.Gray;
                this.StatusForeground = Utils.GetBrush("PinkColor") ?? Brushes.GreenYellow;
            }
        }
        private void SetDifficultyDisplay(string difficultyLevel)
        {
            switch (difficultyLevel)
            {
                case "Beginner":
                    this.DifficultyText = "Beginner";
                    this.DifficultyIcon = MahApps.Metro.IconPacks.PackIconMaterialKind.SeedOutline;
                    this.DifficultyForeground = Utils.GetBrush("InfoBrush") ?? Brushes.Blue;
                    DifficultyBackground = Brushes.Transparent;
                    break;

                case "Intermediate":
                    this.DifficultyText = "Intermidiate";
                    this.DifficultyIcon = MahApps.Metro.IconPacks.PackIconMaterialKind.Fire;
                    this.DifficultyForeground = Utils.GetBrush("WarningBrush") ?? Brushes.Orange;
                    DifficultyBackground = Brushes.Transparent;
                    break;

                case "Advanced":
                    this.DifficultyText = "Advenced";
                    this.DifficultyIcon = MahApps.Metro.IconPacks.PackIconMaterialKind.Rocket;
                    this.DifficultyForeground = Utils.GetBrush("DangerBrush") ?? Brushes.Red;
                    DifficultyBackground = Brushes.Transparent;
                    break;

                default:
                    break;
            }
        }
    }
}
