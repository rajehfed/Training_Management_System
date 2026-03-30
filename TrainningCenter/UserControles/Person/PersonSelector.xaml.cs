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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrainingCenter_BusinessLayer;
using TrainningCenter.Helpers;

namespace TrainningCenter.UserControles
{
    /// <summary>
    /// Interaction logic for PersonSelector.xaml
    /// </summary>
    public partial class PersonSelector : UserControl
    {
        // Public event to notify parent window when a person is selected
        public event EventHandler<PersonSelectedEventArgs> PersonSelectionChanged;
        public class PersonViewModel
        {
            public int PersonId { get; set; }
            public string FullName { get; set; }
            public string Phone { get; set; }
            public string Initials { get; set; }
        }


        // Property to access the currently selected person
        public class PersonSelectedEventArgs
        {
            public PersonViewModel SelectedPerson { get; set; }
        }

        private List<PersonViewModel> _AvailablePersons = new List<PersonViewModel>();
        private bool _isEditMode = false;
        private int? _currentPersonID = null;

        public PersonSelector()
        {
            InitializeComponent();
            //if (!DesignerProperties.GetIsInDesignMode(this))
            //{
            //    // Only load data if we are NOT in the designer
            //    _ = LoadAvailablePeople();
            //}
            this.Loaded += PersonSelector_Loaded;
        }

        private async void PersonSelector_Loaded(object sender, RoutedEventArgs e)
        {
            if (cmbPerson.ItemsSource == null && !DesignerProperties.GetIsInDesignMode(this))
            {
                await LoadAvailablePeople();
            }
        }

        /// <summary>
        /// Gets the currently selected person ID
        /// </summary>
        public int? CurrentPersonID
        {
            get { return (cmbPerson.SelectedItem as PersonViewModel)?.PersonId; }
        }

        /// <summary>
        /// Gets the currently selected person
        /// </summary>
        public PersonViewModel SelectedPerson
        {
            get { return (cmbPerson.SelectedItem as PersonViewModel); }
        }

        public async Task SetEditMode(int PersonID)
        {
            // We Should Make that function Stop the Fetch the data by itself
            // insead the `LoadAvailablePeople()` should Take the responsibility

            _isEditMode = true;
            _currentPersonID = PersonID;

            // Load All Available People + the current one
            await LoadAvailablePeople();

            var personToSelect = _AvailablePersons.FirstOrDefault(p => p.PersonId == PersonID);

            if(personToSelect != null)
            {
                cmbPerson.SelectedItem = personToSelect;
            }
            else
            {
                MessageBox.Show(
                    "The person associated with this user account could not be found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                EventLogger.LogError($"Person with ID {PersonID} not found in available persons.");
            }
        }

        /// <summary>
        /// Enables or disables the control
        /// </summary>
        public void EnableControl()
        {
            cmbPerson.IsEnabled = true;
            btnRefreshPersons.IsEnabled = true;
        }
        public void DisableControl()
        {
            cmbPerson.IsEnabled = false;
            btnRefreshPersons.IsEnabled = false;
        }

        /// <summary>
        /// Loads all persons without user accounts asynchronously.
        /// </summary>
        public async Task LoadAvailablePeople()
        {
            try
            {
                _AvailablePersons.Clear();

                // Get all persons
                var allPeople = await Person.GetPeopleList();

                // Get all Users for do the filtering 
                var allUsers = await User.GetAllUser();
                var usedPersonIDs = allUsers.Select(u => u.PersonId).ToHashSet();

                foreach (var person in allPeople)
                {
                    if (!usedPersonIDs.Contains(person.Id) ||
                        (_isEditMode && _currentPersonID == person.Id))
                    {
                        var personVM = new PersonViewModel
                        {
                            PersonId = person.Id,
                            FullName = $"{person.FirstName} {person.LastName}",
                            Phone = person.PhoneNumber,
                            Initials = GetInitials(person.FirstName, person.LastName)
                        };

                        _AvailablePersons.Add(personVM);
                    }
                }

                cmbPerson.ItemsSource = _AvailablePersons;
                CollectionViewSource.GetDefaultView(cmbPerson.ItemsSource)?.Refresh();

                if (_AvailablePersons.Count > 0 && !_isEditMode)
                {
                    cmbPerson.SelectedIndex = 0;
                }
                else if (_AvailablePersons.Count == 0)
                {
                    pnlPersonInfo.Visibility = Visibility.Collapsed;
                    MessageBox.Show(
                        "No available persons found. All persons already have user accounts.",
                        "Information",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading available persons: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                EventLogger.LogError($"Error loading available persons", ex);
            }
        }

        private void cmbPerson_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPerson.SelectedItem is PersonViewModel person)
            {
                ShowPersonInfo(person);
                
                // Raise the PersonSelectionChanged event
                PersonSelectionChanged?.Invoke(
                    this,
                    new PersonSelectedEventArgs { SelectedPerson = person }
                );
            }
        }

        private void ShowPersonInfo(PersonViewModel person)
        {
            pnlPersonInfo.Visibility = Visibility.Visible;
            tbPersonInitials.Text = person.Initials;
            tbPersonName.Text = person.FullName;
            txtPersonPhone.Text = person.Phone;
        }

        private string GetInitials(string firstName, string lastName)
        {
            string initials = "";

            if (!string.IsNullOrEmpty(firstName))
                initials += firstName[0];

            if (!string.IsNullOrEmpty(lastName))
                initials += lastName[0];

            return string.IsNullOrEmpty(initials) ? "?" : initials.ToUpper();
        }

        private async void btnRefreshPersons_Click(object sender, RoutedEventArgs e)
        {
            await LoadAvailablePeople();
        }

        /// <summary>
        /// Validates that a person is selected
        /// </summary>
        public bool ValidateSelection()
        {
            if(cmbPerson.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select a person.", 
                    "Validation Error",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning
                );
                EventLogger.LogWarning("Person selection validation failed: No person selected.");
                cmbPerson.Focus();
                return false;
            }

            return true;
        }
    }
}
