using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter.ViewModel;
using TrainningCenter.Views.Windows;
using TrainningCenter.Views.Windows.Trainers;
using TrainningCenter_Entities;

namespace TrainningCenter.Views.Pages
{
    /// <summary>
    /// Interaction logic for TrainersPage.xaml
    /// </summary>
    public partial class TrainersPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<TrainerVM> _allTrainers;
        private ObservableCollection<TrainerVM> _filteredTrainers;

        private string _searchQuery = string.Empty;
        private int _selectedStatusFilter = 0;
        private int _selectedSortIndex = 0;


        private int _currentPage = 1;
        private const int ITEMS_PER_PAGE = 25;

        #region Statistics Properties (Bound to UI)

        private int _totalTrainers;
        public int TotalTrainers
        {
            get => _totalTrainers;
            set { _totalTrainers = value; OnPropertyChanged(); }
        }

        private int _activeTrainers;
        public int ActiveTrainers
        {
            get => _activeTrainers;
            set { _activeTrainers = value; OnPropertyChanged(); }
        }

        private int _availableToday;
        public int AvailableToday
        {
            get => _availableToday;
            set { _availableToday = value; OnPropertyChanged(); }
        }

        private int _onLeave;
        public int OnLeave
        {
            get => _onLeave;
            set { _onLeave = value; OnPropertyChanged(); }
        }

        #endregion
        public TrainersPage()
        {
            InitializeComponent();

            this.DataContext = this;

            _allTrainers = new ObservableCollection<TrainerVM>();
            _filteredTrainers = new ObservableCollection<TrainerVM>();

            // Set default selections
            cmbStatusFilter.SelectedIndex = 0;
            cmbSpecializationFilter.SelectedIndex = 0;
            cmbSortBy.SelectedIndex = 0;

            LoadAllTrainers();
        }

        #region Event Handlers
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAllTrainers();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            // 1. Get ALL trainers that pass the current filters (not just the current page)
            var dataToExport = _filteredTrainers.ToList();

            if (dataToExport == null || !dataToExport.Any())
            {
                MessageBox.Show("No trainer data matches your current filters to export.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 2. Open your custom Configuration Window
            var configWindow = new ExportConfigWindow(dataToExport, dataToExport.Count);
            configWindow.Owner = Application.Current.MainWindow;

            if (configWindow.ShowDialog() == true)
            {
                try
                {
                    var fileService = new FileService();
                    string filePath = configWindow.ExportFilePath;

                    if (configWindow.ExportFormat == "Excel")
                    {
                        // Simple export to Excel
                        fileService.ExportToExcel(dataToExport, filePath);
                    }
                    else if (configWindow.ExportFormat == "PDF")
                    {
                        // 3. Define the specific columns for the Trainer Report
                        var columns = new List<string>
                        {
                            "FullName",
                            "Specialization",
                            "ExperienceYears",
                            "DisplayStatus",
                            "Rating",
                            "HireDate"
                        };

                        // Use the Document Title from your window
                        fileService.ExportToPdf(dataToExport, configWindow.DocumentTitle, filePath, columns);
                    }

                    MessageBox.Show("Trainers list exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAddTrainer_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add Trainer Dialog Placeholder");
            var addNewTrainerWindow = new AddEditTrainerWindow();
            addNewTrainerWindow.Owner = Application.Current.MainWindow;
            addNewTrainerWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addNewTrainerWindow.ShowDialog();
        }

        private void txtSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchQuery = txtSearchBox.Text;
            ApplyFilters();
        }

        private void cmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedStatusFilter = cmbStatusFilter.SelectedIndex;
            ApplyFilters();
        }

        private void cmbSpecializationFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            // 🛡️ CRASH PROTECTION: Clear() triggers this event with null
            if (cmbSpecializationFilter.SelectedItem != null)
            {
                ApplyFilters();
            }
        }

