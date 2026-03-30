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

namespace TrainningCenter.UserControles
{
    public partial class TopBar : UserControl
    {
        public TopBar() {
            InitializeComponent();

            // Subscription To Events
            txtSearchBox.TextChanged += TxtSearchBox_TextChanged;
            cbFilter.SelectionChanged += CbFilter_SelectionChanged;
        }


        public static readonly DependencyProperty WelcomMessageProperty =
            DependencyProperty.Register(
                "WelcomMessage", 
                typeof(string), 
                typeof(TopBar), 
                new PropertyMetadata(string.Empty, OnWelcomMessageChanged));

        public string WelcomMessage {
            get { return (string)GetValue(WelcomMessageProperty); }
            set { SetValue(WelcomMessageProperty, value); }
        }
        private static void OnWelcomMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var topBar = d as TopBar;
            topBar.WelcomeText.Text = e.NewValue?.ToString();
        }


        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(
                "Subtitle", 
                typeof(string), 
                typeof(TopBar), 
                new PropertyMetadata(string.Empty, OnSubtitleChanged));
        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }
        private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var topBar = d as TopBar;
            topBar.SubtitleText.Text = e.NewValue?.ToString();
        }



        //public string UserRole
        //{
        //    get { return (string)GetValue(UserRoleProperty); }
        //    set { SetValue(UserRoleProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for UserRole.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty UserRoleProperty =
        //    DependencyProperty.Register(
        //        "UserRole", 
        //        typeof(string), 
        //        typeof(TopBar), 
        //        new PropertyMetadata("", OnUserRoleChanged)
        //        );
        //private static void OnUserRoleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var topBar = d as TopBar;
        //    topBar.UserRoleText.Text = e.NewValue?.ToString();
        //}

        private void CbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchRequested?.Invoke(this, txtSearchBox.Text);
        }

        private void TxtSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearchBox.Text;
            // Implement search logic here
            SearchRequested?.Invoke(this, searchText);
        }

        public event EventHandler<string> SearchRequested;
    }
}
