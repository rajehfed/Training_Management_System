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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static TrainningCenter.UserControles.DificultyLevelSlider;

namespace TrainningCenter.UserControles
{
    /// <summary>
    /// Difficulty Level Selector UserControl
    /// 
    /// Provides a visual way to select specialization difficulty:
    /// 1 = Beginner (Green)
    /// 2 = Medium (Orange)
    /// 3 = Hard (Red)
    /// </summary>
    public partial class DificultyLevelSlider : UserControl
    {
        /// <summary>
        /// Difficulty level enumeration for type safety
        /// </summary>
        public enum enDifficultyLevel
        {
            None = 0,
            Beginner = 1,
            Medium = 2,
            Hard = 3
        }

        // Event Raised When difficulty Level Changed
        public event EventHandler<DifficultyChangedEventArgs> DifficultyChanged;
        public class DifficultyChangedEventArgs : EventArgs
        {
            public int Level { get; set; }
            public enDifficultyLevel DifficultyLevel { get; set; }
            public string LevelName { get; set; }
        }

        public DificultyLevelSlider()
        {
            InitializeComponent();

            SelectedLevel = 1;
        }

        /// <summary>
        /// Gets or sets the selected difficulty level (1, 2, or 3)
        /// </summary>
        public int SelectedLevel
        {
            get
            {
                if (rbBeginner.IsChecked == true) return 1;
                if (rbMedium.IsChecked == true) return 2;
                if (rbHard.IsChecked == true) return 3;
                return 0;
            }
            set
            {
                switch (value)
                {
                    case 1:
                        rbBeginner.IsChecked = true;
                        UpdateDescription(enDifficultyLevel.Beginner);
                        break;

                    case 2:
                        rbMedium.IsChecked = true;
                        UpdateDescription(enDifficultyLevel.Medium);
                        break;

                    case 3:
                        rbHard.IsChecked = true;
                        UpdateDescription(enDifficultyLevel.Hard);
                        break;

                    default:
                        // Clear all sections
                        rbBeginner.IsChecked = false;
                        rbMedium.IsChecked = false;
                        rbHard.IsChecked = false;
                        pnlDescription.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the selected difficulty as enum
        /// </summary>
        public enDifficultyLevel SelectedDifficulty
        {
            get
            {
                return (enDifficultyLevel)SelectedLevel;
            }
            set
            {
                SelectedLevel = (int)value;
            }
        }

        /// <summary>
        /// Gets the selected difficulty name as string
        /// </summary>
        public string SelectedLevelName
        {
            get
            {
                switch (SelectedLevel)
                {
                    case 1: return "Beginner";
                    case 2: return "Intermediate";
                    case 3: return "Advanced";
                    default: return null;
                }
            }
        }

        /// <summary>
        /// Event handler when any level is selected
        /// </summary>
        private void LevelChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            var level = SelectedLevel;
            var difficulty = (enDifficultyLevel)level;

            UpdateDescription(difficulty);
            UpdateLevel(difficulty);

            // Raise event to notify parent
            DifficultyChanged?.Invoke(this, new DifficultyChangedEventArgs
            {
                Level = level,
                DifficultyLevel = difficulty,
                LevelName = SelectedLevelName
            });
        }

        /// <summary>
        /// Updates the description text based on selected level
        /// </summary>
        private void UpdateDescription(enDifficultyLevel level)
        {
            pnlDescription.Visibility = Visibility.Visible;

            switch (level)
            {
                case enDifficultyLevel.Beginner:
                    txtDescription.Text = "Perfect for newcomers with no prior experience. Basic concepts and fundamentals.";
                    break;

                case enDifficultyLevel.Medium:
                    txtDescription.Text = "For students with some background knowledge. Intermediate concepts and practical skills.";
                    break;

                case enDifficultyLevel.Hard:
                    txtDescription.Text = "Advanced level requiring strong foundational knowledge. Complex topics and expert techniques.";
                    break;

                default:
                    pnlDescription.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        /// <summary>
        /// Updates the Active button text based on selected level
        /// </summary>
        private void UpdateLevel(enDifficultyLevel level)
        {
            switch (level)
            {
                case enDifficultyLevel.Beginner:
                    rbBeginner.IsChecked = true;
                    rbMedium.IsChecked = false;
                    rbHard.IsChecked = false;
                    break;

                case enDifficultyLevel.Medium:
                    rbBeginner.IsChecked = false;
                    rbMedium.IsChecked = true;
                    rbHard.IsChecked = false;
                    break;

                case enDifficultyLevel.Hard:
                    rbBeginner.IsChecked = false;
                    rbMedium.IsChecked = false;
                    rbHard.IsChecked = true;
                    txtDescription.Text = "Advanced level requiring strong foundational knowledge. Complex topics and expert techniques.";
                    break;

                default:
                    pnlDescription.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Validates that a level is selected
        /// </summary>
        public bool ValidateSelection()
        {
            if (SelectedLevel == 0)
            {
                MessageBox.Show(
                    "Please select a difficulty level.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Resets the control to default (Beginner)
        /// </summary>
        public void Reset()
        {
            SelectedLevel = 1;
        }

        /// <summary>
        /// Enables or disables the control
        /// </summary>
        public void SetEnable(bool enabled)
        {
            rbBeginner.IsEnabled = enabled;
            rbMedium.IsEnabled = enabled;
            rbHard.IsEnabled = enabled;
        }
    }
}
