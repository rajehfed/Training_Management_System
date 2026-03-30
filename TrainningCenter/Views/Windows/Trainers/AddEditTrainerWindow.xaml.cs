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
using System.Windows.Shapes;
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter.ViewModel;

namespace TrainningCenter.Views.Windows.Trainers
{
    /// <summary>
    /// Interaction logic for AddEditTrainerWindow.xaml
    /// </summary>
    public partial class AddEditTrainerWindow : Window
    {
        private bool _isEditMode = false;
        private TrainerVM _Trainer = null;
        private ObservableCollection<CenterVM> _allCenters;
        #region Constractors
        public AddEditTrainerWindow()
        {
            InitializeComponent();

            _Trainer = new TrainerVM();
            this.DataContext = _Trainer;

            tbTitle.Text = "Add New Trainer";
            LoadCenters();
        }
        public AddEditTrainerWindow(TrainerVM trainer)
        {
            InitializeComponent();

            _isEditMode = true;
            _Trainer = trainer;

            tbTitle.Text = "Edit Trainer";
            tbSubtitle.Text = $"Edit details of {_Trainer.FullName}";

            btnDelete.Visibility = Visibility.Visible;

            this.DataContext = _Trainer;

            LoadCenters();

            this.Loaded += async (s, e) =>
            {
                if (personSelector != null && _Trainer.PersonID > 0)
                {
                    await personSelector.SetEditMode(trainer.PersonID);
                }
                else if (personSelector != null)
                {
                    // Handle case where PersonID is invalid
                    MessageBox.Show(
                        "Cannot load person information. Person data is missing.",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            };
        }
        #endregion

        #region Event Handlers
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void sliderRating_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Update the text block to show value
            if (tbRating != null)
            {
                tbRating.Text = e.NewValue.ToString("0.0");

                // Optional: Update color based on score
                if (e.NewValue >= 4) tbRating.Foreground = (Brush)FindResource("SuccessBrush");
                else if (e.NewValue >= 2.5) tbRating.Foreground = (Brush)FindResource("WarningBrush");
                else tbRating.Foreground = (Brush)FindResource("DangerBrush");
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to delete this trainer?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = await _Trainer.DeleteAsync();

                    if (success)
                    {
                        MessageBox.Show(
                            "Trainer deleted successfully.",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Failed to delete trainer.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error deleting trainer: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    EventLogger.LogError($"Error Deleting Trainer ID: {_Trainer.TrainerID}. Exception: {ex}");
                }
            }
        }

        private async void personSelector_PersonSelectionChanged(object sender, UserControles.PersonSelector.PersonSelectedEventArgs e)
        {
            if (e.SelectedPerson != null)
            {
                _Trainer.PersonID = e.SelectedPerson.PersonId;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!(await ValidateForm())) return;

            try
            {
                // 1. Sync UserControl Selection
                if (personSelector.CurrentPersonID.HasValue)
                {
                    _Trainer.PersonID = personSelector.CurrentPersonID.Value;

                    // 2. Sync Manual Fields (if needed)
                    // Note: Most fields are bound via XAML {Binding ...} so this step is minimal.
                    // Ensure Rating slider updates the VM
                    _Trainer.Rating = (decimal)sliderRating.Value;

                    // 3. Save via ViewModel
                    bool success = await _Trainer.SaveAsync();

                    if (success)
                    {
                        string msg = _isEditMode ? "Trainer updated successfully." : "Trainer added successfully.";
                        MessageBox.Show(msg, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Failed to save trainer.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                EventLogger.LogError($"Error Saving Trainer ID: {_Trainer.TrainerID}. Exception: {ex}");
            }
        }
        #endregion

        #region Helpers

        private async Task<bool> ValidateForm()
        {
            bool isValid = true;

            // Reset Errors
            txtSpecialization.BorderBrush = (Brush)FindResource("BorderBrush");
            txtExperienceYears.BorderBrush = (Brush)FindResource("BorderBrush");
            txtSalary.BorderBrush = (Brush)FindResource("BorderBrush");
            errorSpecialization.Visibility = Visibility.Collapsed;
            errorExperience.Visibility = Visibility.Collapsed;
            errorSalary.Visibility = Visibility.Collapsed;

            // 1. Check Person
            if (personSelector.CurrentPersonID == null)
            {
                MessageBox.Show("Please select a person.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!_isEditMode)
            {
                if (await Trainer.IsTrainerExists(personSelector.CurrentPersonID.Value))
                {
                    MessageBox.Show(
                        "This Person Already a Teacher Please Select an other person",
                        "Validation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return false;
                }
            }

            // 2. Specialization
            if (string.IsNullOrWhiteSpace(txtSpecialization.Text))
            {
                errorSpecialization.Visibility = Visibility.Visible;
                txtSpecialization.BorderBrush = Brushes.Red;
                isValid = false;
            }

            // 3. Experience (Must be int >= 0)
            if (!int.TryParse(txtExperienceYears.Text, out int exp) || exp < 0)
            {
                errorExperience.Visibility = Visibility.Visible;
                txtExperienceYears.BorderBrush = Brushes.Red;
                isValid = false;
            }

            // 4. Salary (Must be decimal >= 0)
            if (!decimal.TryParse(txtSalary.Text, out decimal salary) || salary < 0)
            {
                errorSalary.Visibility = Visibility.Visible;
                txtSalary.BorderBrush = Brushes.Red;
                isValid = false;
            }

            // 5. Hire Date
            if (dpHireDate.SelectedDate == null)
            {
                errorHireDate.Visibility = Visibility.Visible;
                isValid = false;
            }

            return isValid;
        }
        private async void LoadCenters()
        {
            try
            {
                var centers = await Center.GetAllCenters();

                // Create collection
                _allCenters = new ObservableCollection<CenterVM>();

                foreach (var center in centers.Where(c => c.IsActive))
                {
                    _allCenters.Add(new CenterVM(center));
                }

                cmbCenter.ItemsSource = _allCenters;
                cmbCenter.DisplayMemberPath = "CenterName";
                cmbCenter.SelectedValuePath = "CenterID";

                // Set selection
                if (_Trainer.CenterID.HasValue && _Trainer.CenterID.Value > 0)
                {
                    cmbCenter.SelectedValue = _Trainer.CenterID.Value;
                }
                else
                {
                    cmbCenter.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading centers: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

    }
}
