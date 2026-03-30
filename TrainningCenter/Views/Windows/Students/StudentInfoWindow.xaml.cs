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
using TrainningCenter.ViewModel;

namespace TrainningCenter.Views.Windows.Students
{
    /// <summary>
    /// Interaction logic for StudentInfoWindow.xaml
    /// </summary>
    public partial class StudentInfoWindow : Window
    {
        private StudentVM _student;
        public StudentInfoWindow(StudentVM vm)
        {
            InitializeComponent();
            _student = vm;
            this.DataContext = _student;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            // 1. Get a fresh copy for the Edit Window
            var modelForEditing = await _student.MapFromViewModel();

            var editWindow = new AddEditStudent(modelForEditing);
            editWindow.Owner = this; // Keeps it on top

            var result = editWindow.ShowDialog();

            if (result.HasValue && result.Value)
            {
                // 2. The Edit was successful!
                // Tell the current VM to re-fetch its data from the DB to see the changes.
                await _student.ReloadAsync();

                // No need to reset DataContext. ReloadAsync -> RefreshFromModel -> OnPropertyChanged 
                // handles the UI update automatically.
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
