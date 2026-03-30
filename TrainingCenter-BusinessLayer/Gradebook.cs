using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer.Helpers;
using TrainningCenter_DataAccessLayer;
using TrainningCenter_Entities;

namespace TrainingCenter_BusinessLayer
{
    public class Gradebook
    {
        /// <summary>
        /// Retrieves the gradebook and applies initial business rules
        /// </summary>
        public static async Task<List<GradebookItemDTO>> GetGradebookAsync(int assessmentID, int groupID)
        {
            if (assessmentID <= 0 || groupID <= 0)
                throw new ArgumentException("AssessmentID and GroupID must be positive integers.");

            var data = await clsStudentAssessments.GetGradebookAsync(assessmentID, groupID);

            // Business Rule: If a grade is already present, calculate the initial status
            foreach (var item in data)
            {
                // We reset IsDirty because loading from DB isn't a "change"
                item.IsDirty = false;
            }

            return data;
        }

        /// <summary>
        /// Validates and Saves a batch of grades
        /// </summary>
        public async Task<SaveResult> SaveGradebookAsync(IEnumerable<GradebookItemDTO> items, int evaluatorId)
        {
            var result = new SaveResult();
            var dirtyItems = items.Where(i => i.IsDirty).ToList();

            if (!dirtyItems.Any())
            {
                result.IsSuccess = true;
                return result;
            }

            foreach(var item in items)
            {
                try
                {
                    // 1. Business Validation
                    if (!ValidateGrade(item, out string error))
                    {
                        result.Errors.Add($"{item.StudentName}: {error}");
                        continue;
                    }

                    // 2. Automatic Status Logic (Optional Business Rule)
                    // Example: If marks are entered, set status to Passed/Failed automatically if it was "Pending"

                    if (item.MarksObtained.HasValue && item.Status == enAssessmentStatus.Pending)
                    {
                        item.Status = (item.Percentage >= 50) ? enAssessmentStatus.Passed : enAssessmentStatus.Failed;
                    }

                    // 3. Call DAL
                    int newId = await clsStudentAssessments.UpsertGradeAsync(item, evaluatorId);
                    item.StudentAssessmentID = newId;
                    item.IsDirty = false;
                    result.SavedCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error saving {item.StudentName}: {ex.Message}");
                    EventLogger.LogError($"Error saving {item.StudentName}", ex);
                }
            }

            result.IsSuccess = result.Errors.Count == 0;
            return result;
        }

        /// <summary>
        /// Individual validation rules
        /// </summary>
        private bool ValidateGrade(GradebookItemDTO item, out string error)
        {
            error = string.Empty;

            if (item.MarksObtained.HasValue)
            {
                if(item.MarksObtained < 0)
                {
                    error = "Marks cannot be negative.";
                    return false;
                }
                if (item.MarksObtained > item.TotalMarks)
                {
                    error = $"Marks cannot exceed the maximum allowed ({item.TotalMarks}).";
                    return false;
                }
            }

            //if (string.IsNullOrEmpty(item.Status))
            //{
            //    error = "Status is required.";
            //    return false;
            //}

            return true;
        }
    }
    public class SaveResult
    {
        public bool IsSuccess { get; set; }
        public int SavedCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
