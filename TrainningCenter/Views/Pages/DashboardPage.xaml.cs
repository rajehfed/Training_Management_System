using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
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
using System.Windows.Threading;
using TrainningCenter.ViewModel;

namespace TrainningCenter.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        #region Fields

        private DashboardViewModel _viewModel;
        private DispatcherTimer _timeTimer;
        private DispatcherTimer _autoRefreshTimer;

        #endregion

        #region Constructor
        public DashboardPage()
        {
            InitializeComponent();
            InitializeDashboard();
        }
        #endregion

        #region Initialization

        private void InitializeDashboard()
        {
            // Initialize ViewModel
            _viewModel = new DashboardViewModel();
            this.DataContext = _viewModel;

            // Initialize time display
            UpdateDateTime();

            // Start time update timer (every 30 seconds)
            StartTimeUpdates();

            // Start auto-refresh timer (every 5 minutes)
            StartAutoRefresh();
        }

        private void StartAutoRefresh()
        {
            // Auto-refresh dashboard data every 5 minutes
            _autoRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5)
            };
            _autoRefreshTimer.Tick += async (s, e) =>
            {
                await _viewModel.LoadDashboardDataAsync();
            };
            _autoRefreshTimer.Start();
        }

        private void StartTimeUpdates()
        {
            _timeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };

            _timeTimer.Tick += (s, e) => UpdateDateTime();
            _timeTimer.Start();
        }

        private void UpdateDateTime()
        {
            var now = DateTime.Now;
            tbTime.Text = now.ToString("h:mm tt");
            tbDate.Text = now.ToString("dddd, MMM dd, yyyy");
        }

        #endregion

        #region Events Handlers

        private void StatCard_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag == null) return;

            var cardType = border.Tag.ToString();

            switch (cardType)
            {
                case "Students":
                    _viewModel.NavigateToStudentsCommand.Execute("Students");
                    break;

                case "Groups":
                    _viewModel.NavigateToGroupsCommand.Execute("Groups");
                    break;

                case "Revenue":
                    _viewModel.NavigateToPaymentsCommand.Execute("Payments");
                    break;

                case "Attendance":
                    _viewModel.NavigateToSessionsCommand.Execute("Sessions");
                    break;
            }
        }

        private void QuickAction_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag == null) return;

            var action = border.Tag.ToString();

            switch (action)
            {
                case "AddStudent":
                    _viewModel.AddStudentCommand.Execute(null);
                    break;

                case "CreateGroup":
                    _viewModel.CreateGroupCommand.Execute(null);
                    break;

                case "ScheduleSession":
                    _viewModel.ScheduleSessionCommand.Execute(null);
                    break;

                case "TakeAttendance":
                    _viewModel.TakeAttendanceCommand.Execute(null);
                    break;

                case "CreateAssessment":
                    _viewModel.CreateAssessmentCommand.Execute(null);
                    break;

                case "ViewReports":
                    _viewModel.ViewReportsCommand.Execute(null);
                    break;
            }
        }

        #endregion

        #region Cleanup

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Stop timers when page unloads
            _timeTimer?.Stop();
            _autoRefreshTimer?.Stop();
        }

        #endregion
    }
}
