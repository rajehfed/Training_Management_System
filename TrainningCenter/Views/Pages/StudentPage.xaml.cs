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
using TrainingCenter_BusinessLayer.Helpers;
using TrainningCenter.ViewModel;
using TrainningCenter.Views.Windows;
using TrainningCenter.Views.Windows.Students;
using TrainningCenter_Entities;

namespace TrainningCenter.Views.Pages
{
    /// <summary>
    /// Interaction logic for StudentPage.xaml
    /// </summary>
    public partial class StudentPage : Page, INotifyPropertyChanged
    {
        #region Fields & ObservableCollections

        private ObservableCollection<StudentVM> _allStudents;
        private ObservableCollection<StudentVM> _filteredStudents;

        // Filters state
        private string _searchQuery = string.Empty;
        private int _selectedStatusFilterIndex = 0;
        private int _selectedGroupFilterIndex = 0;
        private int _selectedSortFilter = 0;

        // Pagination state
        private int _currentPage = 1;
        private const int ITEMS_PER_PAGE = 20;

        #endregion

        #region Statistics Properties (Bound to UI)
        private int _totalStudents;
        public int totalStudents
        {
            get => _totalStudents;
            set { _totalStudents = value; OnPropertyChanged(); }
        }

        private int _active;
        public int active
        {
            get => _active;
            set { _active = value; OnPropertyChanged(); }
        }

        private int _inactive;
        public int inactive
        {
            get => _inactive;
            set { _inactive = value; OnPropertyChanged(); }
        }

        private int _newEnrolements;
        public int NewEnrolements
        {
            get => _newEnrolements;
            set { _newEnrolements = value; OnPropertyChanged(); }
        }

        #endregion
        public StudentPage()
        {
            InitializeComponent();

            this.DataContext = this;

            _allStudents = new ObservableCollection<StudentVM>();
            _filteredStudents = new ObservableCollection<StudentVM>();

            // Set default selections
            cmbGroupFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndex = 0;
            cmbSortBy.SelectedIndex = 0;

            // Load data
            LoadAllStudentsAsync();
            LoadGroupFiltersAsync();
        }

        // ======================================
        // 1- Items Events!!!
        // ======================================
        #region Events Handelers
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAllStudentsAsync();
        }
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get all Students
                var students = _allStudents;

