using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using TrainningCenter.Views.Windows.Assessments;
using TrainningCenter_Entities;

namespace TrainningCenter.Views.Pages
{
    /// <summary>
    /// Interaction logic for AssessmentsPage.xaml
    /// </summary>
    public partial class AssessmentsPage : Page , INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Properties
        private ObservableCollection<AssessmentViewModel> _assessments;
        private ICollectionView _assessmentsView;
        private AssessmentViewModel _selectedAssessment;
        private string _searchKeyword = string.Empty;
        private enAssessmentType? _filterType;
        private bool _showActiveOnly = false;

        // Statistics
        private int _totalAssessments;
        private int _activeAssessments;
        private int _quizCount;
        private int _examCount;

        // Statistics Properties
        public ObservableCollection<AssessmentViewModel> Assessments
        {
            get => _assessments;
            set
            {
                _assessments = value;
                OnPropertyChanged();
                UpdateStatistics();
            }
        }

        public ICollectionView AssessmentsView
        {
            get => _assessmentsView;
            set
            {
                _assessmentsView = value;
                OnPropertyChanged();
            }
        }

        public AssessmentViewModel SelectedAssessment
        {
            get => _selectedAssessment;
            set
            {
                _selectedAssessment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAssessmentSelected));
            }
        }

        public bool IsAssessmentSelected => SelectedAssessment is not null;

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged();
                AssessmentsView?.Refresh();
            }
        }

        public bool ShowActiveOnly
        {
            get => _showActiveOnly;
            set
            {
                _showActiveOnly = value;
                OnPropertyChanged();
                AssessmentsView?.Refresh();
            }
        }

        // Statistics Properties
        public int TotalAssessments
        {
            get => _totalAssessments;
            set { _totalAssessments = value; OnPropertyChanged(); }
        }

        public int ActiveAssessments
        {
            get => _activeAssessments;
            set { _activeAssessments = value; OnPropertyChanged(); }
        }

        public int QuizCount
        {
            get => _quizCount;
            set { _quizCount = value; OnPropertyChanged(); }
        }

        public int ExamCount
        {
            get => _examCount;
            set { _examCount = value; OnPropertyChanged(); }
        }
        #endregion

        #region Public Properties

        #endregion

        #region Constractor
        public AssessmentsPage()
        {
            InitializeComponent();

            this.DataContext = this;
            _ = LoadAssessments();
        }
        #endregion

        #region Event Handlers
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            AssessmentsView.Refresh();
        }

        private void btnAddNewAssessment_Click(object sender, RoutedEventArgs e)
        {
            var AddNewAssessmentWindow = new AddEditAssessmentWindow();
            AddNewAssessmentWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            AddNewAssessmentWindow.Owner = Application.Current.MainWindow;
            if (AddNewAssessmentWindow.ShowDialog() == true)
            {
                // If a new assessment was created, refresh the list
                _ = LoadAssessments();
            }
        }

        private void btnEditAssessment_Click(object sender, RoutedEventArgs e)
        {
            var EditAssessmentWindow = new AddEditAssessmentWindow(_selectedAssessment);
            EditAssessmentWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            EditAssessmentWindow.Owner = Application.Current.MainWindow;
            if (EditAssessmentWindow.ShowDialog() == true)
            {
                // If a new assessment was updated, refresh the list
                _ = LoadAssessments();
            }
        }

        private void btnToggleStatus_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Loading and Data Handling

        private async Task LoadAssessments()
        {
            Assessments = new ObservableCollection<AssessmentViewModel>(
                (await Assessment.GetAllAsync(1, 50, null)).assessments
                .Select(a => new AssessmentViewModel(a))
            );

            // Setup collection view with filtering
            AssessmentsView = CollectionViewSource.GetDefaultView(Assessments);
            AssessmentsView.Filter = FilterAssessment;

            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            if(_assessments == null) return;

            TotalAssessments = _assessments.Count;
            ActiveAssessments = _assessments.Count(a => a.IsActive);
            QuizCount = _assessments.Count(a => a.AssessmentType == enAssessmentType.Quiz);
            ExamCount = _assessments.Count(a => a.AssessmentType == enAssessmentType.TheoryExam);
        }
        private bool FilterAssessment(object obj)
        {
            if(obj is not AssessmentViewModel assessment)
                return false;

            // Active filter
            if (ShowActiveOnly && !assessment.IsActive)
                return false;

            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                var keyword = SearchKeyword.ToLower();
                if (!assessment.AssessmentName.ToLower().Contains(keyword) &&
                    !assessment.SpecializationName.ToLower().Contains(keyword) &&
                    !assessment.AssessmentType.ToString().ToLower().Contains(keyword))
                    return false;
            }

            // Type filter
            if (_filterType.HasValue && assessment.AssessmentType != _filterType.Value)
                return false;

            return true;
        }
        #endregion

    }
}
