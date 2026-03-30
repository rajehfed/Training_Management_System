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
    /// Interaction logic for ctrlEmptyStateOverlay.xaml
    /// </summary>
    public partial class ctrlEmptyStateOverlay : UserControl
    {
        public ctrlEmptyStateOverlay()
        {
            InitializeComponent();
        }


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.
            Register(
                "Title", 
                typeof(string), 
                typeof(ctrlEmptyStateOverlay), 
                new PropertyMetadata("No Data Available"));


        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public static readonly DependencyProperty MessageProperty = DependencyProperty
            .Register(
                "Message", 
                typeof(string), 
                typeof(ctrlEmptyStateOverlay), 
                new PropertyMetadata("Information will appear here once data has been created or loaded."));


        public PackIconMaterialKind IconKind
        {
            get { return (PackIconMaterialKind)GetValue(IconKindProperty); }
            set { SetValue(IconKindProperty, value); }
        }
        public static readonly DependencyProperty IconKindProperty = DependencyProperty
            .Register(
                "IconKind", 
                typeof(PackIconMaterialKind), 
                typeof(ctrlEmptyStateOverlay),
                new PropertyMetadata(PackIconMaterialKind.FolderOpenOutline));

    }
}
