using System;
using System.Collections.Generic;
using System.Data;
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
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;
using TrainningCenter.Views.Windows;

namespace TrainningCenter.Pages
{
    public partial class PeoplePage : Page
    {
        private List<Person> _allPeople;
        public PeoplePage()
        {
            InitializeComponent();
            _LoadPeopleData();
        }

        private async void _LoadPeopleData()
        {
            try
            {
                DataTable dt = await Person.GetAllPeople();
                _allPeople = new List<Person>();

                foreach(DataRow row in dt.Rows)
                {
                    var person = await Person.Find((int)row["PersonID"]);
                    if (person != null)
                        _allPeople.Add(person);
                }

                dgPeople.ItemsSource = _allPeople;

                // Next Time i will implement here some Statistics about persons
            }
            catch(Exception ex) {
                EventLogger.LogError("An Error Was Ocured When the function _LoadPeopleData try to get data from The db", ex);
                MessageBox.Show(
                    $"Error loading people: {ex.Message}", 
                    "Error",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.Tag is int personId) {
                MessageBox.Show($"View Person ID: {personId}");
                // Navigate to details page
                ViewPersonDetailsWindow viewPersonDetailsWindow = new ViewPersonDetailsWindow(personId);
                viewPersonDetailsWindow.ShowDialog();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.Tag is int personId) {
                MessageBox.Show($"Edit Person ID: {personId}");
                AddUpdatePersonWindow addUpdatePersonWindow = new AddUpdatePersonWindow(personId);
                addUpdatePersonWindow.ShowDialog();
                _LoadPeopleData();
            }
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // we save the selected id in the tag
            if(sender is Button btn && btn.Tag is int personIdToDelete)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this person?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if(result == MessageBoxResult.Yes)
                {
                    Person personToDelete = await Person.Find(personIdToDelete);
                    
                    if(personToDelete == null)
                    {
                        EventLogger.LogWarning($"No Person With Id: {personIdToDelete}");
                        MessageBox.Show($"No Person With Id: {personIdToDelete}", "Error", MessageBoxButton.OK);
                        return;
                    }

                    bool success = await personToDelete.DeletePerson();
                    if (success) {
                        MessageBox.Show("Person deleted successfully!");
                        _LoadPeopleData(); // Refresh
                    }
                    else {
                        MessageBox.Show("Failed to delete person. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnAddNewPerson_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to Add Person page
            AddUpdatePersonWindow addUpdatePersonWindow = new AddUpdatePersonWindow();
            addUpdatePersonWindow.ShowDialog();
            _LoadPeopleData();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if(dgPeople.SelectedItems.Count > 0) {
                btnUpdate.IsEnabled = true;
            }
            else { 
                btnUpdate.IsEnabled = false;
                return;
            }

            btnEdit_Click(sender, e);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allPeople == null) return;
            if (txtSearch.Text == "Search by name, email, or phone...") return;

            // 1. Get the current text from the search box
            string filter = txtSearch.Text.Trim().ToLower();

            // 2. If the search box is empty, show everyone
            if (string.IsNullOrWhiteSpace(filter))
            {
                dgPeople.ItemsSource = _allPeople;
                return;
            }

            // 3. Use LINQ to filter the list based on multiple properties
            var filteredList = _allPeople.Where(p =>
                (p.FirstName != null && p.FirstName.ToLower().Contains(filter)) ||
                (p.LastName != null && p.LastName.ToLower().Contains(filter)) ||
                (p.PhoneNumber != null && p.PhoneNumber.Contains(filter)) ||
                (p.Id.ToString().Contains(filter))
            ).ToList();

            // 4. Update the DataGrid
            dgPeople.ItemsSource = filteredList;
        }
    }
}
