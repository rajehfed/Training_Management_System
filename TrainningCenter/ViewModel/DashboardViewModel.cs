using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter.Views.Windows;
using TrainningCenter.Views.Windows.Assessments;
using TrainningCenter.Views.Windows.Groups;
using TrainningCenter.Views.Windows.Sessions;
using TrainningCenter_Entities;

namespace TrainningCenter.ViewModel
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        #region Fields

        private string _username = "Admin";
        private string _welcomeMessage = "";
        private string _totalStudents = "0";
        private string _studentGrowth = "+0%";
        private string _activeGroups = "0";
        private string _sessionsToday = "0";
        private string _revenue = "$0";
        private string _revenueGrowth = "+0%";
        private string _attendanceRate = "0%";
        private bool _isLoading = false;

        #endregion

        #region Properties

        public string UserName
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set { _welcomeMessage = value; OnPropertyChanged(); }
        }

        public string TotalStudents
        {
            get => _totalStudents;
            set { _totalStudents = value; OnPropertyChanged(); }
        }

        public string StudentGrowth
        {
            get => _studentGrowth;
            set { _studentGrowth = value; OnPropertyChanged(); }
        }

        public string ActiveGroups
        {
            get => _activeGroups;
            set { _activeGroups = value; OnPropertyChanged(); }
        }

        public string SessionsToday
        {
            get => _sessionsToday;
            set { _sessionsToday = value; OnPropertyChanged(); }
        }

        public string Revenue
        {
            get => _revenue;
            set { _revenue = value; OnPropertyChanged(); }
        }

        public string RevenueGrowth
        {
            get => _revenueGrowth;
            set { _revenueGrowth = value; OnPropertyChanged(); }
        }

        public string AttendanceRate
        {
            get => _attendanceRate;
            set { _attendanceRate = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TodaySessionModelDTO> TodaySchedule { get; set; }

        public ObservableCollection<ActivityItemVM> RecentActivities { get; set; }
        public ObservableCollection<EventItemVM> UpcomingEvents { get; set; }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand NavigateToStudentsCommand { get; }
        public ICommand NavigateToGroupsCommand { get; }
        public ICommand NavigateToPaymentsCommand { get; }
        public ICommand NavigateToSessionsCommand { get; }
        public ICommand AddStudentCommand { get; }
        public ICommand CreateGroupCommand { get; }
        public ICommand ScheduleSessionCommand { get; }
        public ICommand TakeAttendanceCommand { get; }
        public ICommand CreateAssessmentCommand { get; }
        public ICommand ViewReportsCommand { get; }

        #endregion

        #region Constructor

        public DashboardViewModel()
        {
            TodaySchedule = new ObservableCollection<TodaySessionModelDTO>();
            RecentActivities = new ObservableCollection<ActivityItemVM>();
            UpcomingEvents = new ObservableCollection<EventItemVM>();

            // Initialize commands
            RefreshCommand = new RelayCommand(async () => await LoadDashboardDataAsync());
            NavigateToStudentsCommand = new RelayCommand<string>(NavigateToPage);
            NavigateToGroupsCommand = new RelayCommand<string>(NavigateToPage);
            NavigateToPaymentsCommand = new RelayCommand<string>(NavigateToPage);
            NavigateToSessionsCommand = new RelayCommand<string>(NavigateToPage);
            AddStudentCommand = new RelayCommand(AddStudent);
            CreateGroupCommand = new RelayCommand(CreateGroup);
            ScheduleSessionCommand = new RelayCommand(ScheduleSession);
            TakeAttendanceCommand = new RelayCommand(TakeAttendance);
            CreateAssessmentCommand = new RelayCommand(CreateAssessment);
            ViewReportsCommand = new RelayCommand(ViewReports);

            // Set welcome message based on time
            SetWelcomeMessage();

            // Load data
            _ = LoadDashboardDataAsync();
        }

        #endregion

        #region Data Loading

        public async Task LoadDashboardDataAsync()
        {
            IsLoading = true;

            try
            {
                var statsTask = LoadStatisticsAsync();
                var scheduleTask = LoadTodayScheduleAsync();
                var activitiesTask = DashboardStatistics.GetActivityLogs();
                var eventsTask = DashboardStatistics.GetUpcomingEvents();

                await Task.WhenAll(statsTask, scheduleTask, activitiesTask, eventsTask);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    RecentActivities.Clear();
                    foreach(var log in activitiesTask.Result)
                    {
                        RecentActivities.Add(MapLogToVM(log));
                    }

                    UpcomingEvents.Clear();
                    foreach(var ev in eventsTask.Result)
                    {
                        UpcomingEvents.Add(MapEventToVM(ev));
                    }
                });
            }
            catch(Exception ex)
            {
                EventLogger.LogError("Error loading dashboard data", ex);
                MessageBox.Show(
                    "An error occurred while loading dashboard data. Please try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        private async Task LoadStatisticsAsync()
        {
            try
            {
                var stats = await DashboardStatistics.GetStatisticsAsync();

                // Update stat cards
                TotalStudents = stats.TotalStudents.ToString("N0");
                ActiveGroups = stats.ActiveGroups.ToString();
                SessionsToday = stats.SessionsToday.ToString();
                Revenue = stats.RevenueThisMonth.ToString("C0");
                AttendanceRate = stats.AttendanceRate.ToString("F1") + "%";

                // Update growth percentages
                StudentGrowth = FormatGrowthPercentage(stats.StudentGrowthPercentage);
                RevenueGrowth = FormatGrowthPercentage(stats.RevenueGrowthPercentage);
            }
            catch(Exception ex)
            {
                EventLogger.LogError("Error loading statistics", ex);
                throw;
            }
        }
        private async Task LoadTodayScheduleAsync()
        {
            try
            {
                var schedule = await DashboardStatistics.GetScheduleStatistics();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TodaySchedule.Clear();
                    foreach (var session in schedule)
                    {
                        TodaySchedule.Add(session);
                    }
                });
            }
            catch(Exception ex)
            {
                EventLogger.LogError("Error loading today's schedule", ex);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private ActivityItemVM MapLogToVM(ActivityLogDTO log)
        {
            var vm = new ActivityItemVM
            {
                Title = $"{log.ActionType} in {log.TableName}",
                Description = log.Details,
                TimeAgo = CalculateTimeAgo(log.ActionDate)
            };

            // Setup Icons and Colors based on ActionType
            switch (log.ActionType.ToUpper())
            {
                case "ADDNEW":
                case "INSERT":
                    vm.Icon = PackIconMaterialKind.PlusCircle;
                    vm.Color = (Brush)Application.Current.Resources["SuccessBrush"] ?? Brushes.Green;
                    vm.BgColor = (Brush)Application.Current.Resources["SuccessLightBrush"] ?? new SolidColorBrush(Color.FromArgb(40, 0, 255, 0));
                    break;
                case "UPDATE":
                case "EDIT":
                    vm.Icon = PackIconMaterialKind.PencilCircle;
                    vm.Color = (Brush)Application.Current.Resources["InfoBrush"] ?? Brushes.Blue;
                    vm.BgColor = (Brush)Application.Current.Resources["InfoLightBrush"] ?? new SolidColorBrush(Color.FromArgb(40, 0, 0, 255));
                    break;
                case "DELETE":
                    vm.Icon = PackIconMaterialKind.MinusCircle;
                    vm.Color = Brushes.Red;
                    vm.BgColor = new SolidColorBrush(Color.FromArgb(40, 255, 0, 0));
                    break;
                default:
                    vm.Icon = PackIconMaterialKind.Clock;
                    vm.Color = Brushes.Gray;
                    vm.BgColor = new SolidColorBrush(Color.FromArgb(30, 128, 128, 128));
                    break;
            }

            return vm;
        }
        private EventItemVM MapEventToVM(UpcomingEventDTO ev)
        {
            var brushConverter = new BrushConverter();
            return new EventItemVM
            {
                Title = ev.Title,
                DateRange = ev.EndDate.HasValue
                    ? $"{ev.StartDate:MMM dd} - {ev.EndDate:dd, yyyy}"
                    : ev.StartDate.ToString("MMMM dd, yyyy"),
                Subtitle = ev.Category,
                MainColor = (Brush)brushConverter.ConvertFromString(ev.MainColor ?? "#6200EE"),
                BgColor = (Brush)brushConverter.ConvertFromString(ev.BgColor ?? "#F3E5F5")
            };
        }
        private string CalculateTimeAgo(DateTime dateTime)
        {
            TimeSpan timeSpan = DateTime.Now.Subtract(dateTime);
            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} min ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} days ago";
            return dateTime.ToString("MMM dd, yyyy");
        }

        private void SetWelcomeMessage()
        {
            var hour = DateTime.Now.Hour;

            if (hour < 12)
                WelcomeMessage = "Good morning! Ready to inspire minds today? ☀️";
            else if (hour < 17)
                WelcomeMessage = "Good afternoon! Your dedication is making a difference 🌟";
            else if (hour < 21)
                WelcomeMessage = "Good evening! Another productive day at the center 🌙";
            else
                WelcomeMessage = "Working late? Your commitment is admirable 🌃";
        }

        private string FormatGrowthPercentage(decimal percentage)
        {
            if (percentage > 0)
                return $"+{percentage:F1}%";
            else if (percentage < 0)
                return $"{percentage:F1}%";
            else
                return "0%";
        }

        #endregion

        #region Navigation Methods

        private void NavigateToPage(string pageName)
        {
            MessageBox.Show($"Navigate to {pageName}", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Quick Action Methods

        private void AddStudent()
        {
            // TODO: Open Add Student dialog
            MessageBox.Show(
                "Opening Add Student dialog...", 
                "Quick Action", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);

            var addStudentDialog = new AddEditStudent();
            addStudentDialog.Owner = Application.Current.MainWindow;
            addStudentDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addStudentDialog.ShowDialog();
        }

        private void CreateGroup()
        {
            // TODO: Open Create Group dialog
            MessageBox.Show(
                "Opening Create Group dialog...", 
                "Quick Action", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);

            var addNewGroup = new AddEditGroup();
            addNewGroup.Owner = Application.Current.MainWindow;
            addNewGroup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addNewGroup.ShowDialog();
        }

        private void ScheduleSession()
        {
            // TODO: Open Schedule Session dialog
            MessageBox.Show(
                "Opening Schedule Session dialog...", 
                "Quick Action", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);

            var addNewSession = new AddNewSessionWindow();
            addNewSession.Owner = Application.Current.MainWindow;
            addNewSession.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addNewSession.ShowDialog();
        }

        private void TakeAttendance()
        {
            // TODO: Open Attendance dialog
            MessageBox.Show(
                "Opening Attendance dialog...", 
                "Quick Action", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);

        }

        private void CreateAssessment()
        {
            // TODO: Open Create Assessment dialog
            MessageBox.Show(
                "Opening Create Assessment dialog...", 
                "Quick Action", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);

            var addNewAssessment = new AddEditAssessmentWindow();
            addNewAssessment.Owner = Application.Current.MainWindow;
            addNewAssessment.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addNewAssessment.ShowDialog();
        }

        private void ViewReports()
        {
            // TODO: Navigate to Reports page
            MessageBox.Show(
                "Opening Reports...", 
                "Quick Action", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    #region View Models for Dashboard Items

    public class ActivityItemVM
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string TimeAgo { get; set; }
        public PackIconMaterialKind Icon { get; set; }
        public Brush Color { get; set; }
        public Brush BgColor { get; set; }
    }

    public class EventItemVM
    {
        public string Title { get; set; }
        public string DateRange { get; set; }
        public string Subtitle { get; set; }
        public Brush MainColor { get; set; }
        public Brush BgColor { get; set; }
    }

    #endregion

    #region RelayCommand

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }

    #endregion
}