                if (!students.Any())
                {
                    MessageBox.Show(
                        "No students to export.",
                        "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                // 2. Show export config window
                var exportDialog = new ExportConfigWindow(students, students.Count())
                {
                    Owner = Application.Current.MainWindow
                };

                if (exportDialog.ShowDialog() == true)
                {
                    // 3. User clicked Export
                    var fileService = new FileService();

                    if (exportDialog.ExportFormat == "Excel")
                    {
                        // Export to Excel
                        var config = new ExportConfig<StudentVM>
                        {
                            SheetName = exportDialog.SheetName,
                            ExcludeProperties = new List<string>
                            {
                                "ImagePath",      // Don't export image paths
                                "IsActive",       // Internal field
                                "CreatedBy"       // Internal field
                            }
                        };

                        fileService.ExportToExcel(students, exportDialog.ExportFilePath, config);
                    }
                    else if (exportDialog.ExportFormat == "PDF")
                    {
                        // Export to PDF
                        var columnsToPrint = new List<string> { "StudentID", "FullName", "StudentNumber", "Email", "Phone", "StudentStatus" };
                        fileService.ExportToPdf(students, exportDialog.DocumentTitle, exportDialog.ExportFilePath, columnsToPrint);
                    }

                    // 4. Show success message
                    MessageBox.Show(
                        $"Successfully exported {students.Count()} students to:\n{exportDialog.ExportFilePath}",
                        "Export Successful",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // 5. Ask to open file
                    if (MessageBox.Show("Do you want to open the exported file?", "Open File",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(exportDialog.ExportFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Export failed: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void btnAddNewStudent_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add Student Dialog Placeholder");
            var addStudentDialog = new AddEditStudent();
            addStudentDialog.Owner = Application.Current.MainWindow;
            addStudentDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addStudentDialog.ShowDialog();
            LoadAllStudentsAsync();
        }

        private void cmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedStatusFilterIndex = cmbStatusFilter.SelectedIndex;
            ApplyFilters();
        }
        private void cmbGroupFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedGroupFilterIndex = cmbGroupFilter.SelectedIndex;
            ApplyFilters();
        }
        private void cmbSortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSortFilter = cmbSortBy.SelectedIndex;
            ApplyFilters();
        }
        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int studentID)
            {
                var student = _allStudents.FirstOrDefault(s => s.StudentID == studentID);
                if (student is null) return;
                var viewStudentDialog = new StudentInfoWindow(student);
                viewStudentDialog.Owner = Application.Current.MainWindow;
                viewStudentDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                viewStudentDialog.ShowDialog();
                LoadAllStudentsAsync();
            }
        }
        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int studentID)
            {
                var student = _allStudents.FirstOrDefault(s => s.StudentID == studentID);
                if (student is null) return;
                var editStudentDialog = new AddEditStudent(await student.MapFromViewModel());
                editStudentDialog.Owner = Application.Current.MainWindow;
                editStudentDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                editStudentDialog.ShowDialog();
                LoadAllStudentsAsync();
            }
        }
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int studentID)
            {
                var student = _allStudents.FirstOrDefault(s => s.StudentID == studentID);
                if (student is null) return;

                if (
                    MessageBox.Show(
                        $"Are you sure you want to delete {student.FullName}?",
                        "Confirm",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    bool success = await student.DeleteAsync();
                    if (success)
                    {
                        LoadAllStudentsAsync();
                        MessageBox.Show("Deleted Successfully");
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete.");
                    }
                }
            }
        }
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {

        }
        private void txtSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchQuery = txtSearchBox.Text;
            ApplyFilters();
        }

        #endregion

        // ======================================
        // 2- Helpers Methods!!!
        // ======================================
        #region Helpers Methods
        /// <summary>
        /// Loads all students from the database
        /// </summary>
        private async void LoadAllStudentsAsync()
        {
            try
            {
                _allStudents.Clear();

                var Students = await Student.GetActiveStudents();

                foreach (var student in Students)
                {
                    _allStudents.Add(new StudentVM(student));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading students: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                EventLogger.LogError("Error loading students In The UI", ex);
            }

            // Update statistics
            UpdateStatistics();

            // Apply filters
            ApplyFilters();
        }

        /// <summary>
        /// Loads group filters
        /// </summary>
        private async void LoadGroupFiltersAsync()
        {
            try
            {
                var groups = await Group.GetAllGroups();

                cmbGroupFilter.Items.Clear();
                cmbGroupFilter.Items.Add(new ComboBoxItem
                {
                    Content = "All Groups",
                    Style = (Style)FindResource("ModernComboBoxItem")
                });

                foreach (var group in groups)
                {
                    cmbGroupFilter.Items.Add(new ComboBoxItem
                    {
                        Content = group.GroupName,
                        Tag = group.GroupID,
                        Style = (Style)FindResource("ModernComboBoxItem")
                    });
                }

                cmbGroupFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Error loading group filters", ex);
            }
        }

        /// <summary>
        /// Gets initials from full name for avatar
        /// </summary>
        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "??";

            var parts = fullName.Trim().Split(' ');
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }

        /// <summary>
        /// Updates the statistics cards
        /// </summary>
        private void UpdateStatistics()
        {
            this.totalStudents = _allStudents.Count;

            this.active = _allStudents.Count(s => s.IsActive && s.StudentStatus == "Active");

            this.inactive = _allStudents.Count(s => !s.IsActive || s.StudentStatus == "Inactive");

            // Get Students Enrolled this mounth
            var thisMonth = DateTime.Now.Month;
            var thisYear = DateTime.Now.Year;
            int newThisMonth = _allStudents.Count(s =>
                s.AdmissionDate.Month == thisMonth &&
                s.AdmissionDate.Year == thisYear);
        }

        /// <summary>
        /// Applies all filters and search
        /// </summary>
        private void ApplyFilters()
        {
            // Start with all students
            IEnumerable<StudentVM> query = _allStudents;

            // 1. Search Filter (Safe & Case Insensitive)
            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                string q = _searchQuery.Trim();

                query = query.Where(s =>
                    (s.FullName != null && s.FullName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (s.StudentNumber != null && s.StudentNumber.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (s.Email != null && s.Email.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                );
            }

            // 2. Status Filter
            // Index 0 = All, 1 = Active, 2 = Inactive, 3 = Suspended, 4 = Graduated
            if (_selectedStatusFilterIndex > 0)
            {
                switch (_selectedStatusFilterIndex)
                {
                    case 1: query = query.Where(s => s.StudentStatus == "Active"); break;
                    case 2: query = query.Where(s => s.StudentStatus == "Inactive"); break;
                    case 3: query = query.Where(s => s.StudentStatus == "Suspended"); break;
                    case 4: query = query.Where(s => s.StudentStatus == "Graduated"); break;
                }
            }

            // 3. Sorting
            switch (_selectedSortFilter)
            {
                case 0: query = query.OrderBy(s => s.FullName); break;       // Name
                case 1: query = query.OrderByDescending(s => s.AdmissionDate); break; // Date
                case 2: query = query.OrderBy(s => s.StudentStatus); break;  // Status
                case 3: query = query.OrderBy(s => s.StudentNumber); break;  // Number
            }

            // Update filtered collection
            _filteredStudents = new ObservableCollection<StudentVM>(query);

            // Reset to page 1 whenever filters change so the user sees results
            _currentPage = 1;

            UpdatePagination();
        }

        /// <summary>
        /// Updates the data grid with current page
        /// </summary>
        private void UpdatePagination()
        {
            int totalItems = _filteredStudents.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / ITEMS_PER_PAGE);

            if (totalPages == 0) totalPages = 1;
            if (_currentPage > totalPages) _currentPage = totalPages;

            var pageItems = _filteredStudents
                .Skip((_currentPage - 1) * ITEMS_PER_PAGE)
                .Take(ITEMS_PER_PAGE)
                .ToList();

            dgStudents.ItemsSource = pageItems;

            // Update Labels (Assuming you have these TextBlocks in XAML)
            if (runTotalCount != null) runTotalCount.Text = totalItems.ToString();
            if (runCurrentPage != null) runCurrentPage.Text = _currentPage.ToString();
            if (runTotalPages != null) runTotalPages.Text = totalPages.ToString();

            // Enable/Disable Buttons
            if (btnPrevious != null) btnPrevious.IsEnabled = _currentPage > 1;
            if (btnNext != null) btnNext.IsEnabled = _currentPage < totalPages;
        }

        #endregion

        // ======================================
        // 3- INotifyPropertyChanged Implementation
        // ======================================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
