using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace TrainningCenter.Views.Windows
{
    /// <summary>
    /// Interaction logic for AddEditStudent.xaml
    /// </summary>
    public partial class AddEditStudent : Window
    {
        private bool _isEditMode;
        private StudentVM _vm;
        private Student _student = null;

        #region Constructors
        public AddEditStudent()
        {
            InitializeComponent();
            _vm = new StudentVM();
            this.DataContext = _vm;

            tbTitle.Text = "Add New Student";
            btnDelete.Visibility = Visibility.Collapsed;
            _isEditMode = false;
        }
        public AddEditStudent(Student student)
        {
            InitializeComponent();

            // 1. Basic UI Setup (Fast and Sync)
            _isEditMode = true;
            tbTitle.Text = "Edit Student";
            tbSubtitle.Text = $"Editing details for {student.StudentName}";
            btnDelete.Visibility = Visibility.Visible;

            // 2. Store the student to load it later
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _student = student;
                _vm = new StudentVM(student);
                this.DataContext = _vm;
            }
        }

        #endregion

        #region Event Handlers
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to delete this student?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    // FIX: Actually call the delete method!
                    bool success = await _vm.DeleteAsync();

                    if (success)
                    {
                        MessageBox.Show(
                            "Student deleted successfully.",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        EventLogger.LogInfo($"Student Deleted. ID: {_vm.StudentID}");
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Failed to delete student from database.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "An error occurred while deleting the student.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    EventLogger.LogError($"Error Deleting Student ID: {_vm.StudentID}. Exception: {ex}");
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            btnClose_Click(sender, e);
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            try
            {
                _vm.PersonID = personSelector.CurrentPersonID.Value;

                if (cmbStatus.SelectedItem is ComboBoxItem selectedItem)
                {
                    _vm.StudentStatus = selectedItem.Content.ToString();
                }

                bool success = await _vm.SaveAsync();

                if (success)
                {
                    string msg = _isEditMode ? "Student updated successfully." : "Student added successfully.";
                    MessageBox.Show(msg, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    EventLogger.LogInfo($"Student Saved. ID: {_vm.StudentID}");
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save student.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while saving the student data.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                EventLogger.LogError($"Error Saving Student Data. Exception: {ex}");
                return;
            }
        }

        #endregion

        #region Helpers Methods

        private async Task LoadStudentData(int? studentID)
        {
            if (studentID is null)
            {
                MessageBox.Show(
                    "Invalid Student ID.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }


            if (_isEditMode)
            {
                try
                {
                    _student = await Student.Find(studentID.Value);
                    if (_student == null)
                    {
                        MessageBox.Show(
                            "Student not found.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        EventLogger.LogWarning($"Cannot Find Student With ID: {studentID}");
                        DialogResult = false;
                        this.Close();
                        return;
                    }

                    _vm = new StudentVM(_student);
                    await personSelector.SetEditMode(_student.PersonInfo.Id);
                    //personSelector.pnlPersonInfo.Visibility = Visibility.Visible;
                    this.DataContext = _vm;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "An error occurred while loading student data.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    EventLogger.LogError($"Error Loading Student With ID: {studentID}. Exception: {ex}");
                    DialogResult = false;
                    this.Close();
                    return;
                }
            }
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            // Reset Styles
            txtStudentNumber.BorderBrush = (Brush)FindResource("BorderBrush"); // Reset to default
            dpAdmissionDate.BorderBrush = (Brush)FindResource("BorderBrush");

            // 1. Check Person
            if (personSelector.CurrentPersonID == null)
            {
                MessageBox.Show("Please select a person.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 2. Check Number
            if (string.IsNullOrWhiteSpace(txtStudentNumber.Text))
            {
                // Error message style is better handled via the TextBlock 'errorStudentNumber' visibility
                errorStudentNumber.Visibility = Visibility.Visible;
                txtStudentNumber.BorderBrush = Brushes.Red;
                isValid = false;
            }
            else
            {
                errorStudentNumber.Visibility = Visibility.Collapsed;
            }

            // 3. Check Date
            if (dpAdmissionDate.SelectedDate == null)
            {
                errorAdmissionDate.Visibility = Visibility.Visible;
                dpAdmissionDate.BorderBrush = Brushes.Red;
                isValid = false;
            }
            else
            {
                errorAdmissionDate.Visibility = Visibility.Collapsed;
            }

            return isValid;
        }
        #endregion

        private void personSelector_PersonSelectionChanged(object sender, UserControles.PersonSelector.PersonSelectedEventArgs e)
        {
            if (!(e.SelectedPerson is null))
            {
                _vm.PersonID = e.SelectedPerson.PersonId;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isEditMode && _student != null)
            {
                try
                {
                    // Now we can properly AWAIT the async method!
                    // This ensures the data is fully loaded and the UI is updated correctly.
                    await LoadStudentData(_student.StudentID);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading person data: {ex.Message}");
                }
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
