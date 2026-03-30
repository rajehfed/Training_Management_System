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
using TrainningCenter.Helpers;

namespace TrainningCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button _CurrentActiveButton; // for dectecting the current Active button
        public MainWindow()
        {
            InitializeComponent();
            _CurrentActiveButton = btnDashboard;
            tbInitials.Text = Globals.CurrentUser.Initials;
            tbRole.Text = Globals.CurrentUser.RoleDisplayName;
            tbUsername.Text = Globals.CurrentUser.Username;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // Minimize window
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Maximize/Restore window
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        // Close window
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnSpecialization_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (_CurrentActiveButton != null)
            {
                _CurrentActiveButton.Style = (Style)FindResource("menuButton");
            }

            if (clickedButton != null)
            {
                clickedButton.Style = (Style)FindResource("menuButtonActive");
                _CurrentActiveButton = clickedButton;
            }

            MainFrame.Source = new Uri("/Views/Pages/SpecializationsPage.xaml", UriKind.Relative);
        }

        private void btnPeople_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if(_CurrentActiveButton != null)
            {
                _CurrentActiveButton.Style = (Style)FindResource("menuButton");
            }

            if(clickedButton != null)
            {
                clickedButton.Style = (Style)FindResource("menuButtonActive");
                _CurrentActiveButton = clickedButton;
            }

            MainFrame.Source = new Uri("/Views/Pages/PeoplePage.xaml", UriKind.Relative);
        }

        private void btnGroups_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (_CurrentActiveButton != null)
            {
                _CurrentActiveButton.Style = (Style)FindResource("menuButton");
            }

            if(clickedButton != null)
            {
                clickedButton.Style = (Style)FindResource("menuButtonActive");
                _CurrentActiveButton = clickedButton;
            }

            MainFrame.Source = new Uri("/Views/Pages/GroupsPage.xaml", UriKind.Relative);
        }

        private void btnStudents_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (_CurrentActiveButton != null)
            {
                _CurrentActiveButton.Style = (Style)FindResource("menuButton");
            }

            if (clickedButton != null)
            {
                clickedButton.Style = (Style)FindResource("menuButtonActive");
                _CurrentActiveButton = clickedButton;
            }

            MainFrame.Source = new Uri("/Views/Pages/StudentPage.xaml", UriKind.Relative);
        }

        private void btnUsers_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (_CurrentActiveButton != null)
            {
                _CurrentActiveButton.Style = (Style)FindResource("menuButton");
            }

            if (clickedButton != null)
            {
                clickedButton.Style = (Style)FindResource("menuButtonActive");
                _CurrentActiveButton = clickedButton;
            }

            MainFrame.Source = new Uri("/Views/Pages/UsersManagementsPage.xaml", UriKind.Relative);
        }

        private void btnTrainers_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (_CurrentActiveButton != null)
            {
                _CurrentActiveButton.Style = (Style)FindResource("menuButton");
            }

            if (clickedButton != null)
            {
                clickedButton.Style = (Style)FindResource("menuButtonActive");
                _CurrentActiveButton = clickedButton;
            }

            MainFrame.Source = new Uri("/Views/Pages/TrainersPage.xaml", UriKind.Relative);
        }

        private void btnCenters_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (_CurrentActiveButton != null)
            {
                _CurrentActiveButton.Style = (Style)FindResource("menuButton");
            }

            if (clickedButton != null)
            {
                clickedButton.Style = (Style)FindResource("menuButtonActive");
                _CurrentActiveButton = clickedButton;
            }

            MainFrame.Source = new Uri("/Views/Pages/CentersPage.xaml", UriKind.Relative);
        }

        private void btnSessions_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (_CurrentActiveButton != null)
            {
                _CurrentActiveButton.Style = (Style)FindResource("menuButton");
            }

            if (clickedButton != null)
            {
                clickedButton.Style = (Style)FindResource("menuButtonActive");
                _CurrentActiveButton = clickedButton;
            }

            MainFrame.Source = new Uri("/Views/Pages/SessionsPage.xaml", UriKind.Relative);
        }

        private void btnAssessments_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (_CurrentActiveButton != null)
            {
                _CurrentActiveButton.Style = (Style)FindResource("menuButton");
            }

            if (clickedButton != null)
            {
                clickedButton.Style = (Style)FindResource("menuButtonActive");
                _CurrentActiveButton = clickedButton;
            }

            MainFrame.Source = new Uri("/Views/Pages/AssessmentsPage.xaml", UriKind.Relative);
        }

        private void btnGradesBook_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (_CurrentActiveButton != null)
            {
                _CurrentActiveButton.Style = (Style)FindResource("menuButton");
            }

            if (clickedButton != null)
            {
                clickedButton.Style = (Style)FindResource("menuButtonActive");
                _CurrentActiveButton = clickedButton;
            }

            MainFrame.Source = new Uri("/Views/Pages/GradeBookPage.xaml", UriKind.Relative);
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (_CurrentActiveButton != null)
            {
                _CurrentActiveButton.Style = (Style)FindResource("menuButton");
            }

            if (clickedButton != null)
            {
                clickedButton.Style = (Style)FindResource("menuButtonActive");
                _CurrentActiveButton = clickedButton;
            }

            MainFrame.Source = new Uri("/Views/Pages/DashboardPage.xaml", UriKind.Relative);
        }
    }
}
