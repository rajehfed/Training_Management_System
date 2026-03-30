using System;
using System.Collections.Generic;
using System.Linq;
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
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter.Views.Windows;

namespace TrainningCenter.Pages
{
    /// <summary>
    /// Interaction logic for UsersManagementsPage.xaml
    /// </summary>

    public partial class UsersManagementsPage : Page
    {
        private List<UserViewModel> _allUser = new List<UserViewModel>();
        
        public class UserViewModel
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string FullName { get; set; }
            public string Phone { get; set; }
            public string Initials { get; set; }
            public string RoleName { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? LastLogin { get; set; }
        }

        public UsersManagementsPage() {
            InitializeComponent();
            LoadUsers();
        }
        private async void LoadUsers()
        {
            try
            {
                _allUser.Clear();
                var users = await User.GetAllUser();

                foreach(var user in users) {
                    var person = user.PersonUser;

                    var userVM = new UserViewModel
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        FullName = person != null ? $"{person.FirstName} {person.LastName}" : "N/A",
                        Phone = person?.PhoneNumber ?? "N/A",
                        Initials = GetInitials(person?.FirstName, person?.LastName),
                        RoleName = user.RoleName,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        LastLogin = user.LastLogin != default(DateTime) ? user.LastLogin : null
                    };

                    _allUser.Add(userVM);
                }

                // HERE WE NEED TO DO SOME THING FOR APPLYING THE FILTER
                ApplyFilter();

            }
            catch(Exception ex) {
                EventLogger.LogError("Error loading users", ex);
                MessageBox.Show($"Error loading users: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ApplyFilter()
        {
            var filteredUsers = _allUser.AsEnumerable();

            // Search Filter
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                string searchTerm = txtSearch.Text.ToLower();
                filteredUsers = filteredUsers.Where(u =>
                    u.Username.ToLower().Contains(searchTerm) ||
                    u.FullName.ToLower().Contains(searchTerm) ||
                    u.Phone.ToLower().Contains(searchTerm)
                );
            }

            // Role Filter
            if(cmbRoleFilter.SelectedItem != null && cmbRoleFilter.SelectedIndex > 0)
            {
                string selectedRole = ((ComboBoxItem)cmbRoleFilter.SelectedItem).Content.ToString();
                filteredUsers = filteredUsers.Where(u => u.RoleName == selectedRole);
            }

            // Status Filter
            if(cmbStatusFilter.SelectedItem != null && cmbStatusFilter.SelectedIndex > 0)
            {
                bool isActive = ((ComboBoxItem)cmbStatusFilter.SelectedItem).Content.ToString() == "Active";
                filteredUsers = filteredUsers.Where(u => u.IsActive == isActive);
            }

            dgUsers.ItemsSource = filteredUsers.ToList();
        }
        private string GetInitials(string firstName, string lastName)
        {
            // Trim and check for null/whitespace in one go
            char? f = !string.IsNullOrWhiteSpace(firstName) ? firstName.Trim()[0] : null;
            char? l = !string.IsNullOrWhiteSpace(lastName) ? lastName.Trim()[0] : null;

            if (f == null && l == null)
                return "?";

            return $"{f}{l}".ToUpper();
        }
        private string GetRoleName(int roleID)
        {
            return roleID switch
            {
                1 => "Student",
                2 => "Admin",
                3 => "Trainer",
                4 => "Receptionist",
                5 => "Manager",
                _ => "Unkown"
            };
        }
        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddEditUserWindow();
            addUserWindow.Owner = Window.GetWindow(this);
            addUserWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (addUserWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void cmbRoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                ApplyFilter();
        }

        private void cmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                ApplyFilter();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            cmbRoleFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndex = 0;
            LoadUsers();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if(button?.Tag is int UserID)
            {
                var editUserWindow = new AddEditUserWindow(UserID);
                editUserWindow.Owner = Window.GetWindow(this); // Set Owner before showing
                editUserWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner; // Set location before showing

                if (editUserWindow.ShowDialog() == true) // Show the dialog and check the result
                {
                    LoadUsers();
                }
            }
            else
            {
                MessageBox.Show(
                    "User not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if(button?.Tag is int UserID)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this user?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        User user = await User.Find(UserID);
                        if (user != null)
                        {
                            if (await user.Delete())
                            {
                                MessageBox.Show(
                                    "User deleted successfully.",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                                EventLogger.LogInfo($"User deleted: ID={user.UserId}, Username={user.Username}");
                                LoadUsers();
                            }
                            else
                            {
                                MessageBox.Show(
                                    "Failed to deactivate user.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                                EventLogger.LogWarning($"Failed to delete user: ID={user.UserId}, Username={user.Username}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error deleting user", ex);
                        MessageBox.Show(
                            $"Error deleting user: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
