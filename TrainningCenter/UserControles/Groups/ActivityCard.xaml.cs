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
using TrainingCenter_BusinessLayer;
using TrainningCenter.ViewModel;

namespace TrainningCenter.UserControles.Groups
{
    /// <summary>
    /// Interaction logic for ActivityCard.xaml
    /// </summary>
    public partial class ActivityCard : UserControl
    {
        public ActivityVM Activity
        {
            get { return (ActivityVM)GetValue(ActivityProperty); }
            set { SetValue(ActivityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Activity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActivityProperty = DependencyProperty
            .Register(
                "Activity", 
                typeof(ActivityVM), 
                typeof(ActivityCard), 
                new PropertyMetadata(null, OnActivityChanged));
        public ActivityCard()
        {
            InitializeComponent();
            //this.DataContext = this;
        }

        private static void OnActivityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is ActivityCard control)
            {
                var newActivity = e.NewValue as ActivityVM;

                if(newActivity != null)
                {
                    control.tbDescriptionText.Text = newActivity.Description;
                    control.tbTimeAgoText.Text = GetTimeAgo(newActivity.Timestamp);
                    control.tbTimestampText.Text = newActivity.Timestamp.ToString("dd MMMM yyyy, hh:mm tt");

                    // Logic to set the Icon and Color based on Activity Type
                    control.UpdateIconAndColor(newActivity.ActivityType);
                }
            }
        }

        /// <summary>
        /// Calculates the relative time span string (e.g., "5 minutes ago").
        /// </summary>
        private static string GetTimeAgo(DateTime timestamp)
        {
            TimeSpan timeSince = DateTime.Now.Subtract(timestamp);

            if (timeSince.TotalMinutes < 1)
                return "Just now";
            if (timeSince.TotalMinutes < 60)
                return $"{(int)timeSince.TotalMinutes} minutes ago";
            if (timeSince.TotalHours < 24)
                return $"{(int)timeSince.TotalHours} hours ago";
            if (timeSince.TotalDays < 30)
                return $"{(int)timeSince.TotalDays} days ago";
            if (timeSince.TotalDays < 365)
                return $"{(int)(timeSince.TotalDays / 30)} months ago";

            return $"{(int)(timeSince.TotalDays / 365)} years ago";
        }
        private void UpdateIconAndColor(string ActivityType)
        {
            PackIconMaterialKind iconKind = PackIconMaterialKind.Information;
            SolidColorBrush foreground = (SolidColorBrush)Application.Current.Resources["InfoBrush"];
            SolidColorBrush background = (SolidColorBrush)Application.Current.Resources["InfoLightBrush"];

            // Normalize the activity type string for reliable comparaison
            string normalizedType = ActivityType?.Trim().ToUpper() ?? string.Empty;

            switch (normalizedType)
            {
                case "PAYMENT_RECEIVED":
                case "INVOICE_PAID":
                    iconKind = PackIconMaterialKind.CurrencyUsd;
                    foreground = (SolidColorBrush)Application.Current.Resources["SuccessBrush"];
                    background = (SolidColorBrush)Application.Current.Resources["SuccessLightBrush"];
                    break;

                case "STUDENT_ENROLLED":
                case "GROUP_JOINED":
                    iconKind = PackIconMaterialKind.AccountPlus;
                    foreground = (SolidColorBrush)Application.Current.Resources["PrimaryBrush"];
                    background = (SolidColorBrush)Application.Current.Resources["PrimaryLightBrush"];
                    break;

                case "SUBSCRIPTION_EXPIRATION":
                case "LICENCE_EXPIRED":
                    iconKind = PackIconMaterialKind.AlertCircle;
                    foreground = (SolidColorBrush)Application.Current.Resources["DangerBrush"];
                    background = (SolidColorBrush)Application.Current.Resources["DangerLightBrush"];
                    break;

                case "COURSE_COMPLETED":
                    iconKind = PackIconMaterialKind.CheckCircle;
                    foreground = (SolidColorBrush)Application.Current.Resources["WarningBrush"];
                    background = (SolidColorBrush)Application.Current.Resources["WarningLightBrush"];
                    break;

                case "GroupUpdate":
                    iconKind = PackIconMaterialKind.Pencil;
                    foreground = (SolidColorBrush)Application.Current.Resources["InfoBrush"];
                    background = (SolidColorBrush)Application.Current.Resources["InfoLightBrush"];
                    break;

                default:
                    // Keep default (Information icon, Info colors) for any unknown or non-specific activities
                    break;
            }

            IconControl.Kind = iconKind;
            IconControl.Foreground = foreground;
            IconControl.Background = background;

            // The Border element is the parent of IconControl
            ((Border)IconControl.Parent).Background = background;
        }
    }
}
