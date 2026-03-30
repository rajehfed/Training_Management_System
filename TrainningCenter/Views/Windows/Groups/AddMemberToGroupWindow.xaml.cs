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
using TrainningCenter.UserControles.Students;
using TrainningCenter.ViewModel;

namespace TrainningCenter.Views.Windows.Groups
{
    /// <summary>
    /// Interaction logic for AddMemberToGroupWindow.xaml
    /// </summary>
    public partial class AddMemberToGroupWindow : Window
    {
        #region Properties & Constractor

        private int _currentStep = 1;
        private int _groupID;
        private string _groupName;
        private decimal _coursePrice;

        // Selected Data
        private StudentVM _selectedStudent;
        private string _selectedPlan = "Full"; // Default

        public AddMemberToGroupWindow(int groupID, string groupName, decimal groupFee)
        {
            InitializeComponent();

            _groupID = groupID;
            _groupName = groupName;
            _coursePrice = groupFee;

            tbGroupName.Text = _groupName;
            tbSummaryGroup.Text = _groupName;

            // Set Pricing (Hardcoded for now, but I should fetch it from Group/Specialization)
            runFullAmount.Text = _coursePrice.ToString("N0");
            runInstallmentAmount.Text = (_coursePrice / 3 * 1.2m).ToString("0");

            // Subscribe to Student Selector Event
            studentSelector.StudentSelected += studentSelector_StudentSelected;

            UpdateUI();
        }

        #endregion

        #region Event Handlers

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void studentSelector_StudentSelected(object sender, UserControles.Students.StudentSelectedEventArgs e)
        {
            _selectedStudent = e.SelectedStudent;

            if(_selectedStudent != null)
            {
                runSelectedStudent.Text = _selectedStudent.FullName;
            }
        }

        private void txtSearchStudent_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void planFullCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is Border card)
            {
                _selectedPlan = card.Tag.ToString();

                // Visual Selection Logic (Hightlight selected Card)
                HighlightPlan(card);
            }
        }

        private void HighlightPlan(Border selectedCard)
        {
            // Reset both cards (Visual Reset)
            planFullCard.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#E0E0E0");
            planFullCard.BorderThickness = new Thickness(2);
            planFullCard.Background = Brushes.White;

            planInstallmentCard.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#E0E0E0");
            planInstallmentCard.BorderThickness = new Thickness(2);
            planInstallmentCard.Background = Brushes.White;

            // Highlight Selected
            selectedCard.BorderBrush = (Brush)FindResource("PrimaryBrush");
            selectedCard.BorderThickness = new Thickness(3);
            selectedCard.Background = (Brush)FindResource("PrimaryUltraLightBrush");
        }


        #endregion

        #region Navigation Logic
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 1)
            {
                _currentStep--;
                UpdateUI();
            }
        }
        private async void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if(_currentStep == 1)
            {
                if(_selectedStudent is null)
                {
                    MessageBox.Show(
                        "Please select a student first.",
                        "Validation", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);

                    return;
                }

                if(await Member.IsEnrolled(_selectedStudent.StudentID, _groupID))
                {
                    MessageBox.Show(
                        "This student is already enrolled in this group.",
                        "Duplicate",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }
            }

            if(_currentStep == 3)
            {
                // Finish / Save
                PerformEnrollment();
                return;
            }

            _currentStep++;
            UpdateUI();
        }

        private void UpdateUI()
        {
            studentSelector.Visibility = _currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
            step2Content.Visibility = _currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
            step3Content.Visibility = _currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;

            // 2. Buttons
            btnBack.Visibility = _currentStep > 1 ? Visibility.Visible : Visibility.Collapsed;
            btnNext.Content = _currentStep == 3 ? "Confirm Enrollment" : "Next →";

            // 3. Progress Text
            tbProgress.Text = $"Step {_currentStep} of 3";

            // ============================================================
            // 4. NEW: Update Step Indicators (Colors)
            // ============================================================
            UpdateStepVisuals();

            // 5. Update Summery Data (If on Step 3)
            if (_currentStep == 3)
            {
                tbSummaryStudent.Text = $"{_selectedStudent.FullName} ({_selectedStudent.StudentNumber})";
                tbSummaryPlan.Text = _selectedPlan == "Full" ? "Full Payment" : "Installment Plan";
                tbSummaryDate.Text = DateTime.Now.ToString("MMMM dd, yyyy");
            }
        }

        private void UpdateStepVisuals()
        {
            // Define colors
            var activeBrush = (Brush)FindResource("PrimaryBrush"); // Purple
            var doneBrush = (Brush)FindResource("SuccessBrush");   // Green
            var inactiveBrush = (Brush)new BrushConverter().ConvertFrom("#E0E0E0"); // Gray

            // STEP 1 Logic
            if (_currentStep == 1) step1Border.Background = activeBrush;
            else step1Border.Background = doneBrush; // If step > 1, it's done

            // STEP 2 Logic
            if (_currentStep == 2) step2Border.Background = activeBrush;
            else if (_currentStep > 2) step2Border.Background = doneBrush;
            else step2Border.Background = inactiveBrush;

            // STEP 3 Logic
            if (_currentStep == 3) step3Border.Background = activeBrush;
            else step3Border.Background = inactiveBrush;
        }
        #endregion

        #region Save Logic

        private async void PerformEnrollment()
        {
            try
            {
                MemberVM newMember = new MemberVM();

                newMember.StudentID = _selectedStudent.StudentID;
                newMember.GroupID = _groupID;
                newMember.DateOfSubscription = DateTime.Now;
                newMember.Status = "Active";
                newMember.IsActive = true;

                if (_selectedPlan == "Full")
                {
                    newMember.Amount = _coursePrice;
                }
                else
                {
                    newMember.Amount = 0;
                }

                bool success = await newMember.SaveAsync();

                if (success)
                {
                    MessageBox.Show(
                        "Student successfully enrolled!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        "Enrollment failed.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                // This catches the specific SQL Errors we wrote (e.g. "Group is full")
                if (ex.Message.Contains("Group is full"))
                {
                    MessageBox.Show(
                        "Cannot enroll: The Group is Full.", 
                        "Capacity Reached", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                }
                else if (ex.Message.Contains("already subscribed"))
                {
                    MessageBox.Show(
                        "This student is already in this group.", 
                        "Duplicate", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show(
                        $"Error: {ex.Message}", 
                        "Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                }
            }
        }

        #endregion
    }
}
