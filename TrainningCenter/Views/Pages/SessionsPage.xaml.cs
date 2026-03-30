using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrainingCenter_BusinessLayer;
using TrainingCenter_BusinessLayer.Helpers;
using TrainningCenter.ViewModel;
using TrainningCenter.Views.Windows;
using TrainningCenter.Views.Windows.AttendanceSessions;
using TrainningCenter.Views.Windows.Sessions;
using TrainningCenter_Entities;

namespace TrainningCenter.Views.Pages
{
    /// <summary>
    /// Interaction logic for SessionsPage.xaml
    /// </summary>
    public partial class SessionsPage : Page, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Private Fields

        enum DateFilterMode
        {
            Today,
            ThisWeek,
            ThisMonth,
            All
        }

        private ObservableCollection<SessionVM> _sessions;
        private ICollectionView _sessionsView;
        private string _searchKeyword = string.Empty;
        private DateFilterMode _dateFilter = DateFilterMode.All;
        private Session.enStatus? _statusFilter = null;
        private string _todaySessions = "0";
        private string _upcomingSessions = "0";
        private string _completedSessions = "0";
        private string _attendanceRate = "0%";
        private string _activeSession = "0";
        private string _pendingSessions = "0";

        #endregion

        #region Public Properties

        public ObservableCollection<SessionVM> Sessions
        {
            get => _sessions;
            set
            {
                _sessions = value;
                OnPropertyChanged();
                UpdateStatistics(); // تحديث الإحصائيات عند تغيير القائمة
            }
        }

        public string TodaySessions
        {
            get => _todaySessions;
            set
            {
                _todaySessions = value;
                OnPropertyChanged();
            }
        }

        public string UpcomingSessions
        {
            get => _upcomingSessions;
            set
            {
                _upcomingSessions = value;
                OnPropertyChanged();
            }
        }

        public string CompletedSessions
        {
            get => _completedSessions;
            set
            {
                _completedSessions = value;
                OnPropertyChanged();
            }
        }

        public string AttendanceRate
        {
            get => _attendanceRate;
            set
            {
                _attendanceRate = value;
                OnPropertyChanged();
            }
        }

        public string ActiveSession
        {
            get => _activeSession;
            set
            {
                _activeSession = value;
                OnPropertyChanged();
            }
        }

        public string PendingSessions
        {
            get => _pendingSessions;
            set
            {
                _pendingSessions = value;
                OnPropertyChanged();
            }
        }

        #endregion
        public SessionsPage()
        {
            InitializeComponent();

            // Create the lists
            Sessions = new ObservableCollection<SessionVM>();

            // Data Binding
            DataContext = this;

            LoadSessions();
        }

        #region Load Methodes

        private async void LoadSessions()
        {
            try
            {
                var sessionsResult = await Session.GetAllSessions();

                if (sessionsResult == null || sessionsResult.Sessions == null)
                {
                    ShowEmptyState();
                    return;
                }

                // Clear the old data
                Sessions.Clear();

                foreach (var sessionDto in sessionsResult.Sessions)
                {
                    // Convert DTO to Model
                    var sessionModel = new Session(sessionDto);

                    // Convert Model to ViewModel
                    var sessionVM = new SessionVM(sessionModel);

                    Sessions.Add(sessionVM);
                }

                // Update Statistics
                UpdateStatistics();

                // In the case we have any data we hide the empty state
                if (Sessions.Any())
                {
                    emptyState.Visibility = Visibility.Collapsed;
                    sessionsListControl.Visibility = Visibility.Visible;
                    
                    _sessionsView = CollectionViewSource.GetDefaultView(Sessions);
                    _sessionsView.Filter = FilterSessions;
                    sessionsListControl.ItemsSource = _sessionsView;
                }
                else
                {
                    ShowEmptyState();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sessions: {ex.Message}",
                       "Error",
                       MessageBoxButton.OK,
                       MessageBoxImage.Error);

                ShowEmptyState();
            }
        }

        private void UpdateStatistics()
        {
            if (Sessions == null || !Sessions.Any())
            {
                TodaySessions = "0";
                UpcomingSessions = "0";
                CompletedSessions = "0";
                AttendanceRate = "0%";
                ActiveSession = "0";
                PendingSessions = "0";
                return;
            }

            var today = DateTime.Today;

            // Today's Sessions
            TodaySessions = Sessions
                .Count(s => s.IsToday && s.Status != "Cancelled" && s.IsActive)
                .ToString();

            // Upcoming Sessions (Next 7 days)
            UpcomingSessions = Sessions
                .Count(s => s.SessionDate.Date > today &&
                    s.SessionDate.Date <= today.AddDays(7) &&
                    s.Status != "Cancelled" &&
                    s.Status != "Completed" &&
                    s.IsActive)
                .ToString();

            // Completed this month
            CompletedSessions = Sessions
                .Count(s => s.Status == "Completed" &&
                    s.SessionDate.Month == today.Month &&
                    s.SessionDate.Year == today.Year)
                .ToString();

            // Attendance Rate
            var totalPresent = Sessions.Sum(s => s.PresentCount);
            var totalStudents = Sessions.Sum(s => s.TotalStudents);
            var rate = totalStudents > 0 ? (totalPresent * 100 / totalStudents) : 0;

            AttendanceRate = $"{rate}%";

            // Active Sessions
            ActiveSession = Sessions
                .Count(s => s.IsActive)
                .ToString();

            // Pending Sessions
            PendingSessions = Sessions
                .Count(s => s.Status == "Pending" &&
                    s.SessionDate.Year == today.Year)
                .ToString();
        }

