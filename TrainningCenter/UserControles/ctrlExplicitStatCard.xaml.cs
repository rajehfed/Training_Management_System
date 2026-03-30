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
    /// Interaction logic for ctrlExplicitStatCard.xaml
    /// </summary>
    public partial class ctrlExplicitStatCard : UserControl
    {
        public ctrlExplicitStatCard()
        {
            InitializeComponent();
        }

        #region Dependency Properties
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(ctrlExplicitStatCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(string),
                typeof(ctrlExplicitStatCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register(
                nameof(StatusText),
                typeof(string),
                typeof(ctrlExplicitStatCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty StatusKindProperty =
            DependencyProperty.Register(
                nameof(StatusKind),
                typeof(PackIconMaterialKind),
                typeof(ctrlExplicitStatCard),
                new PropertyMetadata(PackIconMaterialKind.CheckCircle));

        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(
                nameof(IconKind),
                typeof(PackIconMaterialKind),
                typeof(ctrlExplicitStatCard),
                new PropertyMetadata(PackIconMaterialKind.Information));

        public static readonly DependencyProperty AccentBrushProperty =
            DependencyProperty.Register(
                nameof(AccentBrush),
                typeof(Brush),
                typeof(ctrlExplicitStatCard),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD")))); // لون خلفية الأيقونة

        public static readonly DependencyProperty IconForegroundProperty =
            DependencyProperty.Register(
                nameof(IconForeground),
                typeof(Brush),
                typeof(ctrlExplicitStatCard),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"))));

        public static readonly DependencyProperty StatusBrushProperty =
            DependencyProperty.Register(
                nameof(StatusBrush),
                typeof(Brush),
                typeof(ctrlExplicitStatCard),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"))));

        public static readonly DependencyProperty CardCommandProperty =
            DependencyProperty.Register(
                nameof(CardCommand),
                typeof(ICommand),
                typeof(ctrlExplicitStatCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                nameof(CommandParameter),
                typeof(object),
                typeof(ctrlExplicitStatCard),
                new PropertyMetadata(null));


        public static readonly DependencyProperty AdditionalTextProperty =
            DependencyProperty.Register(
                nameof(AdditionalText),
                typeof(string),
                typeof(ctrlExplicitStatCard),
                new PropertyMetadata(string.Empty));


        #endregion

        #region Public Properties

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public PackIconMaterialKind StatusKind
        {
            get => (PackIconMaterialKind)GetValue(StatusKindProperty);
            set => SetValue(StatusKindProperty, value);
        }

        public PackIconMaterialKind IconKind
        {
            get => (PackIconMaterialKind)GetValue(IconKindProperty);
            set => SetValue(IconKindProperty, value);
        }

        public Brush AccentBrush
        {
            get => (Brush)GetValue(AccentBrushProperty);
            set => SetValue(AccentBrushProperty, value);
        }

        public Brush IconForeground
        {
            get => (Brush)GetValue(IconForegroundProperty);
            set => SetValue(IconForegroundProperty, value);
        }

        public Brush StatusBrush
        {
            get => (Brush)GetValue(StatusBrushProperty);
            set => SetValue(StatusBrushProperty, value);
        }

        public ICommand CardCommand
        {
            get => (ICommand)GetValue(CardCommandProperty);
            set => SetValue(CardCommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public string AdditionalText
        {
            get { return (string)GetValue(AdditionalTextProperty); }
            set { SetValue(AdditionalTextProperty, value); }
        }
        #endregion

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CardCommand?.CanExecute(CommandParameter) == true)
            {
                CardCommand.Execute(CommandParameter);
            }
        }
    }
}
