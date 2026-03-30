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
using TrainningCenter.ViewModel;

namespace TrainningCenter.UserControles.Groups
{
    /// <summary>
    /// Interaction logic for MemberCard.xaml
    /// </summary>
    public partial class MemberCard : UserControl
    {
        #region Event For CRUD
        /// <summary>
        /// Raised when Edit button is clicked
        /// </summary>
        public event RoutedEventHandler EditClicked;

        /// <summary>
        /// Raised when Delete button is clicked
        /// </summary>
        public event RoutedEventHandler DeleteClicked;
        #endregion

        #region  Dependency Property
        // 1. Define the member Depecdancy Property using the ViewModel
        public MemberVM Member
        {
            get { return (MemberVM)GetValue(MemberProperty); }
            set { SetValue(MemberProperty, value); }
        }
        public static readonly DependencyProperty MemberProperty =
            DependencyProperty.Register(
                "Member", 
                typeof(MemberVM), 
                typeof(MemberCard), 
                new PropertyMetadata(null));

        public MemberCard()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers
        private void btnEditMember_Click(object sender, RoutedEventArgs e)
        {
            EditClicked?.Invoke(this, e);
        }

        private void btDeleteMember_Click(object sender, RoutedEventArgs e)
        {
            DeleteClicked?.Invoke(this, e);
        }

        #endregion
    }
}
