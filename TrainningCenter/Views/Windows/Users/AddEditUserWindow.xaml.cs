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

namespace TrainningCenter.Views.Windows
{
    public partial class AddEditUserWindow : Window
    {
        private bool isEditMode = false;
        private User _CurrentUser = null;
        public AddEditUserWindow()
        {
            InitializeComponent();
            isEditMode = false;
            tbModalTitle.Text = "Add New User";
            btnSave.Content = "Create User";
        }

        public AddEditUserWindow(int UserID)
        {
            InitializeComponent();
            isEditMode = true;

            tbModalTitle.Text = "Edit User";
            btnSave.Content = "Update User";
            txtPasswordHint.Visibility = Visibility.Visible;

            if (!DesignerProperties.GetIsInDesignMode(this)) {
                _LoadUserInfo(UserID);
            }
        }

        private async void _LoadUserInfo(int UserID)
        {
            try
            {
                _CurrentUser = await User.Find(UserID);
                if (_CurrentUser == null)
                {
                    MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    EventLogger.LogWarning($"Cannot Find User With ID: {UserID}");
                    DialogResult = false;
                    Close();
                    return;
                }

                txtUsername.Text = _CurrentUser.Username;
                cmbRole.SelectedIndex = (int)_CurrentUser.Role;
                chkIsActive.IsChecked = _CurrentUser.IsActive;
                await personSelector.SetEditMode(_CurrentUser.PersonId);
            }
            catch(Exception ex)
            {
                EventLogger.LogError("Error loading User data", ex);
                MessageBox.Show(
                    $"Error loading User data: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        public bool ValidateInputs()
        {
            if (!personSelector.ValidateSelection())
            {
                return false;
            }

            // Validate Username
            if (string.IsNullOrEmpty(txtUsername.Text)) {
                MessageBox.Show(
                    "Please enter a username.",
                    "Validation Error",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                EventLogger.LogWarning($"Invalid username: {txtUsername.Text}");
                txtUsername.Focus();
                return false;
            }

            // Validate Password
            if(txtUsername.Text.Length <= 3)
            {
                MessageBox.Show(
                    "Username must be at least 3 characters long.", 
                    "Validation Error",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                EventLogger.LogWarning("Username must be at least 3 characters long.");
                txtUsername.Focus();
                return false;
            }

            // Validate Password (only for Add Mode)
            if(!isEditMode && string.IsNullOrEmpty(txtPassword.Password))
            {
                MessageBox.Show("Please enter a password.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Focus();
                return false;
            }
            if (!isEditMode && txtPassword.Password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Focus();
                return false;
            }

            // Validate UserRole
            if (cmbRole.SelectedItem == null)
            {
                MessageBox.Show("Please select a role.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbRole.Focus();
                return false;
            }

            return true;
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateInputs())
            {
                MessageBox.Show("There are Some thing messing!", "Mistake", MessageBoxButton.OK, MessageBoxImage.None);
                EventLogger.LogWarning("This Form is invalid");
                return;
            }

            try {
                btnSave.IsEnabled = false;
                btnSave.Content = "Saving...";

                if(!isEditMode) {
                    isEditMode = true;
                    tbModalTitle.Text = "Edit Existing User";

                    // We Need to Create New User to Avoid Any Errors While Saving it
                    User newUser = new User();
                    newUser.Username = txtUsername.Text.Trim();
                    newUser.Password = txtPassword.Password;
                    newUser.PersonId = personSelector.CurrentPersonID.Value;
                    newUser.IsActive = chkIsActive.IsChecked ?? false;
                    newUser.CreatedAt = DateTime.Now;
                    newUser.UpdatedAt = DateTime.Now;

                    // set the role
                    // (We Cannot Use the index to set it Because The Enum of the role use 2 to the power of N to identify the role!)
                    var selectedRole = cmbRole.SelectedItem as ComboBoxItem;
                    newUser.RoleName = selectedRole.Tag.ToString();
                    newUser.Role = GetRoleEnum(newUser.RoleName);

                    if(await newUser.Save()) {
                        MessageBox.Show(
                            "User created successfully!", 
                            "Success",
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                        EventLogger.LogInfo("User created successfully!");
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Failed to create user. Username might already exist.", 
                            "Error",
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                        EventLogger.LogWarning("Failed to create user. Username might already exist.");
                        btnSave.IsEnabled = true;
                        btnSave.Content = "Create User";
                    }

                    return;
                }
                else
                {
                    // Update an Existing User
                    _CurrentUser.Username = txtUsername.Text.Trim();
                    _CurrentUser.IsActive = chkIsActive.IsChecked ?? false;

                    // Update Password only if provided
                    if (!string.IsNullOrEmpty(txtPassword.Password))
                    {
                        _CurrentUser.Password = txtPassword.Password;
                    }

                    // Update The role
                    var selectedRole = cmbRole.SelectedItem as ComboBoxItem;
                    _CurrentUser.RoleName = selectedRole.Tag.ToString();
                    _CurrentUser.Role = GetRoleEnum(_CurrentUser.RoleName);
                    _CurrentUser.UpdatedAt = DateTime.Now;

                    if (await _CurrentUser.Save())
                    {
                        MessageBox.Show(
                            "User updated successfully!",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        EventLogger.LogInfo("User updated successfully!");
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Failed to update user. Please try again.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        EventLogger.LogWarning("Failed to update user. Please try again.");
                        btnSave.IsEnabled = true;
                        btnSave.Content = "Update User";
                    }
                }

            }
            catch(Exception ex) {
                MessageBox.Show(
                    $"Error saving user: {ex.Message}", 
                    "Error",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                btnSave.IsEnabled = true;
                btnSave.Content = isEditMode ? "Update User" : "Create User";
                EventLogger.LogError("An Error Was Occured While Saving the User!!", ex);
                return;
            }
        }

        private User.enRole GetRoleEnum(string RoleName)
        {
            return RoleName switch
            {
                "Admin" => User.enRole.eAdmin,
                "Trainer" => User.enRole.eTrainer,
                "Student" => User.enRole.eStudent,
                "Receptionist" => User.enRole.eReceptionist,
                "Manager" => User.enRole.eManager,
                _ => User.enRole.eNone
            };
        }
    }
}