        private void cmbSortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSortIndex = cmbSortBy.SelectedIndex;
            ApplyFilters();
        }

        private async void btnViewInfo_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.Tag is int trainerID)
            {
                Trainer t = await Trainer.Find(trainerID);
                if (t != null) 
                {
                    var trainer = new TrainerVM(t);
                    var viewTrainerInfoWindow = new TrainerInformationsWindow(trainer);
                    viewTrainerInfoWindow.Owner = Application.Current.MainWindow;
                    viewTrainerInfoWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    viewTrainerInfoWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show(
                        "An Error Was ocured while try to fetch that Trainer!",
                        "Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);

                    return;
                }
            }
        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.Tag is int trainerID)
            {
                Trainer trainer = await Trainer.Find(trainerID);
                if(trainer != null)
                {
                    var t = new TrainerVM(trainer);
                    var editTrainerWindow = new AddEditTrainerWindow(t);
                    editTrainerWindow.Owner = Application.Current.MainWindow;
                    editTrainerWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    editTrainerWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show(
                        "An Error Was ocured while try to fetch that Trainer!",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int trainerID)
            {
                var trainer = _allTrainers.FirstOrDefault(t => t.TrainerID == trainerID);
                if (trainer == null) return;

                if (MessageBox.Show($"Are you sure you want to delete trainer: {trainer.FullName}?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool seccess = await trainer.DeleteAsync();
                        if (seccess)
                        {
                            MessageBox.Show("Trainer deleted successfully.", "Success");
                            LoadAllTrainers();
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete trainer.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                        EventLogger.LogError($"Error was occured while deleting the Trainer ID {trainerID}", ex);
                    }
                }
            }
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1) { _currentPage--; UpdatePagination(); }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_filteredTrainers.Count / ITEMS_PER_PAGE);
            if (_currentPage < totalPages) { _currentPage++; UpdatePagination(); }
        }

        #endregion

        #region Fetch Methods

        private async void LoadAllTrainers()
        {
            try
            {
                _allTrainers.Clear();

                var trainersFromDb = await Trainer.GetAllTrianers();

                foreach (var trainer in trainersFromDb)
                {
                    _allTrainers.Add(new TrainerVM(trainer));
                }

                // 1. Update Dropdown for Specializations dynamically
                LoadSpecializationFilter();

                // 2. Calculate Stats
                UpdateStatistics();

                // 3. Trigger UI Refresh
                ApplyFilters();


                if (_allTrainers.Count == 0)
                    ctrlEmpltyOverlay.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading trainers: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                EventLogger.LogError("Error loading trainers", ex);
            }
        }

        private async void LoadSpecializationFilter()
        {
            // Get Unique specializations from the loaded data
            var specs = _allTrainers
                .Select(t => t.Specialization)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            // 2. TEMPORARILY Unhook the SelectionChanged event
            // This prevents ApplyFilters from running while we are clearing/adding items
            cmbSpecializationFilter.SelectionChanged -= cmbSpecializationFilter_SelectionChanged;

            // Reset Combox
            cmbSpecializationFilter.Items.Clear();

            // Add Default
            ComboBoxItem allItem = new ComboBoxItem { Content = "All Specializations" };

            // Copy style from XAML resource if needed, or rely on implicit styles
            if (TryFindResource("ModernComboBoxItem") is Style style)
                allItem.Style = style;

            cmbSpecializationFilter.Items.Add(allItem);

            // Add Items
            foreach (var spec in specs)
            {
                ComboBoxItem item = new ComboBoxItem { Content = spec };
                if (TryFindResource("ModernComboBoxItem") is Style itemStyle)
                    item.Style = itemStyle;

            }

            cmbSpecializationFilter.SelectedIndex = 0;

            // 🔴 ADD THIS LINE HERE: Re-hook the event!
            cmbSpecializationFilter.SelectionChanged += cmbSpecializationFilter_SelectionChanged;
        }
        #endregion

        #region Helpers Methods

        private void UpdatePagination()
        {
            // 1. Calculate Paging Logic
            int totalItems = _filteredTrainers.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / ITEMS_PER_PAGE);
            if (totalPages == 0) totalPages = 1;

            // 2. Slice the data (Get only the top 20 for this page)
            var pageItems = _filteredTrainers
                .Skip((_currentPage - 1) * ITEMS_PER_PAGE)
                .Take(ITEMS_PER_PAGE)
                .ToList();

            // 3. Manual Binding
            dgTrainers.ItemsSource = pageItems;

            // 4. Update UI Text (Page X of Y)
            // (Make sure you have these x:Name's in your XAML)
            if (runTotalCount != null) runTotalCount.Text = totalItems.ToString();
            if (runCurrentPage != null) runCurrentPage.Text = _currentPage.ToString();
            if (runTotalPages != null) runTotalPages.Text = totalPages.ToString();

            // 5. Update Buttons (Enable/Disable)
            if (btnPrevious != null) btnPrevious.IsEnabled = _currentPage > 1;
            if (btnNext != null) btnNext.IsEnabled = _currentPage < totalPages;
        }

        private void ApplyFilters()
        {
            IEnumerable<TrainerVM> query = _allTrainers;

            // 1. Search (Name, Email, Specialization)
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                string q = _searchQuery.Trim();
                query = query.Where(t =>
                    (t.FullName != null && t.FullName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (t.Email != null && t.Email.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (t.Specialization != null && t.Specialization.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                );
            }

            // 2. Status Filter
            if (_selectedStatusFilter > 0)
            {
                switch (_selectedStatusFilter)
                {
                    case 1: query = query.Where(t => t.IsActive); break; // Active
                    case 2: query = query.Where(t => t.DisplayStatus == "On Leave"); break; // On Leave
                    case 3: query = query.Where(t => !t.IsActive); break; // Inactive
                }
            }

            // 3. Specialization Filter
            if (cmbSpecializationFilter.SelectedItem is ComboBoxItem item &&
                item.Content.ToString() != "All Specializations")
            {
                string selectedSpec = item.Content.ToString();
                query = query.Where(t => t.Specialization == selectedSpec);
            }

            // 4. Sort
            switch (_selectedSortIndex)
            {
                case 0: query = query.OrderBy(t => t.FullName); break; // Name
                case 1: query = query.OrderByDescending(t => t.ExperienceYears); break; // Experience
                case 2: query = query.OrderByDescending(t => t.Rating); break; // Rating
                case 3: query = query.OrderByDescending(t => t.HireDate); break; // Hire Date
            }

            // Execute Query
            _filteredTrainers = new ObservableCollection<TrainerVM>(query);

            // Reset Page
            _currentPage = 1;
            UpdatePagination();
        }

        private void UpdateStatistics()
        {
            TotalTrainers = _allTrainers.Count;
            ActiveTrainers = _allTrainers.Count(t => t.IsActive);

            // Logic for "On Leave" depends on your Status string implementation
            OnLeave = _allTrainers.Count(t => t.DisplayStatus?.ToLower() == "on leave");

            // Place holder logic for "Available Today"
            AvailableToday = _allTrainers.Count(t => t.IsActive && t.DisplayStatus?.ToLower() != "on leave");
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
