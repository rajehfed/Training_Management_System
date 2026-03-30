using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TrainingCenter_BusinessLayer;
using TrainningCenter.ViewModel;

namespace TrainningCenter.UserControles.Students
{
    public partial class StudentSelector : UserControl, INotifyPropertyChanged
    {
        #region Properties & Events

        public event EventHandler<StudentSelectedEventArgs> StudentSelected;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<StudentVM> _allStudents;
        private ObservableCollection<StudentVM> _filteredStudents;
        private StudentVM _selectedStudent;
        private string _currentFilter = "All";
        private DispatcherTimer _searchDebounceTimer;
        private bool _isDataLoaded = false; // Prevent double loading

        public StudentVM SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (_selectedStudent != value)
                {
                    if (_selectedStudent != null) _selectedStudent.IsSelected = false;

                    _selectedStudent = value;

                    if (_selectedStudent != null)
                    {
                        _selectedStudent.IsSelected = true;
                        UpdateSelectedFooter();
                    }
                    else
                    {
                        selectedFooter.Visibility = Visibility.Collapsed;
                    }

                    OnPropertyChanged(nameof(SelectedStudent));
                    StudentSelected?.Invoke(this, new StudentSelectedEventArgs(_selectedStudent));
                }
            }
        }

        #endregion

        #region Constructor

        public StudentSelector()
        {
            InitializeComponent();

            _allStudents = new ObservableCollection<StudentVM>();
            _filteredStudents = new ObservableCollection<StudentVM>();
            studentsListControl.ItemsSource = _filteredStudents;

            _searchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchDebounceTimer.Tick += SearchDebounceTimer_Tick;

            SetupFilterChips();
            Loaded += StudentSelector_Loaded;
        }

        #endregion

        #region Initialization

        private async void StudentSelector_Loaded(object sender, RoutedEventArgs e)
        {
            // FIX: Prevent re-loading data if tab is switched
            if (!_isDataLoaded)
            {
                await LoadStudentsAsync();
            }
        }

        private void SetupFilterChips()
        {
            // Optional: If chips are dynamic, keep this. 
            // If they are static in XAML, you can just add Click="FilterChip_Click" in XAML directly.
            foreach (var child in filterChips.Children)
            {
                if (child is Border chip) chip.MouseLeftButtonDown += FilterChip_Click;
            }
        }

        #endregion

        #region Data Loading

        public async Task LoadStudentsAsync()
        {
            try
            {
                ShowLoading(true);

                var students = await Student.GetAllStudent();

                if (students != null && students.Any())
                {
                    _allStudents.Clear();
                    foreach (var student in students)
                    {
                        _allStudents.Add(new StudentVM(student));
                    }

                    _isDataLoaded = true;
                    ApplyFilters();
                }
                else
                {
                    ShowEmptyState(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading students: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        #endregion

        #region Selection

        private void btnClearSelection_Click(object sender, RoutedEventArgs e)
        {
            SelectedStudent = null; // Setter handles logic
        }

        private void StudentCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // FIX: Get DataContext directly instead of relying on Tag
            if (sender is FrameworkElement element && element.DataContext is StudentVM student)
            {
                SelectedStudent = student;

                if (sender is Border border)
                    PlaySelectionAnimation(border);
            }
        }

        #endregion

        #region UI Updates & Logic

        private void UpdateStudentCount()
        {
            tbStudentCount.Text = _filteredStudents.Count == 1 ? "1 Student" : $"{_filteredStudents.Count} Students";
        }

        private void UpdateSelectedFooter()
        {
            if (_selectedStudent != null)
            {
                tbSelectedStudent.Text = $"{_selectedStudent.FullName} ({_selectedStudent.StudentNumber})";
                selectedFooter.Visibility = Visibility.Visible;
            }
        }

        private void ShowEmptyState(bool show)
        {
            emptyState.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            studentsListControl.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ShowLoading(bool show)
        {
            loadingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            // Only start animation if visible to save resources
            if (show && FindResource("ShimmerAnimation") is Storyboard sb) sb.Begin();
        }

        private void ApplyFilters()
        {
            var searchText = txtSearch.Text?.Trim().ToLower() ?? string.Empty;

            var filteredList = _allStudents.Where(s =>
            {
                bool matchesSearch = string.IsNullOrWhiteSpace(searchText) ||
                    s.FullName.ToLower().Contains(searchText) ||
                    s.StudentNumber.ToLower().Contains(searchText);

                bool matchesStatus = _currentFilter == "All" ||
                    (_currentFilter == "Active" && s.IsActive) ||
                    (_currentFilter == "Inactive" && !s.IsActive);

                return matchesSearch && matchesStatus;
            }).ToList();

            // Optimization: Only update if count changed or search changed
            _filteredStudents.Clear();
            foreach (var student in filteredList)
            {
                _filteredStudents.Add(student);
            }

            ShowEmptyState(_filteredStudents.Count == 0);
            UpdateStudentCount();

            if (_filteredStudents.Any())
            {
                // FIX: Wait for UI to render items before animating
                Dispatcher.InvokeAsync(AnimateStudentCards, DispatcherPriority.Loaded);
            }
        }

        #endregion

        #region Search & Filter Event Handlers

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnClearSearch.Visibility = string.IsNullOrWhiteSpace(txtSearch.Text) ? Visibility.Collapsed : Visibility.Visible;
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private void SearchDebounceTimer_Tick(object sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            ApplyFilters();
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            txtSearch.Focus();
        }

        private void FilterChip_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedChip)
            {
                _currentFilter = clickedChip.Tag.ToString();

                // Visual Updates
                foreach (var child in filterChips.Children)
                {
                    if (child is Border chip)
                    {
                        bool isActive = (chip == clickedChip);
                        chip.Background = isActive ? (Brush)FindResource("PrimaryBrush") : Brushes.White;
                        chip.BorderBrush = isActive ? (Brush)FindResource("PrimaryBrush") : (Brush)FindResource("BorderBrush");

                        if (FindVisualChild<TextBlock>(chip) is TextBlock tb)
                        {
                            tb.Foreground = isActive ? Brushes.White : (Brush)FindResource("TextPrimaryBrush");
                        }
                    }
                }
                ApplyFilters();
            }
        }

        #endregion

        #region Animations

        private void AnimateStudentCards()
        {
            // Limit animation to visible items to prevent lag
            int count = 0;
            foreach (var student in _filteredStudents)
            {
                // FIX: Check for null container (happens if UI isn't ready)
                var container = studentsListControl.ItemContainerGenerator.ContainerFromItem(student) as FrameworkElement;
                if (container != null)
                {
                    container.Opacity = 0;
                    var fadeIn = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(400),
                        BeginTime = TimeSpan.FromMilliseconds(count * 30), // 30ms stagger is smoother
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    container.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                }

                count++;
                if (count > 15) break; // Only animate the first 15 items
            }
        }

        private void PlaySelectionAnimation(Border border)
        {
            var scaleTransform = border.RenderTransform as ScaleTransform;
            if (scaleTransform == null)
            {
                scaleTransform = new ScaleTransform(1, 1);
                border.RenderTransform = scaleTransform;
                border.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            var pulseAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 1.05,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, pulseAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, pulseAnimation);
        }

        #endregion

        #region Helpers
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild) return typedChild;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
        #endregion
    }

    public class StudentSelectedEventArgs : EventArgs
    {
        public StudentVM SelectedStudent { get; }
        public StudentSelectedEventArgs(StudentVM student) => SelectedStudent = student;
    }
}