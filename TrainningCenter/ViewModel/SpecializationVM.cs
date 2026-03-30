using System.ComponentModel;
using TrainingCenter_BusinessLayer;

namespace TrainningCenter.ViewModel
{
    public class SpecializationVM : INotifyPropertyChanged
    {
        private Specialization _model;
        public Specialization Model => _model;

        public SpecializationVM(Specialization model)
        {
            _model = model;
        }
        public string SpecializationName
        {
            get => _model.SpecializationName;
            set
            {
                _model.SpecializationName = value;
                OnPropertyChanged(nameof(SpecializationName));
            }
        }
        public string Description
        {
            get => _model.Description;
            set
            {
                _model.Description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
        public string Category
        {
            get => _model.Category;
            set
            {
                _model.Category = value;
                OnPropertyChanged(nameof(Category));
            }
        }
        public bool IsActive
        {
            get => (bool)_model.IsActive;
            set
            {
                _model.IsActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
        public string Price
        {
            get => _model.Price?.ToString();
            set
            {
                if (decimal.TryParse(value, out decimal priceValue))
                {
                    _model.Price = priceValue;
                }
                else
                {
                    _model.Price = null;
                }
                OnPropertyChanged(nameof(Price));
            }
        }
        public string Duration
        {
            get => _model.Duration?.ToString();
            set
            {
                if (int.TryParse(value, out int durationValue))
                {
                    _model.Duration = durationValue;
                }
                else
                {
                    _model.Duration = null;
                }
                OnPropertyChanged(nameof(Duration));
            }
        }
        public string Prepequisites
        {
            get => _model.Prerequisites;
            set
            {
                _model.Prerequisites = value;
                OnPropertyChanged(nameof(Prepequisites));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}

