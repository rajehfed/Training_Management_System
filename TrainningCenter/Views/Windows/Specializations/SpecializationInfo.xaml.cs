using System.Windows;
using System.Windows.Input;
using TrainningCenter.ViewModel;
using TrainingCenter_BusinessLayer;
using System.Threading.Tasks;

namespace TrainningCenter.Views.Windows
{
    public partial class SpecializationInfo : Window
    {
        private int _SpecializationID = 0;
        private SpecializationViewModel _vm;
        public SpecializationInfo(int SpecializationID)
        {
            InitializeComponent();
            _SpecializationID = SpecializationID;
            this.DataContext = new SpecializationViewModel(_SpecializationID);
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void btnCose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnViewStudents_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditSpecialization_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AddEditSpecialization(_SpecializationID);

            editWindow.Owner = this;
            if (editWindow.ShowDialog() == true)
            {
                // Refresh data
                _vm = new SpecializationViewModel(_SpecializationID);
                this.DataContext = _vm;
            }
        }
    }
}
