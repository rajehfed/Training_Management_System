using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter.ViewModel;

namespace TrainningCenter.Views.Windows.Sessions
{
    /// <summary>
    /// Interaction logic for AddNewSessionWindow.xaml
    /// </summary>
    public partial class AddNewSessionWindow : Window
    {
        private SessionVM _session;
        private ObservableCollection<GroupVM> _groups;
        private ObservableCollection<TrainerVM> _trainers;

        // أو إذا كان هناك GroupID معين
        public AddNewSessionWindow(int groupID = 0)
        {
            InitializeComponent();

            _session = new SessionVM();
            _groups = new ObservableCollection<GroupVM>();
            _trainers = new ObservableCollection<TrainerVM>();

            this.DataContext = _session;

            // تعيين مصادر البيانات
            cmbGroup.ItemsSource = _groups;
            cmbTrainer.ItemsSource = _trainers;
            cmbGroup.ItemsSource = _groups;

            // تعيين قيم ComboBox
            InitializeSessionTypes();
            InitializeStatusOptions();
            InitializeTimeSlots();

            if (groupID > 0)
            {
                _session.GroupID = groupID;
            }

            Loaded += async (s, e) => await LoadDataAsync();

            SetupRealTimeValidation();
        }

        #region EVENT HANDLERS

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(!ValidateForm())
            {
                MessageBox.Show(
                    "Please correct the highlighted errors.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                SetUIEnabled(false);

                if (cmbSessionType.SelectedItem != null)
                {
                    _session.SessionType = (Session.enSessionType)cmbSessionType.SelectedItem;
                }
                if (cmbStatus.SelectedItem != null)
                {
                    _session.StatusEnum = (Session.enStatus)cmbStatus.SelectedItem;
                }
                bool sessucess = (await _session.SaveAsync(Globals.CurrentUser.UserId)).success;

                if (sessucess)
                {
                    string msg = $"Session '{_session.Topic}' created successfully!";
                    MessageBox.Show(
                        msg,
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        "Failed to save That Session.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (SqlException sqlEx)
            {
                // معالجة أخطاء SQL بشكل أفضل
                string userMessage = ParseSqlErrorMessage(sqlEx.Message);
                MessageBox.Show(
                    userMessage,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                EventLogger.LogError("SQL Error saving Session", sqlEx);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                EventLogger.LogError("Error saving Session", ex);
            }
            finally
            {
                SetUIEnabled(true);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        private void cmbGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cmbGroup.SelectedItem is GroupVM selectedGroup)
            {
                _session.GroupID = selectedGroup.GroupID;
                UpdateInfoCards();
            }
        }

        private void cmbStartTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cmbStartTime.SelectedItem is TimeSpan selectedStartTime)
            {
                _session.StartTime = selectedStartTime;
                UpdateInfoCards();
            }
        }

        private void cmbEndTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cmbEndTime.SelectedItem is TimeSpan selectedEndTime)
            {
                _session.EndTime = selectedEndTime;
                UpdateInfoCards();
            }
        }

        private void dpSessionDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateInfoCards();
        }
        #endregion

        #region DATA LOADING 

        private async Task LoadDataAsync()
        {
            try
            {
                var groups = await GroupVM.LoadAllGroups();
                _groups.Clear();
                foreach (var group in groups)
                {
                    _groups.Add(group);
                }

                // Load trainers
                var trainersList = await Trainer.GetAllTrianers();
                _trainers.Clear();
                foreach (var trainer in trainersList)
                {
                    var trainerVM = new TrainerVM
                    {
                        TrainerID = trainer.TrainerID ?? 0,
                        FullName = trainer.CurrentPerson != null ? trainer.CurrentPerson.FullName : "Unknown"
                    };
                    _trainers.Add(trainerVM);
                }

                UpdateInfoCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading data: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                EventLogger.LogError("Error loading data in AddNewSessionWindow", ex);
            }
        }

        #endregion

        #region HELPER METHODS

