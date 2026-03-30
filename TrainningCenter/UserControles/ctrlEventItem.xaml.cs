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
    /// Interaction logic for ctrlEventItem.xaml
    /// </summary>
    public partial class ctrlEventItem : UserControl
    {
        public ctrlEventItem()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty EventTitleProperty =
            DependencyProperty.Register(
                "EventTitle", 
                typeof(string), 
                typeof(ctrlEventItem), 
                new PropertyMetadata("Event Title"));

        public static readonly DependencyProperty EventDateProperty =
            DependencyProperty.Register(
                "EventDate", 
                typeof(string), 
                typeof(ctrlEventItem), 
                new PropertyMetadata("Date Range"));

        public static readonly DependencyProperty EventSubtitleProperty =
            DependencyProperty.Register(
                "EventSubtitle", 
                typeof(string), 
                typeof(ctrlEventItem), 
                new PropertyMetadata("Subtitle"));

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register(
                "BackgroundColor", 
                typeof(Brush), 
                typeof(ctrlEventItem), 
                new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty AccentColorProperty =
           DependencyProperty.Register(
               "AccentColor", 
               typeof(Brush), 
               typeof(ctrlEventItem), 
               new PropertyMetadata(Brushes.Black));

        public string EventTitle
        {
            get { return (string)GetValue(EventTitleProperty); }
            set { SetValue(EventTitleProperty, value); }
        }
        public string EventDate
        {
            get { return (string)GetValue(EventDateProperty); }
            set { SetValue(EventDateProperty, value); }
        }
        public string EventSubtitle
        {
            get { return (string)GetValue(EventSubtitleProperty); }
            set { SetValue(EventSubtitleProperty, value); }
        }
        public Brush BackgroundColor
        {
            get { return (Brush)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }
        public Brush AccentColor
        {
            get { return (Brush)GetValue(AccentColorProperty); }
            set { SetValue(AccentColorProperty, value); }
        }
    }
}
