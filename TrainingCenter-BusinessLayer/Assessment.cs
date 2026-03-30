using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer;
using TrainningCenter_Entities;

namespace TrainingCenter_BusinessLayer
{
    public class Assessment
    {
        public enum enMode { AddNew = 0, Update = 2 }

        #region Properties
        private enMode _Mode { get; set; }
        public int? AssessmentID { get; set; }
        public int? SpecializationID { get; set; }
        public string SpecializationName { get; set; }
        public string AssessmentName { get; set; }
        public enAssessmentType? AssessmentType { get; set; }
        public decimal? TotalMark { get; set; }
        public decimal? PassingMark { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Instructions { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        #endregion

        #region Constructors
        public Assessment()
        {
            this.AssessmentID = null;
            this.SpecializationID = null;
            this.SpecializationName = null;
            this.AssessmentName = string.Empty;
            this.AssessmentType = enAssessmentType.Presentation;
            this.TotalMark = 0;
            this.PassingMark = 0;
            this.Duration = null;
            this.Instructions = string.Empty;
            this.CreatedBy = null;
            this.CreatedByName = null;
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
            this.IsActive = true;

            this._Mode = enMode.AddNew;
        }
        private Assessment(AssessmentsDTO dto)
        {
            this.AssessmentID = dto.AssessmentID;
            this.SpecializationID = dto.SpecializationID;
            this.SpecializationName = dto.SpecializationName;
            this.AssessmentName = dto.AssessmentName;
            this.AssessmentType = dto.AssessmentType;
            this.TotalMark = dto.TotalMark;
            this.PassingMark = dto.PassingMark;
            this.Duration = dto.Duration;
            this.Instructions = dto.Instructions;
            this.CreatedBy = dto.CreatedBy;
            this.CreatedByName = dto.CreatedByName;
            this.CreatedAt = dto.CreatedAt;
            this.UpdatedAt = dto.UpdatedAt;
            this.IsActive = dto.IsActive;

            this._Mode = enMode.Update;
        }
        // Constructor for update DTO
        public AssessmentUpdateDTO ToUpdateDTO(int modifiedByUserID)
        {
            return new AssessmentUpdateDTO(
                assessmentID: this.AssessmentID.Value,
                specializationID: this.SpecializationID,
                assessmentName: this.AssessmentName,
                assessmentType: this.AssessmentType,
                totalMark: this.TotalMark,
                passingMark: this.PassingMark,
                duration: this.Duration,
                instructions: this.Instructions,
                modifiedByUserID: modifiedByUserID
            );

        }

        // Constructor for Create DTO
        public AssessmentCreationDTO ToCreateDTO()
        {
            return new AssessmentCreationDTO(
                specializationID: this.SpecializationID.Value,
                assessmentName: this.AssessmentName,
                assessmentType: this.AssessmentType.Value,
                totalMark: this.TotalMark.Value,
                passingMark: this.PassingMark.Value,
                duration: this.Duration,
                instructions: this.Instructions,
                createdBy: this.CreatedBy.Value
            );
        }
        #endregion

        #region CRUD Operations

        /// <summary>
        /// Find assessment by ID
        /// </summary>
        public static async Task<Assessment> Find(int assessmentID)
        {
            if (assessmentID < 0) return null;

            AssessmentsDTO dto = await clsAssessment.GetAssessmentByID(assessmentID);
            if (dto == null)
                return null;
            return new Assessment(dto);
        }

        /// <summary>
        /// Find assessment by Name
        /// </summary>
        public static async Task<Assessment> Find(string assessmentName)
        {
            if (string.IsNullOrEmpty(assessmentName)) return null;

            AssessmentsDTO dto = await clsAssessment.GetAssessmentByAssessmentName(assessmentName);
            if (dto == null)
                return null;

            return new Assessment(dto);
        }

        private async Task<(bool success, string message)> _AddNewAsync()
        {
            if (!Validate())
                return (false, "Validation failed. Please check the input data.");

            var createDTO = ToCreateDTO();
            var (insertedID, status, message) = await clsAssessment.AddNewAssessment(createDTO);

            if (insertedID.HasValue && insertedID.Value > 0)
            {
                this.AssessmentID = insertedID.Value;
                this._Mode = enMode.Update; // Switch to update mode after successful creation

                var loaded = await Find(insertedID.Value);
                if (!(loaded is null))
                {
                    CopyFrom(loaded);
                }

                return (true, message);
            }

            return (false, message);
        }
        private async Task<(bool success, string message)> _UpdateAsync()
        {
            if (!Validate())
                return (false, "Validation failed. Please check the input data.");
            if (!this.AssessmentID.HasValue)
                return (false, "Assessment ID is required for update.");

            var updateDTO = ToUpdateDTO(modifiedByUserID: this.CreatedBy.Value);

            var (isSuccess, status, message, updatedDTO) = await clsAssessment.UpdateAssessment(updateDTO);

            if (isSuccess && status == "Success")
            {
                if (updatedDTO != null)
                {
                    // Update the current instance with the latest data from the database
                    var updated = new Assessment(updatedDTO);
                    CopyFrom(updated);
                }

                return (true, message);
            }
            else if (status == "No Changes")
            {
                return (true, message); // No changes is still success
            }

            return (false, message);
        }
        /// <summary>
        /// Save current assessment (Add or Update)
        /// </summary>
        public async Task<(bool success, string message)> SaveAsync(int? modifiedByUserID = null)
        {
            switch (_Mode)
            {
                case enMode.AddNew:
                    {
                        var result = await _AddNewAsync();

                        // Log ONLY if the database insert actually worked
                        if (result.success && modifiedByUserID.HasValue)
                        {
                            // Use the newly generated ID from 'this.AssessmentID'
                            await Task.Run(() => clsSystemLogData.LogAction(
                                modifiedByUserID.Value,
                                "ADDNEW",
                                "Assessments",
                                this.AssessmentID ?? 0,
                                "Added new assessment record"));
                        }
                        return result;
                    }

                case enMode.Update:
                    if (modifiedByUserID is null)
                        return (false, "Modified By User ID is required for update");

                    var updateResult = await _UpdateAsync();

                    if (updateResult.success)
                    {
                        await Task.Run(() => clsSystemLogData.LogAction(
                            modifiedByUserID.Value,
                            "UPDATE",
                            "Assessments",
                            this.AssessmentID.Value,
                            "Updated existing assessment details"));
                    }
                    return updateResult;

                default:
                    return (false, "Invalid mode. Cannot save the assessment.");
            }
        }

        public async Task<(bool success, string message)> DeactivateAsync()
        {
            if (this.AssessmentID.HasValue)
            {
                var (isDeleted, status, message) = await clsAssessment.DeleteAssessment(this.AssessmentID.Value);
                return (isDeleted, message);
            }

            return (false, "Assessment ID is required for deactivation.");
        }
        #endregion

        #region Helpers
        private void CopyFrom(Assessment other)
        {
            this.AssessmentID = other.AssessmentID;
            this.SpecializationID = other.SpecializationID;
            this.SpecializationName = other.SpecializationName;
            this.AssessmentName = other.AssessmentName;
            this.AssessmentType = other.AssessmentType;
            this.TotalMark = other.TotalMark;
            this.PassingMark = other.PassingMark;
            this.Duration = other.Duration;
            this.Instructions = other.Instructions;
            this.CreatedBy = other.CreatedBy;
            this.CreatedByName = other.CreatedByName;
            this.CreatedAt = other.CreatedAt;
            this.UpdatedAt = other.UpdatedAt;
            this.IsActive = other.IsActive;
            this._Mode = other._Mode;
        }

        private bool Validate()
        {
            if (!this.SpecializationID.HasValue || this.SpecializationID <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(this.AssessmentName))
                return false;

            if (!this.AssessmentType.HasValue)
                return false;

            if (!this.TotalMark.HasValue || this.TotalMark <= 0)
                return false;

            if (!this.PassingMark.HasValue || this.PassingMark <= 0)
                return false;

            if (this.PassingMark > this.TotalMark)
                return false;

            if (this.Duration.HasValue)
            {
                if (this.Duration.Value < TimeSpan.FromMinutes(5) ||
                    this.Duration.Value > TimeSpan.FromHours(4))
                    return false;
            }

            if (_Mode == enMode.AddNew && (!this.CreatedBy.HasValue || this.CreatedBy <= 0))
                return false;

            return true;
        }
        #endregion

        #region Static Methods 
        /// <summary>
        /// Get all assessments with pagination
        /// </summary>
        public static async Task<(List<Assessment> assessments, int totalRecords)> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 50,
            bool? isActive = true)
        {
            var paginatedResult = await clsAssessment.GetAllAssessments(isActive, pageNumber, pageSize);

            var assessments = new List<Assessment>();
            foreach (var dto in paginatedResult.Items)
            {
                assessments.Add(new Assessment(dto));
            }

            return (assessments, paginatedResult.TotalCount);
        }
        /// <summary>
        /// Get assessments by specialization
        /// </summary>
        public static async Task<List<Assessment>> GetAllBySpecializationIDAsync(int specializationID, bool? isActive = null)
        {
            var dtos = await clsAssessment.GetAllAssessmentsBySpecializationId(specializationID, isActive);
            var assessments = new List<Assessment>();
            foreach (var dto in dtos.Items)
            {
                assessments.Add(new Assessment(dto));
            }
            return assessments;
        }
        #endregion

        #region Business Logic Methods
        /// <summary>
        /// Check if assessment is available (active and not expired)
        /// </summary>
        public bool IsAvailable()
        {
            if (!this.IsActive)
                return false;
            if (this.Duration.HasValue)
            {
                var expirationTime = this.CreatedAt.Add(this.Duration.Value);
                if (DateTime.Now > expirationTime)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if student passed based on their score
        /// </summary>
        public bool IsPassed(decimal studentScore)
        {
            if (!this.PassingMark.HasValue || !this.TotalMark.HasValue)
                return false;

            if (!this.IsAvailable())
                return false;
            return studentScore >= this.PassingMark;
        }
        #endregion
    }
}
