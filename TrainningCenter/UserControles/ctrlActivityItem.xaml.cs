using MahApps.Metro.IconPacks;
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
    /// <summary>
    /// Interaction logic for ctrlActivityItem.xaml
    /// </summary>
    public partial class ctrlActivityItem : UserControl
    {
        public ctrlActivityItem()
        {
            InitializeComponent();
        }


        // --- Dependency Properties to allow Data Binding ---

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(ctrlActivityItem),
                new PropertyMetadata("Activity Title"));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                "Description",
                typeof(string),
                typeof(ctrlActivityItem),
                new PropertyMetadata("Activity description goes here."));

        public static readonly DependencyProperty TimeAgoProperty =
            DependencyProperty.Register(
                "TimeAgo",
                typeof(string),
                typeof(ctrlActivityItem),
                new PropertyMetadata("Just now"));

        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(
                "IconKind",
                typeof(PackIconMaterialKind),
                typeof(ctrlActivityItem),
                new PropertyMetadata(PackIconMaterialKind.Bell));

        public static readonly DependencyProperty IconForegroundProperty =
            DependencyProperty.Register(
                "IconForeground",
                typeof(Brush),
                typeof(ctrlActivityItem),
                new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty IconBackgroundProperty =
            DependencyProperty.Register(
                "IconBackground",
                typeof(Brush),
                typeof(ctrlActivityItem),
                new PropertyMetadata(Brushes.Transparent));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public string TimeAgo
        {
            get { return (string)GetValue(TimeAgoProperty); }
            set { SetValue(TimeAgoProperty, value); }
        }
        public PackIconMaterialKind IconKind
        {
            get { return (PackIconMaterialKind)GetValue(IconKindProperty); }
            set { SetValue(IconKindProperty, value); }
        }
        public Brush IconForeground
        {
            get { return (Brush)GetValue(IconForegroundProperty); }
            set { SetValue(IconForegroundProperty, value); }
        }
        public Brush IconBackground
        {
            get { return (Brush)GetValue(IconBackgroundProperty); }
            set { SetValue(IconBackgroundProperty, value); }
        }
    }
}
