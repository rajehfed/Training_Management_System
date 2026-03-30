using MahApps.Metro.IconPacks;
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
using TrainningCenter_Entities;

namespace TrainningCenter.Views.Windows.Assessments
{
    /// <summary>
    /// Interaction logic for AddEditAssessmentWindow.xaml
    /// </summary>
    public partial class AddEditAssessmentWindow : Window
    {
        #region Fields
        private int _currentStep = 1;
        private enAssessmentType? _selectedType;
        private AssessmentViewModel _assessmentVM;
        private bool _isEditMode = false;

        private List<Specialization> SpecializationsList; // Loaded from database
        #endregion

        #region Constractors
        public AddEditAssessmentWindow()
        {
            InitializeComponent();
            _isEditMode = false;

            _assessmentVM = new AssessmentViewModel();
        }
        public AddEditAssessmentWindow(AssessmentViewModel selectedAssessment)
        {
            InitializeComponent();
            _isEditMode = true;

            _assessmentVM = selectedAssessment;
            this.DataContext = _assessmentVM;
        }
        #endregion

        #region Event Handlers
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to cancel? All entered data will be lost.",
                "Confirm Cancel",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DialogResult = false;
                Close();
            }
        }

        private void TypeCard_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border is null) return;

            // Reset all cards
            ResetTypeCards();

            // Highlight selected card
            border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8147E7"));
            border.BorderThickness = new Thickness(3);
            border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0EDFF"));

            // Parse and store selected type
            string typeString = border.Tag.ToString();
            _selectedType = AssessmentsDTO.StringToEnumMapping(typeString);

            // Update icon and text for step 2
            UpdateSelectedTypeDisplay();
        }

        private void ResetTypeCards()
        {
            var cards = new[] { quizCard, theoryExamCard, practicalExamCard, projectCard, assignmentCard, presentationCard };
            foreach (var card in cards)
            {
                card.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
                card.BorderThickness = new Thickness(2);
                card.Background = Brushes.White;
            }
        }
        private void UpdateSelectedTypeDisplay()
        {
            tbSelectedType.Text = _selectedType.ToString();

            // Update icon based on type
            selectedTypeIcon.Kind = _selectedType switch
            {
                enAssessmentType.Quiz => PackIconMaterialKind.HelpCircleOutline,
                enAssessmentType.TheoryExam => PackIconMaterialKind.BookOpenPageVariant,
                enAssessmentType.PracticalTest => PackIconMaterialKind.CodeTags,
                enAssessmentType.Project => PackIconMaterialKind.FolderMultiple,
                enAssessmentType.Assignment => PackIconMaterialKind.ClipboardText,
                enAssessmentType.Presentation => PackIconMaterialKind.Presentation,
                _ => PackIconMaterialKind.HelpCircleOutline
            };
        }
        private void QuickDuration_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            var totalMinutes = int.Parse(button.Tag.ToString());
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;

            txtHours.Text = hours.ToString();
            txtMinutes.Text = minutes.ToString();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 1)
            {
                _currentStep--;
                UpdateStepUI();
            }
        }
        #endregion

        #region Step Navigation
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCurrentStep())
                return;

            if (_currentStep < 3)
            {
                _currentStep++;
                UpdateStepUI();
            }
            else
            {
                // Final step - create assessment
                SaveAssessmentAsync();
            }
        }

        private void UpdateStepUI()
        {
            // Update visibility
            step1Content.Visibility = _currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
            step2Content.Visibility = _currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
            step3Content.Visibility = _currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;

            // Update step indicators
            UpdateStepIndicator(step2Circle, _currentStep >= 2);
            UpdateStepIndicator(step3Circle, _currentStep >= 3);

            // Update header subtitle
            tbHeaderSubtitle.Text = _currentStep switch
            {
                1 => "Step 1: Choose Assessment Type",
                2 => "Step 2: Enter Basic Information",
                3 => "Step 3: Configure Settings",
                _ => ""
            };

            // Update progress text
            tbProgress.Text = $"Step {_currentStep} of 3";

            // Update button text
            btnBack.Visibility = _currentStep > 1 ? Visibility.Visible : Visibility.Collapsed;
            btnNext.Content = _currentStep == 3 ? (_isEditMode ? "Update Assessment" : "Create Assessment") : "Next →";
        }

        private void UpdateStepIndicator(Border circle, bool isActiveOrCompleted)
        {
            if (isActiveOrCompleted)
            {
                circle.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8147E7"));
                circle.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8147E7"));
                circle.BorderThickness = new Thickness(3);

                var textBlock = circle.Child as TextBlock;
                if (textBlock != null)
                    textBlock.Foreground = Brushes.White;
            }
            else
            {
                circle.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
                circle.BorderBrush = Brushes.Transparent;
                circle.BorderThickness = new Thickness(0);

                var textBlock = circle.Child as TextBlock;
                if (textBlock != null)
                    textBlock.Foreground = Brushes.White;
            }
        }

        #endregion

        #region Validation

        private bool ValidateCurrentStep()
        {
            switch (_currentStep)
            {
                case 1:
                    return ValidateStep1();

                case 2:
                    return ValidateStep2();

                case 3:
                    return ValidateStep3();

                default:
                    return true;
            }
        }
        private bool ValidateStep1()
        {
            if (!_selectedType.HasValue)
            {
                MessageBox.Show(
                    "Please select an assessment type.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
        private bool ValidateStep2()
        {
            // For simplicity, we just check if a duration is selected
            if (string.IsNullOrWhiteSpace(txtAssessmentName.Text))
            {
                MessageBox.Show(
                    "Please enter an assessment name.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtAssessmentName.Focus();
                return false;
            }
            return true;
        }
        private bool ValidateStep3()
        {
            // Validate passing marks
            if (!decimal.TryParse(txtPassingMarks.Text, out decimal passingMarks) || passingMarks <= 0)
            {
                MessageBox.Show(
                    "Please enter a valid passing marks value (greater than 0).",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtPassingMarks.Focus();
                return false;
            }
            // Validate total marks
            if (!decimal.TryParse(txtTotalMarks.Text, out decimal totalMarks) || totalMarks <= 0)
            {
                MessageBox.Show(
                    "Please enter a valid total marks value (greater than 0).",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtPassingMarks.Focus();
                return false;
            }

            // Check passing marks <= total marks
            if (passingMarks > totalMarks)
            {
                MessageBox.Show(
                    "Passing marks cannot be greater than total marks.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtPassingMarks.Focus();
                return false;
            }

            // Validate duration
            if (!int.TryParse(txtHours.Text, out int hours) || hours < 0)
                hours = 0;

            if (!int.TryParse(txtMinutes.Text, out int minutes) || minutes < 0)
                minutes = 0;

            var totalDuration = (hours * 60) + minutes;
            if (totalDuration <= 0)
            {
                MessageBox.Show(
                    "Please enter a valid duration (must be greater than 0).",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtMinutes.Focus();
                return false;
            }

            // Update passing percentage display
            var passPercentage = (passingMarks / totalMarks) * 100;
            runPassPercentage.Text = $"{passPercentage:F1}%";

            return true;
        }
        #endregion

        #region Create Assessment

        private async void SaveAssessmentAsync()
        {
            try
            {
                // Parse duration
                int hours = int.TryParse(txtHours.Text, out int h) ? h : 0;
                int minutes = int.TryParse(txtMinutes.Text, out int m) ? m : 0;
                int totalMinutes = (hours * 60) + minutes;

                //if(_isEditMode)
                //    // Create assessment object
                //    _assessmentVM = new AssessmentViewModel
                //    {
                //        AssessmentID = _isEditMode ? _assessmentVM.AssessmentID : null,
                //        AssessmentName = txtAssessmentName.Text.Trim(),
                //        AssessmentType = _selectedType,
                //        SpecializationID = (int)cmbSpecialization.SelectedValue, // TODO: Get actual ID from combo
                //        TotalMarks = decimal.Parse(txtTotalMarks.Text),
                //        PassingMark = decimal.Parse(txtPassingMarks.Text),
                //        Duration = TimeSpan.FromMinutes(totalMinutes),
                //        Instructions = txtInstructions.Text.Trim(),
                //        IsActive = chkIsActive.IsChecked ?? true,
                //        CreatedByUserID = Globals.CurrentUser.UserId, // TODO: Get from current logged-in user
                //        CreatedAt = DateTime.Now
                //    };
                //else
                //{
                //    Assessment assessment = await Assessment.Find(_assessmentVM.AssessmentID.Value);
                //}

                _assessmentVM.AssessmentName = txtAssessmentName.Text.Trim();
                _assessmentVM.AssessmentType = _selectedType;
                _assessmentVM.SpecializationID = (int)cmbSpecialization.SelectedValue;
                _assessmentVM.TotalMarks = decimal.Parse(txtTotalMarks.Text);
                _assessmentVM.PassingMark = decimal.Parse(txtPassingMarks.Text);
                _assessmentVM.Duration = TimeSpan.FromMinutes(totalMinutes);
                _assessmentVM.Instructions = txtInstructions.Text.Trim();
                _assessmentVM.IsActive = chkIsActive.IsChecked ?? true;

                var result = await _assessmentVM.SaveAsync();

                if (result.success)
                {
                    // Use _isEditMode to show the correct message
                    string action = _isEditMode ? "updated" : "created";

                    MessageBox.Show(
                        $"Assessment '{_assessmentVM.AssessmentName}' {action} successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(
                        $"Failed to save assessment: {result.message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error creating assessment: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                EventLogger.LogError($"Error creating assessment:", ex);
            }
        }
        #endregion

        #region Loaded Specializations
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SpecializationsList = await Specialization.GetAllSpecializations();
                cmbSpecialization.ItemsSource = SpecializationsList;
                cmbSpecialization.SelectedValuePath = "SpecializationID";
                if (_isEditMode)
                {
                    // Pre-select specialization in edit mode
                    var selectedSpec = SpecializationsList.FirstOrDefault(s => s.SpecializationID == _assessmentVM.SpecializationID);
                    if (selectedSpec != null)
                        cmbSpecialization.SelectedItem = selectedSpec;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading specializations: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                EventLogger.LogError($"Error loading specializations: {ex.Message}");
            }
        }
        #endregion
    }
}
