using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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

namespace TrainningCenter.Views.Windows
{
    public partial class AddUpdatePersonWindow : Window
    {
        private enum enMode { eAddNew = 0, eUpdate = 1 }
        private enMode _CurrentMode = enMode.eAddNew;
        private int _personID = -1;
        private string _imagePath = null;

        public AddUpdatePersonWindow()
        {
            InitializeComponent();

            _CurrentMode = enMode.eAddNew;
            SetupWindow();
        }

        public AddUpdatePersonWindow(int PersonID)
        {
            InitializeComponent();

            _CurrentMode = enMode.eUpdate;
            _personID = PersonID;
            SetupWindow();
            Loaded += async (_, __) => await _LoadPersonData(PersonID);
        }

        private void SetupWindow()
        {
            // Set Window Title
            if (_CurrentMode == enMode.eAddNew)
            {
                Title = "Add New Person";
                tbWindowTitle.Text = "Add New Person";
            }
            else
            {
                Title = "Update Person";
                tbWindowTitle.Text = "Update Person";
                btnSave.Content = CreateSaveButtonContent("Update Person");
            }

            // Set default date
            dpDateOfBirth.SelectedDate = DateTime.Now.AddYears(-18);
        }

        private UIElement CreateSaveButtonContent(string text) // for dynamically creating a new content of the save button based on the mode!
        {
            var panel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal
            };

            var icon = new MahApps.Metro.IconPacks.PackIconMaterial
            {
                Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.ContentSave,
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 0, 8, 0)
            };

            var textBlock = new System.Windows.Controls.TextBlock
            {
                Text = text
            };

            panel.Children.Add(icon);
            panel.Children.Add(textBlock);

            return panel;
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            // Reset all error messages
            HideAllErrors();

            // Validate First Name
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                tbFirstNameError.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Validate Last Name
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                tbLastNameError.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Validate National Number
            if (string.IsNullOrWhiteSpace(txtNationalNo.Text))
            {
                tbNationalNoError.Visibility = Visibility.Visible;
                tbNationalNoError.Text = "National number is required";
                isValid = false;
            }
            else if (txtNationalNo.Text.Length < 2)
            {
                tbNationalNoError.Visibility = Visibility.Visible;
                tbNationalNoError.Text = "National number must be at least 2 characters";
                isValid = false;
            }

            // Validate Date of Birth
            if (!dpDateOfBirth.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a date of birth.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }
            else if (dpDateOfBirth.SelectedDate.Value > DateTime.Now.AddYears(-5))
            {
                MessageBox.Show("Person must be at least 5 years old.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }

            // Validate Phone Number
            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text))
            {
                tbPhoneError.Visibility = Visibility.Visible;
                tbPhoneError.Text = "Phone number is required";
                isValid = false;
            }
            else if (!IsValidPhone(txtPhoneNumber.Text))
            {
                tbPhoneError.Visibility = Visibility.Visible;
                tbPhoneError.Text = "Invalid phone number format";
                isValid = false;
            }

            // Validate Email (optional but must be valid if provided)
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
            {
                tbEmailError.Visibility = Visibility.Visible;
                isValid = false;
            }

