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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter_Entities;
using Microsoft.Win32;
using TrainningCenter.Views.Windows;

namespace TrainningCenter.Views.Pages
{
    /// <summary>
    /// Interaction logic for GradeBookPage.xaml
    /// </summary>
    public partial class GradeBookPage : Page, INotifyPropertyChanged
    {
        #region Fields and Properties
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private ObservableCollection<GradebookItemDTO> _results;
        public ObservableCollection<GradebookItemDTO> Results
        {
            get => _results;
            set { _results = value; OnPropertyChanged(); }
        }

        private ICollectionView _resultsView;
        public ICollectionView ResultsView
        {
            get => _resultsView;
            set { _resultsView = value; OnPropertyChanged(); }
        }

        // Filters
        private int? _selectedAssessmentId;
        public int? SelectedAssessmentId
        {
            get => _selectedAssessmentId;
            set
            {
                if (_selectedAssessmentId == value)
                    return;

                _selectedAssessmentId = value;
                OnPropertyChanged();

                _ = LoadGroupsForAssessment();
                SelectedGroupId = null;
                Results?.Clear();
            }
        }

        private int? _selectedGroupId;
        public int? SelectedGroupId
        {
            get => _selectedGroupId;
            set
            {
                _selectedGroupId = value; OnPropertyChanged();
                _ = LoadResults();
            }
        }

