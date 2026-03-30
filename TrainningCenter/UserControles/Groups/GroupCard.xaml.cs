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
using TrainingCenter_BusinessLayer;

namespace TrainningCenter.UserControles.Groups
{
    /// <summary>
    /// Interaction logic for GroupCard.xaml
    /// </summary>
    public partial class GroupCard : UserControl
    {
        public event EventHandler<Group> GroupClicked;
        private Group _group;
        public GroupCard()
        {
            InitializeComponent();
        }

        public void SetGroup(Group group)
        {
            _group = group;
            tbGroupNameText.Text = group.GroupName;
            tbMemberCountText.Text = $"{group.CurrentTrainees} members";
            ActiveBadge.Visibility = group.StatusName == "Active" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GroupClicked?.Invoke(this, _group);
        }
    }
}