        private bool ValidateForm()
        {
            // 1. Use ViewModel validation
            if (!_session.IsValid(out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 2. UI-specific validation
            bool isValid = true;
            ResetValidationErrors();

            // Required fields
            if (_session.GroupID <= 0)
            {
                HighlightControl(cmbGroup);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(_session.Topic))
            {
                HighlightControl(txtTopic);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(_session.Location))
            {
                HighlightControl(txtLocation);
                isValid = false;
            }

            // Date validation
            if (_session.SessionDate < DateTime.Today)
            {
                HighlightControl(dpSessionDate);
                isValid = false;
            }

            // Time validation
            if (_session.EndTime <= _session.StartTime)
            {
                HighlightControl(cmbEndTime);
                isValid = false;
            }

            if (!isValid)
            {
                MessageBox.Show(
                    "Please correct the highlighted fields.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            return isValid;
        }
        private void ResetValidationErrors()
        {
            var defaultBrush = (Brush)FindResource("BorderBrush");

            cmbGroup.BorderBrush = defaultBrush;
            txtTopic.BorderBrush = defaultBrush;
            txtLocation.BorderBrush = defaultBrush;
            dpSessionDate.BorderBrush = defaultBrush;
            cmbEndTime.BorderBrush = defaultBrush;
        }
        private void HighlightControl(Control control)
        {
            control.BorderBrush = Brushes.Red;
            control.BorderThickness = new Thickness(2);
        }

        private void SetupRealTimeValidation()
        {
            // Update the validation in any change
            txtTopic.TextChanged += (s, e) => ValidateField(txtTopic, !string.IsNullOrWhiteSpace(txtTopic.Text));
            txtLocation.TextChanged += (s, e) => ValidateField(txtLocation, !string.IsNullOrWhiteSpace(txtLocation.Text));
            dpSessionDate.SelectedDateChanged += (s, e) => ValidateField(dpSessionDate, dpSessionDate.SelectedDate >= DateTime.Today);
            cmbGroup.SelectionChanged += (s, e) => ValidateField(cmbGroup, cmbGroup.SelectedItem != null);
            cmbStartTime.SelectionChanged += (s, e) => ValidateTimes();
            cmbEndTime.SelectionChanged += (s, e) => ValidateTimes();
        }
        private void ValidateField(Control control, bool isValid)
        {
            if (isValid)
            {
                control.BorderBrush = (Brush)FindResource("BorderBrush");
                control.BorderThickness = new Thickness(1);
            }
            else
            {
                control.BorderBrush = Brushes.Red;
                control.BorderThickness = new Thickness(2);
            }
        }
        private void ValidateTimes()
        {
            if (cmbStartTime.SelectedItem is TimeSpan startTime &&
                cmbEndTime.SelectedItem is TimeSpan endTime)
            {
                bool isValid = endTime > startTime;

                if (isValid)
                {
                    cmbStartTime.BorderBrush = (Brush)FindResource("BorderBrush");
                    cmbEndTime.BorderBrush = (Brush)FindResource("BorderBrush");
                    validationPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    cmbStartTime.BorderBrush = Brushes.Red;
                    cmbEndTime.BorderBrush = Brushes.Red;
                    validationPanel.Visibility = Visibility.Visible;
                    tbValidationMessage.Text = "End time must be after start time";
                }
            }
        }

        private void InitializeSessionTypes()
        {
            cmbSessionType.ItemsSource = Enum.GetValues(typeof(Session.enSessionType)).Cast<Session.enSessionType>();
            cmbSessionType.SelectedIndex = 0;
        }
        private void InitializeStatusOptions()
        {
            cmbStatus.ItemsSource = Enum.GetValues(typeof(Session.enStatus)).Cast<Session.enStatus>();
            cmbStatus.SelectedIndex = 0;
        }
        private void InitializeTimeSlots()
        {
            var timeSlots = new List<TimeSpan>();
            for (int hour = 8; hour <= 20; hour++) { 
                for(int minute = 0; minute < 60; minute += 30)
                {
                    timeSlots.Add(new TimeSpan(hour, minute, 0));
                }
            }

            cmbStartTime.ItemsSource = timeSlots;
            cmbEndTime.ItemsSource = timeSlots;

            //// تنسيق العرض
            //cmbStartTime.DisplayMemberPath = "ToString";
            //cmbEndTime.DisplayMemberPath = "ToString";
        }

        private void UpdateInfoCards()
        {
            // Update group info card
            if(cmbGroup.SelectedItem is GroupVM selectedGroup)
            {
                tbSelectedGroup.Text = $"{selectedGroup.GroupName}\n{selectedGroup.TrainersList} students";
            }
            
            tbSelectedDate.Text = _session.SessionDate.ToString("dd MMM yyyy");

            if(_session.StartTime != TimeSpan.Zero && _session.EndTime != TimeSpan.Zero)
            {
                tbSelectedTime.Text = $"{_session.StartTime:hh\\:mm} - {_session.EndTime:hh\\:mm}";
                var duration = _session.EndTime - _session.StartTime;
                tbDuration.Text = $"Duration: {duration.TotalHours:F1} hrs";
            }
        }

        private void SetUIEnabled(bool enabled)
        {
            btnSave.IsEnabled = enabled;
            btnCancel.IsEnabled = enabled;
            cmbGroup.IsEnabled = enabled;
            txtTopic.IsEnabled = enabled;
            cmbEndTime.IsEnabled = enabled;
            cmbStartTime.IsEnabled = enabled;
            dpSessionDate.IsEnabled = enabled;
            txtLocation.IsEnabled = enabled;
            txtNotes.IsEnabled = enabled;

            Cursor = enabled ? Cursors.Arrow : Cursors.Wait;
        }
        private string ParseSqlErrorMessage(string sqlError)
        {
            if (sqlError.Contains("already has an active session"))
                return "This group already has a session scheduled at this time.";
            if (sqlError.Contains("Trainer is already scheduled"))
                return "The selected trainer is already booked at this time.";
            if (sqlError.Contains("Group is full"))
                return "The group has reached maximum capacity.";

            return "A database error occurred. Please try again.";
        }

        #endregion

    }
}
