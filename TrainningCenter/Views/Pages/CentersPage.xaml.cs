using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter.UserControles.Centers;
using TrainningCenter.ViewModel;
using TrainningCenter.Views.Windows.Centers;

namespace TrainningCenter.Views.Pages
{
    /// <summary>
    /// Interaction logic for CentersPage.xaml
    /// </summary>
    public partial class CentersPage : Page
    {
        private ObservableCollection<CenterVM> _centerVMs;
        private string _searchQuery = string.Empty;
        private int _selectedStatusIndex;
        public CentersPage()
        {
            InitializeComponent();

            _centerVMs = new ObservableCollection<CenterVM>();

            // load All Centers
            LoadAllCenters();

            btnGridView_Click(null, null);
        }

        #region Event Handlers
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAllCenters();
        }

        private async void btnAddNewCenter_Click(object sender, RoutedEventArgs e)
        {
            var AddNewCenter = new AddEditCenterWindow();
            AddNewCenter.Owner = Application.Current.MainWindow;
            AddNewCenter.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            AddNewCenter.ShowDialog();
            LoadAllCenters();
        }

        private void txtSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchQuery = txtSearchBox.Text;
            ApplyFilters();
        }

        private void btnGridView_Click(object sender, RoutedEventArgs e)
        {
            // Already in grid view
            btnGridView.Opacity = 1.0;
            btnListView.Opacity = 0.5;

            listIcon.Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"];
            gridIcon.Foreground = (Brush)Application.Current.Resources["PrimaryBrush"];

            // 2. Swap Templates
            centersItemsControl.ItemTemplate = (DataTemplate)FindResource("GridViewTemplate");
            centersItemsControl.ItemsPanel = (ItemsPanelTemplate)FindResource("GridPanelTemplate");
        }

        private void btnListView_Click(object sender, RoutedEventArgs e)
        {
            // Switch to list view (TODO: implement)
            btnGridView.Opacity = 0.5;
            btnListView.Opacity = 1.0;

            listIcon.Foreground = (Brush)Application.Current.Resources["PrimaryBrush"];
            gridIcon.Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"];

            // 2. Swap Templates
            centersItemsControl.ItemTemplate = (DataTemplate)FindResource("ListViewTemplate");
            centersItemsControl.ItemsPanel = (ItemsPanelTemplate)FindResource("ListPanelTemplate");
        }

        private void cmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedStatusIndex = cmbStatusFilter.SelectedIndex;
            ApplyFilters();
        }

        private async void CenterCard_EditClicked(object sender, RoutedEventArgs e)
        {
            if (sender is CenterCard card && card.DataContext is CenterVM vm)
            {
                // 2. Get ID from the ViewModel directly
                int centerID = vm.CenterID;

                try
                {
                    // 3. Fetch fresh data from DB (Good practice before editing)
                    var centerModel = await Center.Find(centerID);

                    if (centerModel == null)
                    {
                        MessageBox.Show(
                            $"Cannot find Center with ID: {centerID}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    // 4. Open Edit Window
                    var editWindow = new AddEditCenterWindow(new CenterVM(centerModel));
                    editWindow.Owner = Application.Current.MainWindow;
                    editWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                    // 5. Check result and Refresh
                    if (editWindow.ShowDialog() == true)
                    {
                        LoadAllCenters(); // Refresh the list to show changes
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error loading center: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private async void CenterCard_DeleteClicked(object sender, RoutedEventArgs e)
        {
            // Exact same logic, just different sender type (CenterListItem)
            if (sender is CenterCard item && item.DataContext is CenterVM vm)
            {
                PerformDelete(vm);
            }
        }

        private async void CenterCard_CardClicked(object sender, RoutedEventArgs e)
        {
            // 1. Cast sender to the UserControl
            if (sender is CenterCard card && card.DataContext is CenterVM vm)
            {
                // 2. Instead of Deleting, trigger the Edit Logic
                // Since CenterCard_EditClicked expects 'sender' to be the card, we can reuse it directly.
                // Or better yet, extract the 'Edit' logic into a shared method like 'PerformEdit(vm)'.

                var CenterInfo = new ViewCenterInfoWindow(vm);
                CenterInfo.Owner = Application.Current.MainWindow;
                CenterInfo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                CenterInfo.ShowDialog();
            }
        }

        private async void CenterListItem_EditClicked(object sender, RoutedEventArgs e)
        {
            if (sender is CenterListItem card && card.DataContext is CenterVM vm)
            {
                // 2. Get ID from the ViewModel directly
                int centerID = vm.CenterID;

                try
                {
                    // 3. Fetch fresh data from DB (Good practice before editing)
                    var centerModel = await Center.Find(centerID);

                    if (centerModel == null)
                    {
                        MessageBox.Show(
                            $"Cannot find Center with ID: {centerID}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    // 4. Open Edit Window
                    var editWindow = new AddEditCenterWindow(new CenterVM(centerModel));
                    editWindow.Owner = Application.Current.MainWindow;
                    editWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                    // 5. Check result and Refresh
                    if (editWindow.ShowDialog() == true)
                    {
                        LoadAllCenters(); // Refresh the list to show changes
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error loading center: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private async void CenterListItem_DeleteClicked(object sender, RoutedEventArgs e)
        {
            // Exact same logic, just different sender type (CenterListItem)
            if (sender is CenterListItem item && item.DataContext is CenterVM vm)
            {
                PerformDelete(vm);
            }
        }
        #endregion

        #region Helpers

        private async void LoadAllCenters()
        {
            try
            {
                _centerVMs.Clear();

                var centersFromDb = await Center.GetAllCenters();

                foreach (var center in centersFromDb)
                {
                    _centerVMs.Add(new CenterVM(center));
                }

                centersItemsControl.ItemsSource = _centerVMs;

                if (_centerVMs.Count == 0)
                {
                    EmptyState.Visibility = Visibility.Visible;
                    centersItemsControl.Visibility = Visibility.Collapsed; // Hide empty list
                }
                else
                {
                    EmptyState.Visibility = Visibility.Collapsed;
                    centersItemsControl.Visibility = Visibility.Visible;
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading centers: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                EventLogger.LogError("Error loading centers", ex);
            }
        }

        private void ApplyFilters()
        {
            IEnumerable<CenterVM> query = _centerVMs;

            // 1. Search (Name, Email, Phone)
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                string q = _searchQuery.Trim();
                query = query.Where(c =>
                    (c.CenterName != null && c.CenterName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (c.City != null && c.City.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (c.Address != null && c.Address.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (c.Facilities != null && c.Facilities.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                );
            }

            // 2. Status Filter
            if (_selectedStatusIndex > 0)
            {
                switch (_selectedStatusIndex)
                {
                    case 1: query = query.Where(c => c.IsActive); break;
                    case 2: query = query.Where(c => !c.IsActive); break;
                }
            }

            // 3. EXECUTE & UPDATE UI
            var filteredList = query.ToList();
            centersItemsControl.ItemsSource = filteredList;

            // 4. Handle Empty State (If search returns nothing)
            if (filteredList.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                centersItemsControl.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyState.Visibility = Visibility.Collapsed;
                centersItemsControl.Visibility = Visibility.Visible;
            }
        }

        private async void PerformDelete(CenterVM vm)
        {
            // 1. PRE-CHECK: Look at the counts we already loaded
            if (vm.TrainersCount > 0 || vm.GroupsCount > 0)
            {
                // Build a specific message based on what is found
                string details = "";

                if (vm.TrainersCount > 0)
                    details += $"• {vm.TrainersCount} Active Trainer(s)\n";

                if (vm.GroupsCount > 0)
                    details += $"• {vm.GroupsCount} Training Group(s)\n";

                MessageBox.Show(
                    $"This Center cannot be deleted because it has related data:\n\n{details}\n" +
                    "Please reassign or delete these records before deleting the center.",
                    "Cannot Delete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // Stop here. Do not try to delete from DB.
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete {vm.CenterName}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (await vm.DeleteAsync())
                    {
                        MessageBox.Show("Center deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadAllCenters(); // Refresh UI
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete center.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    // Fallback for Stale Data (e.g., someone added a trainer 1 second ago)
                    if (ex.Message.Contains("active Trainers") || ex.Message.Contains("FK_"))
                    {
                        MessageBox.Show("The Database rejected the delete. New data may have been added recently.", "Constraint Error");
                    }
                    else
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Error");
                    }
                }
            }
        }

        #endregion

    }
}