        private bool FilterSessions(object obj)
        {
            if (obj is not SessionVM s) return false;

            // Search
            if (!string.IsNullOrEmpty(_searchKeyword))
            {
                if (!Match(s.Topic, _searchKeyword) &&
                    !Match(s.GroupName, _searchKeyword) &&
                    !Match(s.TrainerName, _searchKeyword))
                    return false;
            }

            // 2. Date filter
            if (!MatchDate(s))
                return false;

            // 3. Status filter
            if (_statusFilter.HasValue && s.StatusEnum != _statusFilter.Value)
                return false;

            return true;
        }
        private bool Match(string source, string keyword)
        {
            return !string.IsNullOrEmpty(source) &&
                   source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private bool MatchDate(SessionVM s)
        {
            var today = DateTime.Today;

            switch (_dateFilter)
            {
                case DateFilterMode.Today:
                    return s.SessionDate.Date == today;

                case DateFilterMode.ThisWeek:
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    var endOfWeek = startOfWeek.AddDays(7);
                    return s.SessionDate.Date >= startOfWeek && s.SessionDate.Date < endOfWeek;

                case DateFilterMode.ThisMonth:
                    return s.SessionDate.Month == today.Month && s.SessionDate.Year == today.Year;

                case DateFilterMode.All:
                default:
                    return true;
            }
        }


        #endregion

        #region Filter Methods

        /// <summary>
        /// Cycles through date filter options
        /// </summary>
        private void CycleDateFilter()
        {
            // Cycle through: All -> Today -> This Week -> This Month -> All
            _dateFilter = _dateFilter switch
            {
                DateFilterMode.All => DateFilterMode.Today,
                DateFilterMode.Today => DateFilterMode.ThisWeek,
                DateFilterMode.ThisWeek => DateFilterMode.ThisMonth,
                DateFilterMode.ThisMonth => DateFilterMode.All,
                _ => DateFilterMode.All
            };

            UpdateDateFilterUI();
            _sessionsView?.Refresh();
        }

        /// <summary>
        /// Updates the date filter button appearance
        /// </summary>
        private void UpdateDateFilterUI()
        {
            var (text, color) = _dateFilter switch
            {
                DateFilterMode.Today => ("Today", "#8147E7"),
                DateFilterMode.ThisWeek => ("This Week", "#8147E7"),
                DateFilterMode.ThisMonth => ("This Month", "#8147E7"),
                DateFilterMode.All => ("All Dates", "#E0E0E0"),
                _ => ("All Dates", "#E0E0E0")
            };

            tbDateFilter.Text = text;
            btnDateFilter.BorderBrush = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(color));
        }

        /// <summary>
        /// Cycles through status filter options
        /// </summary>
        private void CycleStatusFilter()
        {
            // Cycle: All -> Pending -> Active -> Completed -> Cancelled -> All
            if (!_statusFilter.HasValue)
            {
                _statusFilter = Session.enStatus.Scheduled;
            }
            else
            {
                _statusFilter = _statusFilter.Value switch
                {
                    Session.enStatus.Postponed => Session.enStatus.Completed,
                    Session.enStatus.Completed => Session.enStatus.Scheduled,
                    Session.enStatus.Scheduled => Session.enStatus.Cancelled,
                    Session.enStatus.Cancelled => null,
                    _ => null
                };

                UpdateStatusFilterUI();
                _sessionsView?.Refresh();
            }
        }

