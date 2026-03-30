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

namespace TrainningCenter.Views.Windows.Trainers
{
    /// <summary>
    /// Interaction logic for TrainerInformationsWindow.xaml
    /// </summary>
    public partial class TrainerInformationsWindow : Window
    {
        private TrainerVM _trainer;
        public TrainerInformationsWindow(TrainerVM trainer)
        {
            InitializeComponent();
            _trainer = trainer;
            this.DataContext = trainer;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnEditTrainer_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
