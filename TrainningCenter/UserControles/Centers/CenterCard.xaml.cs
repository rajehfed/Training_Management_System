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
    /// Interaction logic for CenterCard.xaml
    /// </summary>
    public partial class CenterCard : UserControl
    {
        // 1. Define Events
        public event RoutedEventHandler EditClicked;
        public event RoutedEventHandler DeleteClicked;
        public event RoutedEventHandler CardClicked;

        public CenterCard()
        {
            InitializeComponent();
        }

        private void CenterCardBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CardClicked?.Invoke(this, e);
        }

        private void btnEditCenter_Click(object sender, RoutedEventArgs e)
        {
            EditClicked?.Invoke(this, e);
            e.Handled = true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteClicked?.Invoke(this, e);
            e.Handled = true;
        }
    }
}