        /// <summary>
        /// Updates the status filter button appearance
        /// </summary>
        private void UpdateStatusFilterUI()
        {
            var (text, color) = _statusFilter switch
            {
                Session.enStatus.Scheduled => ("Scheduled", "#FFA500"),
                Session.enStatus.Postponed => ("Postponed", "#8147E7"),
                Session.enStatus.Completed => ("Completed", "#28A745"),
                Session.enStatus.Cancelled => ("Cancelled", "#DC3545"),
                null => ("All Status", "#E0E0E0"),
                _ => ("All Status", "#E0E0E0")
            };

            tbStatusFilter.Text = text;
            btnStatusFilter.BorderBrush = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(color));
        }

        /// <summary>
        /// Resets all filters to default
        /// </summary>
        private void ResetFilters()
        {
            _searchKeyword = string.Empty;
            _dateFilter = DateFilterMode.All;
            _statusFilter = null;

            txtSearch.Text = string.Empty;
            UpdateDateFilterUI();
            UpdateStatusFilterUI();

            _sessionsView?.Refresh();
        }
        #endregion

        #region Event Handlers
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSessions();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchKeyword = txtSearch.Text;

            // Show/hide clear button
            btnClearSearch.Visibility = string.IsNullOrEmpty(_searchKeyword)
                ? Visibility.Collapsed
                : Visibility.Visible;

            _sessionsView?.Refresh();
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            _searchKeyword = string.Empty;
            btnClearSearch.Visibility = Visibility.Collapsed;
            _sessionsView?.Refresh();
        }

        private void SessionCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            int sessionID = (int)border.Tag;

            // Searching for the session
            var sessionVM = Sessions.FirstOrDefault(s => s.SessionID == sessionID);

            if (sessionVM is not null)
            {
                // فتح صفحة تفاصيل الجلسة
                // var detailsPage = new SessionDetailsPage(sessionVM);
                // NavigationService.Navigate(detailsPage);

                MessageBox.Show($"Opening session: {sessionVM.Topic}",
                               "Session Details",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
        }

        private async void btnTakeAttendance_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var sessionId = (int)button.Tag;

            // البحث عن الجلسة
            var sessionVM = Sessions.FirstOrDefault(s => s.SessionID == sessionId);

            if (sessionVM == null)
            {
                MessageBox.Show("Session not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!sessionVM.CanTakeAttendance)
            {
                MessageBox.Show("Cannot take attendance for this session. It might be cancelled or completed.",
                               "Not Allowed",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            // فتح نافذة تسجيل الحضور
            var attendanceWindow = new AttendanceDialogWindow(sessionVM);
            attendanceWindow.ShowDialog();

            // بعد إغلاق النافذة، تحديث البيانات
            await RefreshSessionData(sessionId);

            MessageBox.Show($"Attendance recorded for session: {sessionVM.Topic}",
                           "Success",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }

        private async Task RefreshSessionData(int sessionID)
        {
            try
            {
                // Fetch the data of the updated session
                var session = await Session.Find(sessionID);
                if (session != null)
                {
                    var updatedVM = new SessionVM(session);

                    // Update the session in the list
                    var index = Sessions.IndexOf(Sessions.First(s => s.SessionID == sessionID));
                    if (index >= 0)
                    {
                        Sessions[index] = updatedVM;
                        UpdateStatistics();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error refreshing data: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                EventLogger.LogWarning($"Error Refreshing Data: {ex.Message}");
            }
        }
        private void btnViewCalendar_Click(object sender, RoutedEventArgs e)
        {
            // فتح عرض التقويم
            MessageBox.Show("Calendar view feature coming soon!",
                           "Info",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }

        private void btnExportData_Click(object sender, RoutedEventArgs e)
        {
            if (Sessions == null || !Sessions.Any())
            {
                MessageBox.Show("There is no session data to export.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 1. Get the filtered data currently shown in the UI
            var visibleSessions = _sessionsView.Cast<SessionVM>().ToList();

            // 2. Open your Configuration Window
            var configWindow = new ExportConfigWindow(visibleSessions, visibleSessions.Count);
            configWindow.Owner = Application.Current.MainWindow;

            if (configWindow.ShowDialog() == true)
            {
                try
                {
                    var fileService = new FileService();
                    string filePath = configWindow.ExportFilePath;

                    if (configWindow.ExportFormat == "Excel")
                    {
                        // Export everything the user sees to Excel
                        fileService.ExportToExcel(visibleSessions, filePath);
                    }
                    else if (configWindow.ExportFormat == "PDF")
                    {
                        // 3. Define professional columns for a Session Report
                        // Use the property names found in your SessionVM
                        var columns = new List<string>
                        {
                            "SessionDate",
                            "Topic",
                            "GroupName",
                            "TrainerName",
                            "Status",
                            "AttendanceRate" // This shows how successful the session was!
                        };

                        fileService.ExportToPdf(visibleSessions, configWindow.DocumentTitle, filePath, columns);
                    }

                    MessageBox.Show("Sessions report exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnNewSession_Click(object sender, RoutedEventArgs e)
        {
            // فتح نافذة إضافة جلسة جديدة
            // var newSessionWindow = new NewSessionWindow();
            // newSessionWindow.ShowDialog();

            // إذا تمت الإضافة، إعادة تحميل البيانات
            // if (newSessionWindow.DialogResult == true)
            // {
            //     LoadSessions();
            // }

            var newSessionWindow = new AddNewSessionWindow();
            newSessionWindow.ShowDialog();

            if (newSessionWindow.DialogResult == true)
            {
                LoadSessions();
            }
        }


        private void btnDateFilter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CycleDateFilter();
        }

        private void btnStatusFilter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CycleStatusFilter();
        }
        private void ShowEmptyState()
        {
            emptyState.Visibility = Visibility.Visible;
            sessionsListControl.Visibility = Visibility.Collapsed;
        }
        #endregion

    }
}