        private enAssessmentStatus? _selectedStatus; // Changed to string to match DTO
        public enAssessmentStatus? SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); ApplyFilters(); }
        }

        // Statistics
        private decimal _averageMarks;
        public decimal AverageMarks { get => _averageMarks; set { _averageMarks = value; OnPropertyChanged(); } }
        private decimal _passRate;
        public decimal PassRate { get => _passRate; set { _passRate = value; OnPropertyChanged(); } }
        private int _totalStudents;
        public int TotalStudents { get => _totalStudents; set { _totalStudents = value; OnPropertyChanged(); } }
        private int _passedCount;
        public int PassedCount { get => _passedCount; set { _passedCount = value; OnPropertyChanged(); } }
        private int _failedCount;
        public int FailedCount { get => _failedCount; set { _failedCount = value; OnPropertyChanged(); } }

        public ObservableCollection<Assessment> AvailableAssessments { get; set; } = new ObservableCollection<Assessment>();
        public ObservableCollection<Group> AvailableGroups { get; set; } = new ObservableCollection<Group>();
        public IEnumerable<enAssessmentStatus> StatusOptions =>
            Enum.GetValues(typeof(enAssessmentStatus)).Cast<enAssessmentStatus>();

        #endregion

        #region Contractor
        public GradeBookPage()
        {
            InitializeComponent();
            this.DataContext = this;
            Loaded += GradeBookPage_Loaded;
        }
        private async void GradeBookPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMetadata();
        }
        private async Task LoadMetadata()
        {
            var assessments = await Assessment.GetAllAsync();

            foreach (var a in assessments.assessments)
                AvailableAssessments.Add(a);
        }
        #endregion

        #region Event Handlers
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (Results == null || !Results.Any())
            {
                MessageBox.Show("No data available to export.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 1. Get the filtered data from your View
            var visibleData = ResultsView.Cast<GradebookItemDTO>().ToList();

            // 2. Open your custom Configuration Window
            // We pass the data and the count so your window can show "Ready to export X records"
            var configWindow = new ExportConfigWindow(visibleData, visibleData.Count);
            configWindow.Owner = Application.Current.MainWindow;

            if (configWindow.ShowDialog() == true)
            {
                try
                {
                    var fileService = new FileService();
                    string filePath = configWindow.ExportFilePath;

                    if (configWindow.ExportFormat == "Excel")
                    {
                        // Use the settings from your window
                        var excelConfig = new ExportConfig<GradebookItemDTO>
                        {
                            SheetName = configWindow.SheetName
                        };

                        fileService.ExportToExcel(visibleData, filePath, excelConfig);
                    }
                    else if (configWindow.ExportFormat == "PDF")
                    {
                        // Define the columns you want for the Gradebook
                        var columns = new List<string> { "StudentName", "MarksObtained", "Status", "Feedback" };

                        // Use the Document Title entered in your window
                        fileService.ExportToPdf(visibleData, configWindow.DocumentTitle, filePath, columns);
                    }

                    MessageBox.Show("Export completed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (Results == null || !Results.Any(x => x.IsDirty))
            {
                MessageBox.Show("No changes to save.");
                return;
            }

            btnSaveChanges.IsEnabled = false; // Prevent double clicks

            try
            {
                var bll = new Gradebook();
                var result = await bll.SaveGradebookAsync(Results, Globals.CurrentUser.UserId);

                if (result.IsSuccess)
                {
                    MessageBox.Show(
                        $"Successfully saved {result.SavedCount} records.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    UpdateStatistics(); // Refresh counts
                }
                else
                {
                    string errorSummary = string.Join("\n", result.Errors.Take(5));
                    if (result.Errors.Count > 5) errorSummary += "\n...and more.";

                    MessageBox.Show(
                        $"Saved {result.SavedCount} items, but encountered errors:\n\n{errorSummary}",
                        "Partial Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                EventLogger.LogError("Error saving gradebook changes.", ex);
            }
            finally
            {
                btnSaveChanges.IsEnabled = true;
            }
        }

        private void btnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            _selectedAssessmentId = null;
            _selectedGroupId = null;
            _selectedStatus = null;
            AvailableGroups.Clear();

            // Notify the UI
            OnPropertyChanged(nameof(SelectedAssessmentId));
            OnPropertyChanged(nameof(SelectedGroupId));
            OnPropertyChanged(nameof(SelectedStatus));

            Results?.Clear();
            UpdateStatistics();
        }
        #endregion

        #region Methods
        private async Task LoadResults()
        {
            if (SelectedAssessmentId.HasValue && SelectedGroupId.HasValue)
            {
                try
                {
                    var data = await Gradebook.GetGradebookAsync(SelectedAssessmentId.Value, SelectedGroupId.Value);

                    Results = new ObservableCollection<GradebookItemDTO>(data);
                    ResultsView = CollectionViewSource.GetDefaultView(Results);
                    ResultsView.Filter = FilterResult;

                    UpdateStatistics();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading data: " + ex.Message);
                }
            }
        }
        private bool FilterResult(object obj)
        {
            if (obj is not GradebookItemDTO result) return false;

            // If SelectedStatus is null, it means "Show All"
            if (SelectedStatus.HasValue && result.Status != SelectedStatus.Value)
                return false;

            return true;
        }
        private async Task LoadGroupsForAssessment()
        {
            AvailableGroups.Clear();

            if (!SelectedAssessmentId.HasValue)
            {
                Results?.Clear(); // Clear the grid if no assessment is selected
                UpdateStatistics();
                return;
            }
            try
            {
                // Find the selected assessment object to get the SpecializationID
                var assessment = AvailableAssessments.FirstOrDefault(a => a.AssessmentID == SelectedAssessmentId);

                if (assessment?.SpecializationID != null)
                {
                    var groups = await Group.GetGroupsBySpecializationDTO(assessment.SpecializationID.Value);

                    // Populate the dropdown
                    foreach (var g in groups)
                        AvailableGroups.Add(g);
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Error loading groups for specialization", ex);
            }
        }

        private void ApplyFilters()
        {
            ResultsView?.Refresh();
            UpdateStatistics();
        }
        private void UpdateStatistics()
        {
            if (Results == null || ResultsView == null) return;

            // Important: Use Cast to count ONLY what is visible after filtering
            var visibleItems = ResultsView.Cast<GradebookItemDTO>().ToList();

            TotalStudents = visibleItems.Count;
            PassedCount = visibleItems.Count(x => x.Status == enAssessmentStatus.Passed);
            FailedCount = visibleItems.Count(x => x.Status == enAssessmentStatus.Failed);

            if (visibleItems.Any(x => x.MarksObtained.HasValue))
            {
                AverageMarks = visibleItems.Where(x => x.MarksObtained.HasValue)
                                           .Average(x => x.MarksObtained.Value);

                PassRate = TotalStudents > 0 ? (decimal)PassedCount / TotalStudents * 100 : 0;
            }
            else
            {
                AverageMarks = 0;
                PassRate = 0;
            }
        }
        #endregion
    }
}