            return isValid;
        }

        private void HideAllErrors()
        {
            tbFirstNameError.Visibility = Visibility.Collapsed;
            tbLastNameError.Visibility = Visibility.Collapsed;
            tbNationalNoError.Visibility = Visibility.Collapsed;
            tbPhoneError.Visibility = Visibility.Collapsed;
            tbEmailError.Visibility = Visibility.Collapsed;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Simple Email regex pattern
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                EventLogger.LogWarning("Email does not match the normes");
                return false;
            }
        }
        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Remove common phone number characters
            string CleanPhone =
                phone.Replace(" ", "")
                    .Replace("-", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("+", "");

            // Check if it contains only digits and is at least 10 digits
            return CleanPhone.Length >= 10 && Regex.IsMatch(CleanPhone, @"^\d+$");
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnBrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*",
                Title = "Select Profile Image",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _imagePath = openFileDialog.FileName;
                    txtImagePath.Text = _imagePath;
                    LoadImage(_imagePath);
                    btnRemoveImage.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    EventLogger.LogError("Error loading image", ex);
                    MessageBox.Show(
                        $"Error loading image: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to cancel? Any unsaved changes will be lost.",
                "Confirm Cancel",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
            {
                MessageBox.Show(
                    "Please fill in all required fields correctly.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Show loading
                LoadingOverlay.Visibility = Visibility.Visible;
                Person _person = null;

                if(_CurrentMode == enMode.eAddNew)
                {
                    _person = new Person();
                }
                else
                {
                    _person = await Person.Find(_personID);
                    if (_person == null)
                    {
                        MessageBox.Show($"Person with ID {_personID} not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Fill person Object
                _person.FirstName = txtFirstName.Text.Trim();
                _person.LastName = txtLastName.Text.Trim();
                _person.NationalNo = txtNationalNo.Text.Trim();
                _person.DateOfBirth = dpDateOfBirth.SelectedDate.Value;
                _person.Gender = (cbGender.SelectedItem as ComboBoxItem).Tag.ToString() == "M"
                    ? Person.enGender.Male : Person.enGender.Female;
                _person.Nationality = cbNationality.Text.Trim();
                _person.PhoneNumber = txtPhoneNumber.Text.Trim();
                _person.Email = txtEmail.Text.Trim();
                _person.Address = txtAddress.Text.Trim();

                if (!string.IsNullOrEmpty(_imagePath))
                {
                    // in case of update we delete the old one and save the new one
                    if (_CurrentMode == enMode.eUpdate && !string.IsNullOrEmpty(_person.ImagePath))
                    {
                        _person.ImagePath = ImageHelper.UpdateImage(
                            ImageHelper.GetFullImagePath(_person.ImagePath),
                            txtImagePath.Text
                        );
                    }
                    else
                        _person.ImagePath = ImageHelper.SaveImage(txtImagePath.Text);
                }

                // Save to database
                bool seccess = await _person.Save();

                // Hide loading
                LoadingOverlay.Visibility = Visibility.Collapsed;

                if (seccess)
                {
                    string message = _CurrentMode == enMode.eUpdate ? "Person updated successfully!" : "Person added successfully!";
                    EventLogger.LogInfo(message);
                    MessageBox.Show(
                        message,
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    EventLogger.LogWarning("Failed to save person. Please try again.");
                    MessageBox.Show(
                        "Failed to save person. Please try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );
                }
            }
            catch (Exception ex)
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                EventLogger.LogError("Failed to save person Due to an Error", ex);
                MessageBox.Show(
                    $"Error: {ex.Message},",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
            }
        }

        private async Task _LoadPersonData(int personID)
        {
            try
            {
                var person = await Person.Find(personID);

                txtFirstName.Text = person.FirstName;
                txtLastName.Text = person.LastName;
                txtNationalNo.Text = person.NationalNo;
                dpDateOfBirth.SelectedDate = person.DateOfBirth;
                cbGender.SelectedIndex = person.Gender == (Person.enGender.Male) ? 0 : 1;
                cbNationality.Text = person.Nationality;
                txtPhoneNumber.Text = person.PhoneNumber;
                txtEmail.Text = person.Email;
                txtAddress.Text = person.Address;
                txtImagePath.Text = ImageHelper.GetFullImagePath(person.ImagePath);

                if (!string.IsNullOrEmpty(txtImagePath.Text))
                    LoadImage(txtImagePath.Text);

                EventLogger.LogInfo("Person Data Was Retrieved Successfully!");
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Error loading person data", ex);
                MessageBox.Show(
                    $"Error loading person data: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void LoadImage(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    imgPreview.Source = bitmap;
                    iconPlaceHolder.Visibility = Visibility.Collapsed;
                    btnRemoveImage.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    EventLogger.LogError("Error loading image", ex);
                    MessageBox.Show($"Error loading image: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    iconPlaceHolder.Visibility = Visibility.Visible;
                }
            }
            else
            {
                iconPlaceHolder.Visibility = Visibility.Visible;
            }
        }

        private void btnRemoveImage_Click(object sender, RoutedEventArgs e)
        {
            _imagePath = null;
            txtImagePath.Text = string.Empty;
            imgPreview.Source = null;
            iconPlaceHolder.Visibility = Visibility.Visible;
            btnRemoveImage.Visibility = Visibility.Collapsed;
        }
    }
}
