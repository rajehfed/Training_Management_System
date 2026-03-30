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
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter.ViewModel;

namespace TrainningCenter.Views.Windows.Groups
{
    /// <summary>
    /// Interaction logic for AddEditGroup.xaml
    /// </summary>
    public partial class AddEditGroup : Window
    {
        private bool _isEditMode = false;
        private GroupVM _vm = null;
        private Group _group = null;

        /// <summary>
        /// Constructor for ADD mode
        /// </summary>
        public AddEditGroup()
        {
            InitializeComponent();
            _vm = new GroupVM();

            this.DataContext = _vm;
            _isEditMode = false;
            Title = "Add New Group";
            tbTitle.Text = "Add New Group";
            tbDescription.Text = "Create a new training group";
            iconHeader.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.AccountGroupOutline;
            btnSave.Content = "Save Group";

            // Initialize Form
            //InitializeForm();
            _ = LoadDataAndSetContextAsync();
        }

        /// <summary>
        /// Constructor for EDIT mode
        /// </summary>
        /// <param name="groupToEdit">The group to edit</param>
        public AddEditGroup(GroupVM vm)
        {
            InitializeComponent();

            _vm = vm;
            this.DataContext = _vm;

            _isEditMode = true;
            Title = "Edit Group";
            tbTitle.Text = "Edit Group Details";
            tbDescription.Text = $"Modify {_vm.GroupName} details";
            iconHeader.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pencil;
            btnSave.Content = "Update Group";

            _ = LoadDataAndSetContextAsync();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            btnCancel_Click(sender, e);
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to cancel? Any unsaved changes will be lost.",
                "Confirm Cancel",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes) {
                DialogResult = false;
                Close();
            }
        }
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            // Track success state
            bool success = false;
            try
            {
                // Disable button to prevent double-click
                btnSave.IsEnabled = false;
                btnSave.Content = _isEditMode ? "Updating..." : "Saving...";

                // Create group from form
                Group group = await _vm.MapFromViewModel();

                // Save to the DB
                success = await group.Save();
                if (success)
                {
                    MessageBox.Show(
                        "Group saved successfully!",
                        "Success", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    this.DialogResult = true; // Indicate success to the calling window
                    Close();
                }
                else
                {
                    MessageBox.Show(
                        "Failed to save group. The database operation was unsuccessful.", 
                        "Warning", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                }

            }
            catch(Exception ex)
            {
                // Log the error and notify the user
                EventLogger.LogError($"Error during save operation: {ex.Message}", ex);
                MessageBox.Show(
                    $"An unexpected error occurred while saving the group: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                // Always re-enable the button and reset the content
                btnSave.IsEnabled = true;
                btnSave.Content = _isEditMode ? "Update Group" : "Save Group";
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // INITIALIZATION METHODS
        // ═══════════════════════════════════════════════════════════════

        private async Task LoadDataAndSetContextAsync()
        {
            await _vm.LoadComboBoxSourcesAsync(); // Load the lists into the ViewModel
            this.DataContext = _vm; // Set the DataContext
        }


        // ═══════════════════════════════════════════════════════════════
        // VALIDATION METHODS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Validates all required fields
        /// </summary>
        private bool ValidateForm()
        {
            var errors = new List<string>();

            // Group Name
            if (string.IsNullOrWhiteSpace(txtGroupName.Text))
                errors.Add("• Group Name is required");

            // Center
            if (cmbCenter.SelectedValue == null)
                errors.Add("• Training Center is required");

            // Specialization
            if (cmbSpecialization.SelectedValue == null)
                errors.Add("• Specialization is required");

            // Trainer
            if (cmbTrainer.SelectedValue == null)
                errors.Add("• Trainer is required");

            // Max Trainees
            if (string.IsNullOrWhiteSpace(txtMaxTrainees.Text))
                errors.Add("• Maximum Trainees is required");
            else if (!int.TryParse(txtMaxTrainees.Text, out int maxTrainees) || maxTrainees <= 0)
                errors.Add("• Maximum Trainees must be a positive number");

            // Start Date
            if (dpStartDate.SelectedDate == null)
                errors.Add("• Start Date is required");

            // End Date
            if (dpEndDate.SelectedDate == null)
                errors.Add("• End Date is required");
            else if (dpStartDate.SelectedDate != null && dpEndDate.SelectedDate < dpStartDate.SelectedDate)
                errors.Add("• End Date must be after Start Date");

            // Show errors if any
            if (errors.Any())
            {
                MessageBox.Show(
                    "Please fix the following errors:\n\n" + string.Join("\n", errors),
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

    }
}
