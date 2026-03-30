using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using TrainningCenter.ViewModel;

namespace TrainningCenter.Views.Windows.Centers
{
    /// <summary>
    /// Interaction logic for ViewCenterInfoWindow.xaml
    /// </summary>
    public partial class ViewCenterInfoWindow : Window
    {
        private CenterVM _vm;
        public ViewCenterInfoWindow(CenterVM vm)
        {
            InitializeComponent();
            _vm = vm;
            this.DataContext = _vm;

            // Generate Facility Tags dynamically
            LoadFacilitiesTags();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnEditCenter_Click(object sender, RoutedEventArgs e)
        {
            // Open the Edit Window
            // We pass the same VM (or a refreshable copy)
            var editWindow = new AddEditCenterWindow(_vm);
            editWindow.Owner = this;

            bool? result = editWindow.ShowDialog();

            if (result == true)
            {
                // If saved, refresh tags in case facilities changed
                LoadFacilitiesTags();
                // Since we passed the VM by reference and it implements INotifyPropertyChanged, 
                // the main texts (Name, Capacity) update automatically! 
            }
        }

        private async void btnDeleteCenter_Click(object sender, RoutedEventArgs e)
        {
            // 1. Check Constraints (Trainers/Groups)
            if (_vm.TrainersCount > 0 || _vm.GroupsCount > 0)
            {
                MessageBox.Show(
                    $"Cannot delete this center because it has related data ({_vm.TrainersCount} Trainers, {_vm.GroupsCount} Groups).",
                    "Constraint Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // 2. Confirm
            if (MessageBox.Show($"Are you sure you want to delete {_vm.CenterName}?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = await _vm.DeleteAsync();
                    if (success)
                    {
                        MessageBox.Show("Center deleted successfully.", "Success");
                        this.DialogResult = true; // Signal parent to refresh list
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete center.", "Error");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        private void LoadFacilitiesTags()
        {
            facilitiesTagsPanel.Children.Clear();

            if (string.IsNullOrWhiteSpace(_vm.Facilities)) return;

            // Split by comma
            var facilities = _vm.Facilities.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach(var facility in facilities )
            {
                // Create a nice Border for each tag
                Border tag = new Border
                {
                    Background = (Brush)FindResource("InputBackgroundBrush"),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(10, 5, 10, 5),
                    Margin = new Thickness(0, 0, 8, 8)
                };

                TextBlock text = new TextBlock
                {
                    Text = facility.Trim(),
                    FontSize = 12,
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    FontWeight = FontWeights.Medium
                };

                tag.Child = text;
                facilitiesTagsPanel.Children.Add(tag);
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
