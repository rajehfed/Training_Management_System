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
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrainningCenter.UserControles
{
    /// <summary>
    /// Interaction logic for QuickActionCard.xaml
    /// </summary>
    public partial class QuickActionCard : UserControl
    {

        #region Fields

        private Storyboard _hoverInStoryboard;
        private Storyboard _hoverOutStoryboard;
        private Storyboard _clickStoryboard;

        #endregion

        #region Constructor

        public QuickActionCard()
        {
            InitializeComponent();
            InitializeAnimations();

            // Set gradient colors when loaded
            this.Loaded += QuickActionCard_Loaded;
        }

        private void QuickActionCard_Loaded(object sender, RoutedEventArgs e)
        {
            // Update gradient colors from dependency properties
            UpdateGradientColors();
        }

        private static void OnGradientColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as QuickActionCard;
            control?.UpdateGradientColors();
        }

        private void UpdateGradientColors()
        {
            if (gradientStart != null && gradientEnd != null && shadowEffect != null)
            {
                gradientStart.Color = GradientStartColor;
                gradientEnd.Color = GradientEndColor;
                shadowEffect.Color = GradientStartColor;
            }
        }

        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(QuickActionCard),
                new PropertyMetadata("Action Title"));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                nameof(Description),
                typeof(string),
                typeof(QuickActionCard),
                new PropertyMetadata("Action description"));

        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(
                nameof(IconKind),
                typeof(PackIconMaterialKind),
                typeof(QuickActionCard),
                new PropertyMetadata(PackIconMaterialKind.Star));

        public static readonly DependencyProperty GradientStartColorProperty =
            DependencyProperty.Register(
                nameof(GradientStartColor),
                typeof(Color),
                typeof(QuickActionCard),
                new PropertyMetadata((Color)ColorConverter.ConvertFromString("#8147E7"), OnGradientColorChanged));

        public static readonly DependencyProperty GradientEndColorProperty =
            DependencyProperty.Register(
                nameof(GradientEndColor),
                typeof(Color),
                typeof(QuickActionCard),
                new PropertyMetadata((Color)ColorConverter.ConvertFromString("#A980F5"), OnGradientColorChanged));

        public static readonly DependencyProperty CardCommandProperty =
            DependencyProperty.Register(
                nameof(CardCommand),
                typeof(ICommand),
                typeof(QuickActionCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                nameof(CommandParameter),
                typeof(object),
                typeof(QuickActionCard),
                new PropertyMetadata(null));
        #endregion

        #region Public Properties

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public PackIconMaterialKind IconKind
        {
            get => (PackIconMaterialKind)GetValue(IconKindProperty);
            set => SetValue(IconKindProperty, value);
        }

        public Color GradientStartColor
        {
            get => (Color)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public Color GradientEndColor
        {
            get => (Color)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
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

        #endregion

        #region Animation Setup
        private void InitializeAnimations()
        {
            // Hover In Animation
            _hoverInStoryboard = new Storyboard();

            // Lift card up
            var liftAnimation = new DoubleAnimation
            {
                To = -6,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(liftAnimation, translateTransform);
            Storyboard.SetTargetProperty(liftAnimation, new PropertyPath(TranslateTransform.YProperty));
            _hoverInStoryboard.Children.Add(liftAnimation);

            // Enhance shadow
            var shadowAnimation = new DoubleAnimation
            {
                To = 0.5,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(shadowAnimation, shadowEffect);
            Storyboard.SetTargetProperty(shadowAnimation, new PropertyPath(DropShadowEffect.OpacityProperty));
            _hoverInStoryboard.Children.Add(shadowAnimation);

            var shadowBlurAnimation = new DoubleAnimation
            {
                To = 30,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(shadowBlurAnimation, shadowEffect);
            Storyboard.SetTargetProperty(shadowBlurAnimation, new PropertyPath(DropShadowEffect.BlurRadiusProperty));
            _hoverInStoryboard.Children.Add(shadowBlurAnimation);

            // Scale icon
            var iconScaleXAnimation = new DoubleAnimation
            {
                To = 1.1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
            };
            Storyboard.SetTarget(iconScaleXAnimation, iconScaleActual);
            Storyboard.SetTargetProperty(iconScaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
            _hoverInStoryboard.Children.Add(iconScaleXAnimation);

            var iconScaleYAnimation = new DoubleAnimation
            {
                To = 1.1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
            };
            Storyboard.SetTarget(iconScaleYAnimation, iconScaleActual);
            Storyboard.SetTargetProperty(iconScaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));
            _hoverInStoryboard.Children.Add(iconScaleYAnimation);

            // Move arrow
            var arrowAnimation = new DoubleAnimation
            {
                To = 4,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(arrowAnimation, arrowTransform);
            Storyboard.SetTargetProperty(arrowAnimation, new PropertyPath(TranslateTransform.XProperty));
            _hoverInStoryboard.Children.Add(arrowAnimation);

            // Hover Out Animation (reverse)
            _hoverOutStoryboard = new Storyboard();

            var liftOutAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(liftOutAnimation, translateTransform);
            Storyboard.SetTargetProperty(liftOutAnimation, new PropertyPath(TranslateTransform.YProperty));
            _hoverOutStoryboard.Children.Add(liftOutAnimation);

            var shadowOutAnimation = new DoubleAnimation
            {
                To = 0.3,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(shadowOutAnimation, shadowEffect);
            Storyboard.SetTargetProperty(shadowOutAnimation, new PropertyPath(DropShadowEffect.OpacityProperty));
            _hoverOutStoryboard.Children.Add(shadowOutAnimation);

            var shadowBlurOutAnimation = new DoubleAnimation
            {
                To = 20,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(shadowBlurOutAnimation, shadowEffect);
            Storyboard.SetTargetProperty(shadowBlurOutAnimation, new PropertyPath(DropShadowEffect.BlurRadiusProperty));
            _hoverOutStoryboard.Children.Add(shadowBlurOutAnimation);

            var iconScaleXOutAnimation = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(iconScaleXOutAnimation, iconScaleActual);
            Storyboard.SetTargetProperty(iconScaleXOutAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
            _hoverOutStoryboard.Children.Add(iconScaleXOutAnimation);

            var iconScaleYOutAnimation = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(iconScaleYOutAnimation, iconScaleActual);
            Storyboard.SetTargetProperty(iconScaleYOutAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));
            _hoverOutStoryboard.Children.Add(iconScaleYOutAnimation);

            var arrowOutAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(arrowOutAnimation, arrowTransform);
            Storyboard.SetTargetProperty(arrowOutAnimation, new PropertyPath(TranslateTransform.XProperty));
            _hoverOutStoryboard.Children.Add(arrowOutAnimation);

            // Click Animation (quick scale)
            _clickStoryboard = new Storyboard();

            var clickScaleX = new DoubleAnimationUsingKeyFrames();
            clickScaleX.KeyFrames.Add(new EasingDoubleKeyFrame(0.95, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
            clickScaleX.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
            Storyboard.SetTarget(clickScaleX, iconScaleActual);
            Storyboard.SetTargetProperty(clickScaleX, new PropertyPath(ScaleTransform.ScaleXProperty));
            _clickStoryboard.Children.Add(clickScaleX);

            var clickScaleY = new DoubleAnimationUsingKeyFrames();
            clickScaleY.KeyFrames.Add(new EasingDoubleKeyFrame(0.95, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
            clickScaleY.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
            Storyboard.SetTarget(clickScaleY, iconScaleActual);
            Storyboard.SetTargetProperty(clickScaleY, new PropertyPath(ScaleTransform.ScaleYProperty));
            _clickStoryboard.Children.Add(clickScaleY);
        }
        #endregion

        #region Event Handlers
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Play click animation
            _clickStoryboard.Begin();

            // Execute command if available
            if (CardCommand?.CanExecute(CommandParameter) == true)
            {
                CardCommand.Execute(CommandParameter);
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            _hoverOutStoryboard.Stop();
            _hoverInStoryboard.Begin();
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            _hoverInStoryboard.Stop();
            _hoverOutStoryboard.Begin();
        }
        #endregion
    }
}
