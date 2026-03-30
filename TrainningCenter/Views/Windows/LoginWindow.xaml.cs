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

namespace TrainningCenter.Views.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private bool _isPasswordVisible = false;
        private TextBox _txtPasswordVisible;
        public LoginWindow()
        {
            InitializeComponent();
            InitializePasswordToggle();
            LoadSavedCredentials();
            txtUsername.Focus();
        }
        private void InitializePasswordToggle()
        {
            try
            {
                // Create a TextBox for showing password (hidden by default)
                _txtPasswordVisible = new TextBox
                {
                    Style = (Style)FindResource("ModernTextBox"),
                    Visibility = Visibility.Collapsed
                };

                // Add it to the same grid as password box
                var grid = txtPassword.Parent as Grid;
                if (grid != null)
                {
                    grid.Children.Add(_txtPasswordVisible);
                    _txtPasswordVisible.KeyDown += txtPassword_KeyDown;
                }
            }
            catch(Exception ex)
            {
                EventLogger.LogError("Error initializing password toggle", ex);
            }
        }

        private void LoadSavedCredentials()
        {
            try
            {
                string username = txtUsername.Text;
                string password = _isPasswordVisible ? _txtPasswordVisible.Text : txtPassword.Password;
                var credentials = Utils.LoadCredentials(ref username, ref password);

                if (credentials && !string.IsNullOrEmpty(username))
                {
                    txtUsername.Text = username;
                    chkRememberMe.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Error loading saved credentials", ex);
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit?",
                "Exit Application",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes) {
                Application.Current.Shutdown();
            }
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                txtPassword.Focus();
        }

        private void btnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible) {
                // Show password
                _txtPasswordVisible.Text = txtPassword.Password;
                _txtPasswordVisible.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;
                iconPasswordVisibility.Kind = PackIconMaterialKind.Eye;
            }
            else {
                // Hide password
                txtPassword.Password = _txtPasswordVisible.Text;
                txtPassword.Visibility = Visibility.Visible;
                _txtPasswordVisible.Visibility = Visibility.Collapsed;
                iconPasswordVisibility.Kind = PackIconMaterialKind.EyeOff;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Please contact your system administrator to reset your password.\n\n" +
                "Email: admin@trainingcenter.com\n" +
                "Phone: +213 555 123 456",
                "Forgot Password",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            _ = PerformLogin();
        }

        private async void btnQuickAdmin_Click(object sender, RoutedEventArgs e)
        {
            txtUsername.Text = "admin";
            txtPassword.Password = "admin123";
            await PerformLogin();
        }
        private async void btnQuickUser_Click(object sender, RoutedEventArgs e)
        {
            txtUsername.Text = "user";
            txtPassword.Password = "user123";
            await PerformLogin();
        }
        private void ShowError(string message)
        {
            tbErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
        private async Task PerformLogin()
        {
            // Hide error message
            ErrorMessage.Visibility = Visibility.Collapsed;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowError("Please enter your username");
                txtUsername.Focus();
                return;
            }

            string password = _isPasswordVisible ? _txtPasswordVisible.Text : txtPassword.Password;
            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your password");
                if (_isPasswordVisible)
                    _txtPasswordVisible.Focus();
                else
                    txtPassword.Focus();

                return;
            }

            // Show loading overlay
            LoadingOverlay.Visibility = Visibility.Visible;
            btnLogin.IsEnabled = false;

            try
            {
                // Authenticate User using the BLL
                User authenticatedUser = await AuthenticateUser(txtUsername.Text.Trim(), password);

                if (authenticatedUser != null)
                {
                    // Ckeck if the User is Active
                    if (!authenticatedUser.IsActive)
                    {
                        LoadingOverlay.Visibility = Visibility.Collapsed;
                        btnLogin.IsEnabled = true;
                        ShowError("Your account has been deactivated. Please contact an administrator.");
                        return;
                    }

                    // Update last login time
                    authenticatedUser.LastLogin = DateTime.Now;
                    await authenticatedUser.Save();

                    // Save credentials if remember me is checked
                    if(chkRememberMe.IsChecked == true)
                    {
                        // Call a helper Class that contain a methodes for saving Credentials
                        Utils.SaveCredentials(txtUsername.Text, password);
                    }
                    else
                    {
                        // Call a helper Class that contain a methodes for clearing Credentials
                        Utils.DeleteCredentials();
                    }

                    // Store current user in session `in this case a Global Class`
                    Globals.CurrentUser = authenticatedUser;

                    // Hide loading overlay
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                    this.Hide();

                    // Open main window
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.ShowDialog();

                    // Close login window
                    this.Close();
                }
                else
                {
                    // Hide loading overlay
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                    btnLogin.IsEnabled = true;

                    // Show error message
                    ShowError("Invalid username or password. Please try again.");

                    // Clear password
                    txtPassword.Password = string.Empty;
                    _txtPasswordVisible.Text = string.Empty;
                    txtPassword.Focus();
                }
            }
            catch(Exception ex)
            {
                // Hide loading overlay
                LoadingOverlay.Visibility = Visibility.Collapsed;
                btnLogin.IsEnabled = true;

                // Show user-friendly error message
                ShowError("Login failed. Please check your connection and try again.");

                EventLogger.LogError($"Login failed for user {txtUsername.Text}: {ex}");
            }
        }
        private async Task<User> AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            try
            {
                // Create a temporary user object to check credentials
                User tempUser = new User();

                bool UserExists = await tempUser.IsUserExists(username, password);

                if (!UserExists)
                    return null;

                User authenticatedUser = await User.FindByUsername(username);

                return authenticatedUser;
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Authentication error", ex);
                //Console.WriteLine();
                return null;
            }
        }
        private async void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await PerformLogin();
        }
    }
}
