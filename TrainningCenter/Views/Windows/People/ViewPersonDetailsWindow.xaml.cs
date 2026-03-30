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
    /// Interaction logic for ViewPersonDetailsWindow.xaml
    /// </summary>
    public partial class ViewPersonDetailsWindow : Window
    {
        private int _personID;
        public ViewPersonDetailsWindow(int personID)
        {
            InitializeComponent();

            _personID = personID;
            _LoadPersonData();
        }

        private async void _LoadPersonData()
        {
            try
            {
                Person person = await Person.Find(_personID);

                tbPersonID.Text = $"Person ID: #{_personID}";

                // Personal Information
                tbFirstName.Text = person.FirstName;
                tbLastName.Text = person.LastName;
                tbFullName.Text = person.FullName;
                tbNationalNo.Text = person.NationalNo;
                tbDateOfBirth.Text = person.DateOfBirth.ToString("d");
                tbAge.Text = Utils.CalculateAge(person.DateOfBirth).ToString();
                tbGender.Text = person.Gender == Person.enGender.Male ? "Male" : "Female";
                tbNationality.Text = person.Nationality;

                // Set gender icon
                iconGender.Kind = (person.Gender == Person.enGender.Male) ?
                    PackIconMaterialKind.GenderMale : PackIconMaterialKind.GenderFemale;
                iconGender.Foreground =
                    (person.Gender == Person.enGender.Male)
                    ? new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4A90E2"))
                    : new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#8147E7"));

                // Contact Information
                tbPhoneNumber.Text = person.PhoneNumber;
                tbEmail.Text = person.Email;
                tbAddress.Text = person.Address;

                // Additional Information
                tbCreatedDate.Text = person.CreatedAt.ToString("d");
                tbLastModified.Text = person.LastUpdatedAt.ToString("d");

                // Load profile image if exists
                imgProfile.Source = ImageHelper.LoadImage(ImageHelper.GetFullImagePath(person.ImagePath));
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to Fill the Window with the person Data Due to an Error", ex);
                MessageBox.Show($"Error loading person data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddUpdatePersonWindow addUpdatePersonWindow = new AddUpdatePersonWindow(_personID);
                addUpdatePersonWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Error opening edit window", ex);
                MessageBox.Show(
                    $"Error opening edit window: {ex.Message}",
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error
                );
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // implement deletion methodes
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
