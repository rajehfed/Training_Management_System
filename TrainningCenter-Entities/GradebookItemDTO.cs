using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter_Entities
{
    public enum enAssessmentStatus
    {
        Pending,
        Passed,
        Failed,
        Absent
    }
    // This prevents sending all 30+ students back to the database when only 2 grades were changed.
    public class GradebookItemDTO : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // Also notify calculated properties
            if (propertyName == nameof(MarksObtained))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Percentage)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Grade)));
            }
            return true;
        }
        #endregion

        private decimal? _marksObtained;
        private enAssessmentStatus _status;
        private string _feedback;
        private bool _isDirty;

        // IDs
        public int? StudentAssessmentID { set; get; }
        public int StudentID { set; get; }
        public int AssessmentID { set; get; }

        // Student Info (Read Only)
        public string StudentName { set; get; }
        public string StudentNumber { get; set; }

        // Assessment Info (Read Only)
        public string AssessmentName { get; set; }
        public decimal TotalMarks { get; set; }

        // Editable Fields
        public decimal? MarksObtained
        {
            get => _marksObtained;
            set
            {
                if (value > TotalMarks)
                {
                    // Optional: You could throw an error or just cap it
                    value = TotalMarks;
                }
                if (SetProperty(ref _marksObtained, value))
                {
                    MarkDirty();
                    // Optional Business Rule: Auto-calculate Status
                    if (value.HasValue)
                    {
                        Status = (Percentage >= 50) ? enAssessmentStatus.Passed : enAssessmentStatus.Failed;
                    }
                }
            }
        }

        public enAssessmentStatus Status
        {
            get => _status;
            set { if (SetProperty(ref _status, value)) MarkDirty(); }
        }

        public string Feedback
        {
            get => _feedback;
            set { if (SetProperty(ref _feedback, value)) MarkDirty(); }
        }

        public DateTime AssessmentDate { get; set; }

        // Change Tracking
        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }
        // UI Helpers
        public decimal Percentage => TotalMarks > 0 ? ((MarksObtained ?? 0) / TotalMarks) * 100 : 0;
        private void MarkDirty() => IsDirty = true;
        public string Grade => CalculateGrade(Percentage);
        private string CalculateGrade(decimal percent)
        {
            if (percent >= 90) return "A+";
            if (percent >= 80) return "A";
            if (percent >= 70) return "B";
            if (percent >= 60) return "C";
            if (percent >= 50) return "D";
            return "F";
        }
        public static string GetStatusDisplay(enAssessmentStatus status) => status.ToString();
        public static enAssessmentStatus ParseStatus(string status)
        {
            if (Enum.TryParse(status, out enAssessmentStatus result))
                return result;
            throw new ArgumentException($"Invalid status value: {status}");
        }
    }
}
