using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter_Entities
{
    public enum enAssessmentType
    {
        Presentation = 0,
        Quiz = 1,
        Assignment = 2,
        Project = 3,
        PracticalTest = 4,
        TheoryExam = 5
    };
    public class AssessmentsDTO
    {
        public int? AssessmentID { get; set; }
        public int SpecializationID { get; set; }
        public string SpecializationName { get; set; }
        public string AssessmentName { get; set; }
        public enAssessmentType AssessmentType { get; set; }
        public decimal TotalMark { get; set; }
        public decimal PassingMark { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Instructions { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        public AssessmentsDTO()
        {
        }
        public AssessmentsDTO(
            int? assessmentID,
            int specializationID,
            string specializationName,
            enAssessmentType assessmentType,
            string assessmentName,
            decimal totalMark,
            decimal passingMark,
            TimeSpan? duration,
            string instructions,
            int? createdBy,
            string createdByName,
            DateTime createdAt,
            DateTime? updatedAt,
            bool isActive
        )
        {
            AssessmentID = assessmentID;
            SpecializationID = specializationID;
            SpecializationName = specializationName;
            AssessmentType = assessmentType;
            AssessmentName = assessmentName;
            TotalMark = totalMark;
            PassingMark = passingMark;
            Duration = duration;
            Instructions = instructions;
            CreatedBy = createdBy;
            CreatedByName = createdByName;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            IsActive = isActive;
        }

        public static enAssessmentType StringToEnumMapping(string assessmentType)
        {
            switch (assessmentType)
            {
                case "Presentation":
                    return enAssessmentType.Presentation;

                case "Quiz":
                    return enAssessmentType.Quiz;

                case "Assignment":
                    return enAssessmentType.Assignment;

                case "Project":
                    return enAssessmentType.Project;

                case "Practical Test":
                    return enAssessmentType.PracticalTest;

                case "Theory Exam":
                    return enAssessmentType.TheoryExam;

                default:
                    return enAssessmentType.TheoryExam;
            }
        }
        public static string FromEnumToString(enAssessmentType assessmentType)
        {
            switch (assessmentType)
            {
                case enAssessmentType.Assignment: return "Assignment";
                case enAssessmentType.Presentation: return "Presentation";
                case enAssessmentType.Project: return "Project";
                case enAssessmentType.PracticalTest: return "Practical Test";
                case enAssessmentType.TheoryExam: return "Theory Exam";

                default: return "Theory Exam";
            }
        }
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;

        public PaginatedResult()
        {
            Items = new List<T>();
        }
    }

    public class AssessmentCreationDTO
    {
        public int SpecializationID { get; set; }
        public string AssessmentName { get; set; }
        public enAssessmentType AssessmentType { get; set; }
        public decimal TotalMark { get; set; }
        public decimal PassingMark { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Instructions { get; set; }
        public int CreatedBy { get; set; }
        public AssessmentCreationDTO(
            int specializationID,
            string assessmentName,
            enAssessmentType assessmentType,
            decimal totalMark,
            decimal passingMark,
            TimeSpan? duration,
            string instructions,
            int createdBy
        )
        {
            SpecializationID = specializationID;
            AssessmentName = assessmentName;
            AssessmentType = assessmentType;
            TotalMark = totalMark;
            PassingMark = passingMark;
            Duration = duration;
            Instructions = instructions;
            CreatedBy = createdBy;
        }
    }

    public class AssessmentUpdateDTO
    {
        public int? AssessmentID { get; set; }
        public int? SpecializationID { get; set; }
        public string AssessmentName { get; set; }
        public enAssessmentType? AssessmentType { get; set; }
        public decimal? TotalMark { get; set; }
        public decimal? PassingMark { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Instructions { get; set; }
        public int? ModifiedByUserID { get; set; }

        public AssessmentUpdateDTO(
            int? assessmentID,
            int? specializationID = null,
            string assessmentName = null,
            enAssessmentType? assessmentType = null,
            decimal? totalMark = null,
            decimal? passingMark = null,
            TimeSpan? duration = null,
            string instructions = null,
            int? modifiedByUserID = null)
        {
            AssessmentID = assessmentID;
            SpecializationID = specializationID;
            AssessmentName = assessmentName;
            AssessmentType = assessmentType;
            TotalMark = totalMark;
            PassingMark = passingMark;
            Duration = duration;
            Instructions = instructions;
            ModifiedByUserID = modifiedByUserID;
        }
    }
}
