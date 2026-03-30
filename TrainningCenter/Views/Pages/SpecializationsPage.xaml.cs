using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
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
using TrainningCenter.Views.Windows;

namespace TrainningCenter.Views.Pages
{
    /// <summary>
    /// Interaction logic for SpecializationsPage.xaml
    /// </summary>
    public partial class SpecializationsPage : Page
    {
        public class SpecializationViewModel
        {
            public int SpecializationId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsActive { get; set; }
            public int StudentCount { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public ObservableCollection<SpecializationViewModel> _AllSpecializations = new ObservableCollection<SpecializationViewModel>();
        public SpecializationsPage()
        {
            InitializeComponent();
            itemSpecialization.ItemsSource = _AllSpecializations;
            DataContext = this;
            LoadSpecializations();
        }

        private async void LoadSpecializations()
        {
            try
            {
                var specializations = await Specialization.GetAllSpecializations();
                _AllSpecializations.Clear();

                foreach (var spec in specializations)
                {
                    int studentCount = await Group.GetStudentCountBySpecialization(spec.SpecializationID);
                    _AllSpecializations.Add(new SpecializationViewModel
                    {
                        SpecializationId = spec.SpecializationID,
                        Name = spec.SpecializationName,
                        Description = spec.Description,
                        IsActive = spec.IsActive,
                        StudentCount = studentCount,
                        CreatedAt = spec.CreatedAt
                    });
                }

                UpdateStatistics();
            }
            catch(Exception ex)
            {
                EventLogger.LogError("Error loading specializations", ex);
                MessageBox.Show(
                    $"Error loading specializations: {ex.Message}", 
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            if (_AllSpecializations == null)
                return;

            tbTotalCount.Text = _AllSpecializations.Count.ToString();
            tbActiveCount.Text = _AllSpecializations.Count(s => s.IsActive).ToString();
            tbTotalStudents.Text = _AllSpecializations.Sum(s => s.StudentCount).ToString();

            // Recent Additions (this month)
            var thisMonth = DateTime.Now.Month;
            var thisYear = DateTime.Now.Year;
            tbRecentCount.Text = _AllSpecializations
                .Count(s => s.CreatedAt.Month == thisMonth && s.CreatedAt.Year == thisYear)
                .ToString();
        }
        private void ApplyFilter()
        {
            if (!IsLoaded) return;

            if(_AllSpecializations == null || _AllSpecializations.Count == 0)
            {
                itemSpecialization.ItemsSource = null;
                return;
            }

            IEnumerable<SpecializationViewModel> filtredList = _AllSpecializations;

            // 1. Search filter
            string searchTerm = txtSearch.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                filtredList = filtredList
                    .Where(s =>
                        (s.Name?.ToLower().Contains(searchTerm) ?? false) ||
                        (s.Description?.ToLower().Contains(searchTerm) ?? false)
                    );
            }

            // 2. Status Filter
            if(cmbStatusFilter?.SelectedIndex > 0)
            {
                var selectedItem = cmbStatusFilter.SelectedItem as ComboBoxItem;
                if(selectedItem != null)
                {
                    bool isActive = selectedItem.Content.ToString() == "Active";
                    filtredList = filtredList.Where(s => s.IsActive == isActive);
                }
            }

            // 3. Update Display
            var result = filtredList.ToList();
            itemSpecialization.ItemsSource = null;
            itemSpecialization.ItemsSource = result;

            // 4. Show/Hide Empty State
            bool hasResults = result.Any();
            if(itemSpecialization?.Parent is ScrollViewer scrollViewer)
            {
                scrollViewer.Visibility = hasResults ? Visibility.Visible : Visibility.Collapsed;
            }

            if(pnlEmptyState != null)
            {
                pnlEmptyState.Visibility = hasResults ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void btnAddNewSpecialization_Click(object sender, RoutedEventArgs e)
        {
            AddEditSpecialization addNewSpecialization = new AddEditSpecialization();
            addNewSpecialization.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addNewSpecialization.Owner = Window.GetWindow(this);
            addNewSpecialization.ShowDialog();
            LoadSpecializations();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void cmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmbStatusFilter.SelectedIndex = 0;
            LoadSpecializations();
        }

        private void SpecializationCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if(e.ClickCount < 2)
            {
                if (border.Tag is int specializationID)
                {
                    var viewDetailedInfo = new SpecializationInfo(specializationID);
                    viewDetailedInfo.Owner = Window.GetWindow(this);
                    viewDetailedInfo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    viewDetailedInfo.ShowDialog();
                }
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if(btn.Tag is int specializationID)
            {
                var editSpecialization = new AddEditSpecialization(specializationID);
                editSpecialization.Owner = Window.GetWindow(this);
                editSpecialization.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                if(editSpecialization.ShowDialog() == true)
                {
                    LoadSpecializations();
                }
            }
            else
            {
                MessageBox.Show(
                    "Specialization not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

        }

        private async void btnClick_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if(btn.Tag is int specializationID)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this Specialization?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if(result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Specialization specialization = await Specialization.Find(specializationID);
                        if(specialization != null)
                        {
                            if(await specialization.Delete())
                            {
                                MessageBox.Show(
                                   "User deleted successfully.",
                                   "Success",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                                EventLogger.LogInfo($"Specialization deleted: ID={specialization.SpecializationID}, Name={specialization.SpecializationName}");
                                LoadSpecializations();
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Failed to Delete Specialization.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                EventLogger.LogWarning($"Failed to delete Specialization:  ID={specialization.SpecializationID}, Name={specialization.SpecializationName}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error deleting specialization", ex);
                        MessageBox.Show(
                            $"Error deleting specialization: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnAddSpecialization_Click(object sender, RoutedEventArgs e)
        {
            btnAddNewSpecialization_Click(sender, e);
        }
    }
}
