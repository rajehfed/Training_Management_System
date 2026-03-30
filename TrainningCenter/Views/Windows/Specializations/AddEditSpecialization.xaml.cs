using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter.ViewModel;

namespace TrainningCenter.Views.Windows
{
    /// <summary>
    /// Interaction logic for AddEditSpecialization.xaml
    /// </summary>
    public partial class AddEditSpecialization : Window
    {
        bool _isEditMode = false;
        Specialization _CurrentSpecialization = null;
        private SpecializationVM _vm;
        public AddEditSpecialization()
        {
            InitializeComponent();
            _isEditMode = false;
            tbModalTitle.Text = "Add New Specialization";
            btnSave.Content = "Add Specialization";
        }

        public AddEditSpecialization(int SpecializationID)
        {
            InitializeComponent();
            _isEditMode = true;
            tbModalTitle.Text = "Edit Specialization Details";
            btnSave.Content = "Save Changes";

            if(!DesignerProperties.GetIsInDesignMode(this)) {
                LoadSpecializationInfo(SpecializationID);
            }
        }

        private async void LoadSpecializationInfo(int SpecializationID)
        {
            if (_isEditMode)
            {
                try
                {
                    _CurrentSpecialization = await Specialization.Find(SpecializationID);
                    if(_CurrentSpecialization == null)
                    {
                        MessageBox.Show("Specialization not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        EventLogger.LogWarning($"Cannot Find Specialization With ID: {SpecializationID}");
                        DialogResult = false;
                        this.Close();
                        return;
                    }

                    _vm = new SpecializationVM(_CurrentSpecialization);

                    DataContext = _vm;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(
                        "An error occurred while loading specialization data.",
                        "Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error
                        );
                    EventLogger.LogError($"Error loading Specialization ID {SpecializationID}", ex);
                    DialogResult = false;
                    this.Close();
                }
            }
        }
        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            btnClose_Click(sender, e);
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
            {
                MessageBox.Show(
                    "There are Some Missing in The Form!",
                    "Missing Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop
                    );

                return;
            }

            try
            {
                btnSave.IsEnabled = false;
                btnSave.Content = "Please Wait...";

                if (!_isEditMode)
                {

                    // Here we need to create a New instance from Specialization Class
                    Specialization specialization = new Specialization();
                    specialization.SpecializationName = txtName.Text;
                    specialization.Description = txtDescription.Text;
                    specialization.IsActive = chkIsActive.IsChecked == true;
                    specialization.Category = txtCategory.Text;
                    specialization.Level = difficultySelector.SelectedLevelName;
                    specialization.Prerequisites = txtPrerequisites.Text;
                    specialization.Price = Convert.ToDecimal(txtPrice.Text);
                    specialization.Duration = Convert.ToInt32(txtDuration.Text);
                    specialization.CreatedAt = DateTime.Now;
                    specialization.LastUpdatedAt = DateTime.Now;

                    if(await specialization.Save())
                    {
                        MessageBox.Show(
                            "Specialization created successfully!",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        EventLogger.LogInfo("Specialization created successfully!");
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Failed to create Specialization.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        EventLogger.LogWarning("Failed to create Specialization.");
                        btnSave.IsEnabled = true;
                        btnSave.Content = "Create Specialization";
                    }
                }
                else
                {
                    // Update an Existing Specialization
                    _CurrentSpecialization.SpecializationName = txtName.Text;
                    _CurrentSpecialization.Description = txtDescription.Text;
                    _CurrentSpecialization.IsActive = chkIsActive.IsChecked == true;
                    _CurrentSpecialization.Category = txtCategory.Text;
                    _CurrentSpecialization.Level = difficultySelector.SelectedLevelName;
                    _CurrentSpecialization.Prerequisites = txtPrerequisites.Text;
                    _CurrentSpecialization.Price = Convert.ToDecimal(txtPrice.Text);
                    _CurrentSpecialization.Duration = Convert.ToInt32(txtDuration.Text);
                    _CurrentSpecialization.LastUpdatedAt = DateTime.Now;

                    if(await _CurrentSpecialization.Save())
                    {
                        MessageBox.Show(
                            "Specialization updated successfully!",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        EventLogger.LogInfo("Specialization updated successfully!");
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Failed to update Specialization. Please try again.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        EventLogger.LogWarning("Failed to update Specialization. Please try again.");
                        btnSave.IsEnabled = true;
                        btnSave.Content = "Update Specialization";
                    }
                }
            }
            catch(Exception ex) {
                MessageBox.Show(
                    $"Error saving Specialization: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                btnSave.IsEnabled = true;
                btnSave.Content = _isEditMode ? "Update Specialization" : "Create Specialization";
                EventLogger.LogError("An Error Was Occured While Saving the Specialization!!", ex);
                return;
            }
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(txtName.Text) || txtName.Text.Length <= 1)
            { 
                MessageBox.Show(
                    "The Specialization Name is Required",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                isValid = false;
            }

            if (string.IsNullOrEmpty(txtDescription.Text) || txtDescription.Text.Length <= 3)
            {
                MessageBox.Show(
                    "The Description Name is Required",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                isValid = false;
            }

            if (string.IsNullOrEmpty(txtCategory.Text))
            {
                MessageBox.Show(
                    "The Specialization Category is Required",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                isValid = false;
            }

            if (difficultySelector.SelectedDifficulty == UserControles.DificultyLevelSlider.enDifficultyLevel.None)
            {
                MessageBox.Show(
                    "The Specialization Level Must be Selected",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                isValid = false;
            }

            if (!string.IsNullOrEmpty(txtPrice.Text) && Convert.ToDecimal(txtPrice.Text) < 0)
            {
                MessageBox.Show(
                    "The Specialization Price Must be Positive not null!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                isValid = false;
            }

            if (!string.IsNullOrEmpty(txtDuration.Text) && Convert.ToDecimal(txtDuration.Text) < 0)
            {
                MessageBox.Show(
                    "The Specialization Duration must be a Positive not null Value!",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                isValid = false;
            }

            if (chkIsActive.IsChecked == false)
                MessageBox.Show(
                    "Inactive specializations won't be available for new enrollments",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );

            return isValid;
        }

        private void difficultySelector_DifficultyChanged(object sender, UserControles.DificultyLevelSlider.DifficultyChangedEventArgs e)
        {

        }

        private void txtPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 1. Check if the typed character is a number or a decimal point
            Regex regex = new Regex("[^0-9.]+");

            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            // 2. Handle the special case for the decimal point
            if(e.Text == ".")
            {
                var textBox = sender as TextBox;
                // Block The decimal point if one already exists in the text
                if (textBox.Text.Contains("."))
                {
                    e.Handled = true;
                }
            }
        }
        private void txtPrice_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if(!IsTextAllowed(text, isInteger: false))
                {
                    // If the pasted text is not a valid decimal, cancel the paste command
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        private static bool IsTextAllowed(string text, bool isInteger)
        {
            Regex regex;

            if (isInteger)
            {
                // Regex for a valid integer
                regex = new Regex("^[0-9]+$");
            }
            else
            {
                // Regex for a valid decimal
                regex = new Regex("^[0-9]*\\.?[0-9]+$");
            }

            return regex.IsMatch(text);
        }
        private void txtDuration_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 1. Check if the typed character is a number or a decimal point
            Regex regex = new Regex("[^0-9.]+");

            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            // 2. Handle the special case for the decimal point
            if (e.Text == ".")
            {
                var textBox = sender as TextBox;
                // Block The decimal point if one already exists in the text
                if (textBox.Text.Contains("."))
                {
                    e.Handled = true;
                }
            }
        }
        private void txtDuration_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text, isInteger: false))
                {
                    // If the pasted text is not a valid decimal, cancel the paste command
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
