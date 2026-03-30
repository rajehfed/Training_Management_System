using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter.ViewModel;
using TrainningCenter_DataAccessLayer;
using TrainningCenter_Entities;
using static TrainningCenter.ViewModel.AttendanceRecordVM;

namespace TrainningCenter.Views.Windows.AttendanceSessions
{
    /// <summary>
    /// Interaction logic for AttendanceDialogWindow.xaml
    /// </summary>
    public partial class AttendanceDialogWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
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

        #region Fields

        private SessionVM _session;
        private ObservableCollection<AttendanceRecordVM> _attendanceRecords;

        private int _presentCount = 0;
        private int _lateCount = 0;
        private int _absentCount = 0;
        private int _totalStudents = 0;
        private int _attendanceRate = 0;

        #endregion

        #region Properties for Binding

        public int PresentCount
        {
            get => _presentCount;
            set => SetProperty(ref _presentCount, value);
        }

        public int LateCount
        {
            get => _lateCount;
            set => SetProperty(ref _lateCount, value);
        }

        public int AbsentCount
        {
            get => _absentCount;
            set => SetProperty(ref _absentCount, value);
        }

        public int AttendanceRate
        {
            get => _attendanceRate;
            set => SetProperty(ref _attendanceRate, value);
        }

        #endregion

        #region Constractor
        public AttendanceDialogWindow(SessionVM vm)
        {
            InitializeComponent();

            _session = vm;
            _attendanceRecords = new ObservableCollection<AttendanceRecordVM>();

            // Set DataContext for summary panel bindings
            this.DataContext = this;

            // Set ItemsSource for students list
            studentsListControl.ItemsSource = _attendanceRecords;

            // Subscribe to collection changed event 
            _attendanceRecords.CollectionChanged += (s, e) => UpdateSummary();

            InitializeWindow();
            _ = LoadStudentsAsync();
        }

        #endregion

        #region Data Loading

        private async Task LoadStudentsAsync()
        {
            try
            {
                _attendanceRecords.Clear();

                if (_session.SessionID.HasValue)
                {
                    var attendanceResult = await Attendance.GetSessionAttendance(_session.SessionID.Value);

                    if (attendanceResult?.Attendances?.Any() == true)
                    {
                        // Attendance Exists
                        foreach (var student in attendanceResult.Attendances)
                            _attendanceRecords.Add(new AttendanceRecordVM(student));
                    }
                    else
                    {
                        // No Existing Attendance - Load from Group
                        var students = await Member.GetMembersList(_session.GroupID, OnlySubscribing: true);
                        foreach (var student in students)

                            _attendanceRecords.Add(
                                AttendanceRecordVM.FromMember(new MemberVM(student), _session.SessionID.Value)
                            );
                    }

                    _totalStudents = _attendanceRecords.Count;
                    UpdateSummary();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading students: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region Initialization

        private void InitializeWindow()
        {
            // Set session info in header
            tbSessionInfo.Text = $"{_session.Topic} - {_session.GroupName} | {_session.FormattedDate}";

            // Initialize summary
            UpdateSummary();
        }

        #endregion

        #region Events Handlers
        private void btnMarkAllPresent_Click(object sender, RoutedEventArgs e)
        {
            foreach (var record in _attendanceRecords)
            {
                record.AttendanceStatus = enAttendanceStatus.Present;
            }

            UpdateSummary();
        }

        private void btnMarkAllAbsent_Click(object sender, RoutedEventArgs e)
        {
            foreach (var record in _attendanceRecords)
            {
                record.AttendanceStatus = enAttendanceStatus.Absent;
                record.ArrivalTime = null;
            }

            UpdateSummary();
        }

        private async void btnSaveAttendance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var errors = new List<string>();

                foreach (var record in _attendanceRecords)
                {
                    if (!record.IsValid(out string errorMessage))
                    {
                        errors.Add($"Student {record.FullName} ({record.StudentNumber}): {errorMessage}");
                    }
                }

                if (errors.Any())
                {
                    MessageBox.Show(
                        $"Please fix the following errors:\n{string.Join("\n", errors)}",
                        "Validation Errors",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                btnSaveAttendance.IsEnabled = false;

                var results = new List<AttendanceUpsertResultDTO>();
                var failedRecords = new List<string>();

                foreach (var record in _attendanceRecords)
                {
                    try
                    {
                        if (_session.SessionID.HasValue)
                        {
                            var dto = record.ToCreateUpdateDTO();
                            dto.Notes = txtNotes.Text;
                            dto.RecordedBy = Globals.CurrentUser?.UserId ?? 1;

                            var result = await Attendance.RecordAttendance(
                                dto.StudentID,
                                _session.SessionID.Value,
                                (TrainingCenter_BusinessLayer.Attendance.enAttendanceStatus)record.AttendanceStatus,
                                dto.ArrivalTime,
                                dto.RecordedBy.Value);

                            results.Add(result);

                            if (!result.Success)
                            {
                                failedRecords.Add($"{record.FullName}: {result.ErrorMessage}");
                            }
                            else
                            {
                                // Update record ID if it was new
                                if (record.AttendanceID <= 0 && result.AttendanceID.HasValue)
                                {
                                    record.AttendanceID = result.AttendanceID.Value;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        failedRecords.Add($"{record.FullName}: {ex.Message}");
                    }
                }

                // Show summary
                if (failedRecords.Any())
                {
                    MessageBox.Show(
                        $"Some records failed to save:\n{string.Join("\n", failedRecords.Take(5))}",
                        "Partial Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show(
                        $"Successfully saved {results.Count} attendance records!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // تحديث الجلسة في الصفحة الأم
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving attendance: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btnSaveAttendance.IsEnabled = true;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_presentCount > 0 || _lateCount > 0)
            {
                var result = MessageBox.Show(
                    "You have unsaved data. Close anyway?",
                    "Confirm",
                    MessageBoxButton.YesNo);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        #endregion

        #region Helpers

        private enAttendanceStatus ParseAttendanceStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return enAttendanceStatus.Absent;

            return status.ToLower() switch
            {
                "present" => enAttendanceStatus.Present,
                "absent" => enAttendanceStatus.Absent,
                "late" => enAttendanceStatus.Late,
                "excused" => enAttendanceStatus.Excused,
                "canceled" => enAttendanceStatus.Canceled,
                _ => enAttendanceStatus.Absent
            };
        }

        private void UpdateSummary()
        {
            // Count By Status
            PresentCount = _attendanceRecords.Count(r => r.AttendanceStatus == AttendanceRecordVM.enAttendanceStatus.Present);
            LateCount = _attendanceRecords.Count(r => r.AttendanceStatus == AttendanceRecordVM.enAttendanceStatus.Late);
            AbsentCount = _attendanceRecords.Count(r => r.AttendanceStatus == AttendanceRecordVM.enAttendanceStatus.Absent);

            // Calculate attendance rate
            int attended = PresentCount + LateCount;
            if (_totalStudents > 0)
            {
                AttendanceRate = (int)((double)attended / _totalStudents * 100);
            }
            else
            {
                AttendanceRate = 0;
            }

            // Update total students text
            tbTotalStudents.Text = $"{attended} / {_totalStudents} Students";
        }

        #endregion
    }
}
