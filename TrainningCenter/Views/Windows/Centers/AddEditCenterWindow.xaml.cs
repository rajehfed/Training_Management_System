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
using TrainningCenter.Helpers;
using TrainningCenter.ViewModel;

namespace TrainningCenter.Views.Windows.Centers
{
    /// <summary>
    /// Interaction logic for AddEditCenterWindow.xaml
    /// </summary>
    public partial class AddEditCenterWindow : Window
    {
        private CenterVM _center;
        private bool _isEditMode = false;

        #region Constructors
        public AddEditCenterWindow()
        {
            InitializeComponent();
            InitializeAddMode();

        }
        public AddEditCenterWindow(CenterVM center)
        {
            InitializeComponent();
            _center = center;
            _isEditMode = true;

            InitializeEditMode();
        }
        #endregion

        #region Initialization

        private void InitializeAddMode()
        {
            _isEditMode = false;
            _center = new CenterVM();

            tbTitle.Text = "Add Training Center";
            tbSubtitle.Text = "Fill in the center information below";
            iconPreview.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.OfficeBuildingPlus;

            // FIX: Set DataContext so bindings work!
            this.DataContext = _center;

            btnDelete.Visibility = Visibility.Collapsed;

            // Set defaults
            txtCapacity.Text = "20";
            chkIsActive.IsChecked = true;
        }
        private void InitializeEditMode()
        {
            _isEditMode = true;

            tbTitle.Text = "Edit Training Center";
            tbSubtitle.Text = $"Editing: {_center.CenterName}";
            iconPreview.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.OfficeBuildingCog;

            btnDelete.Visibility = Visibility.Visible;

            this.DataContext = _center;
        }

        #endregion

        #region Event Handlers
        private void btnCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                $"Are you sure you want to delete {_center.CenterName}?",
                "Confirm Delete", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    bool seccess = await _center.DeleteAsync();
                    if (seccess)
                    {
                        MessageBox.Show(
                            "Center deleted successfully.", 
                            "Success", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);

                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Failed to delete center (It might have related data).", 
                            "Error", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(
                        $"Error: {ex.Message}", 
                        "Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);

                    EventLogger.LogWarning($"Error: {ex.Message}");
                }
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                bool success = await _center.SaveAsync();
                if (success)
                {
                    string msg = _isEditMode ? "Center updated successfully." : "Center added successfully.";
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
                        "Failed to save center.", 
                        "Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);

                // Ensure you have EventLogger in your namespace
                 EventLogger.LogError("Error saving center", ex);
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Helpers

        private bool ValidateForm()
        {
            bool isValid = true;

            // Reset Visual Errors
            txtCenterName.BorderBrush = (Brush)FindResource("BorderBrush");
            errorCenterName.Visibility = Visibility.Collapsed;

            txtCenterCity.BorderBrush = (Brush)FindResource("BorderBrush");
            errorCity.Visibility = Visibility.Collapsed;

            txtAddress.BorderBrush = (Brush)FindResource("BorderBrush");
            errorAddress.Visibility = Visibility.Collapsed;

            txtCapacity.BorderBrush = (Brush)FindResource("BorderBrush");
            errorCapacity.Visibility = Visibility.Collapsed;

            txtEmail.BorderBrush = (Brush)FindResource("BorderBrush");
            errorEmail.Visibility = Visibility.Collapsed;

            // 1. Center Name
            if (string.IsNullOrWhiteSpace(txtCenterName.Text))
            {
                errorCenterName.Visibility = Visibility.Visible;
                txtCenterName.BorderBrush = Brushes.Red;
                isValid = false;
            }

            // 2. City
            if (string.IsNullOrWhiteSpace(txtCenterCity.Text))
            {
                errorCity.Visibility = Visibility.Visible;
                txtCenterCity.BorderBrush = Brushes.Red;
                isValid = false;
            }

            // 3. Address
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                errorAddress.Visibility = Visibility.Visible;
                txtAddress.BorderBrush = Brushes.Red;
                isValid = false;
            }

            // 4. Capacity
            if (!int.TryParse(txtCapacity.Text, out int cap) || cap <= 0)
            {
                errorCapacity.Visibility = Visibility.Visible;
                txtCapacity.BorderBrush = Brushes.Red;
                isValid = false;
            }

            // 5. Email
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@"))
            {
                errorEmail.Visibility = Visibility.Visible;
                txtEmail.BorderBrush = Brushes.Red;
                isValid = false;
            }

            return isValid;
        }

        #endregion
    }
}
