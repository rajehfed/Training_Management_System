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

namespace TrainningCenter.UserControles.Centers
{
    /// <summary>
    /// Interaction logic for CenterListItem.xaml
    /// </summary>
    public partial class CenterListItem : UserControl
    {
        public event RoutedEventHandler EditClicked;
        public event RoutedEventHandler DeleteClicked;

        public CenterListItem()
        {
            InitializeComponent();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            EditClicked?.Invoke(this, e);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteClicked?.Invoke(this, e);
        }
    }
}
