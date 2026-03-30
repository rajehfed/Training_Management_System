using MahApps.Metro.IconPacks;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TrainningCenter.UserControles.Groups
{
    /// <summary>
    /// Interaction logic for StatCard.xaml
    /// </summary>
    public partial class StatCard : UserControl
    {
        public StatCard()
        {
            InitializeComponent();
        }

        public string Value {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty
            .Register(
                "Value", 
                typeof(string), 
                typeof(StatCard), 
                new PropertyMetadata("0"));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static readonly DependencyProperty LabelProperty = DependencyProperty
            .Register(
                "Label", 
                typeof(string), 
                typeof(StatCard), 
                new PropertyMetadata(""));

        public PackIconMaterialKind IconKind
        {
            get { return (PackIconMaterialKind)GetValue(IconTypeProperty); }
            set { SetValue(IconTypeProperty, value); }
        }

        public static readonly DependencyProperty IconTypeProperty = DependencyProperty
            .Register(
                "IconKind", 
                typeof(PackIconMaterialKind), 
                typeof(StatCard), 
                new PropertyMetadata(PackIconMaterialKind.Alert));
        public Brush BackgroudBrush
        {
            get { return (Brush)GetValue(BackgroudBrushProperty); }
            set { SetValue(BackgroudBrushProperty, value); }
        }

        public static readonly DependencyProperty BackgroudBrushProperty = DependencyProperty
            .Register(
                "BackgroudBrush", 
                typeof(Brush), 
                typeof(StatCard), 
                new PropertyMetadata(null));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty
            .Register(
                "IconBrush", 
                typeof(Brush), 
                typeof(StatCard), 
                new PropertyMetadata(null));

    }
}
