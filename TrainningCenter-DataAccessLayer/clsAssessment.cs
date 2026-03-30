using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer.Helpers;
using TrainningCenter_Entities;

namespace TrainningCenter_DataAccessLayer
{
    public static class clsAssessment
    {
        public static async Task<PaginatedResult<AssessmentsDTO>> GetAllAssessments
            (bool? isActive = null, int pageSize = 50, int pageNumber = 1)
        {
            PaginatedResult<AssessmentsDTO> paginatedResult = new PaginatedResult<AssessmentsDTO>()
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllAssessments", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@IsActive", System.Data.SqlDbType.Bit).Value =
                        isActive.HasValue ? isActive.Value : DBNull.Value;
                    command.Parameters.Add("@PageNumber", System.Data.SqlDbType.Int).Value = pageNumber;
                    command.Parameters.Add("@PageSize", System.Data.SqlDbType.Int).Value = pageSize;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            int assessmentIDIndex = reader.GetOrdinal("AssessmentID");
                            int specializationIDIndex = reader.GetOrdinal("SpecializationID");
                            int specializationNameIndex = reader.GetOrdinal("SpecializationName");
                            int assessmentTypeIndex = reader.GetOrdinal("AssessmentType");
                            int assessmentNameIndex = reader.GetOrdinal("AssessmentName");
                            int totalMarksIndex = reader.GetOrdinal("TotalMarks");
                            int passingMarksIndex = reader.GetOrdinal("PassingMarks");
                            int durationIndex = reader.GetOrdinal("Duration");
                            int instructionsIndex = reader.GetOrdinal("Instructions");
                            int createdByIndex = reader.GetOrdinal("CreatedBy");
                            int userNameIndex = reader.GetOrdinal("UserName");
                            int createdAtIndex = reader.GetOrdinal("CreatedAt");
                            int updatedAtIndex = reader.GetOrdinal("UpdatedAt");
                            int isActiveIndex = reader.GetOrdinal("IsActive");
                            int totalCountIndex = reader.GetOrdinal("TotalCount");

                            bool isTotalCountSet = false;

                            while (await reader.ReadAsync())
                            {

                                // Set total count from the first row (using the SQL OVER() window function result)
                                if (!isTotalCountSet)
                                {
                                    paginatedResult.TotalCount = reader.GetInt32(totalCountIndex);
                                    isTotalCountSet = true;
                                }

                                paginatedResult.Items.Add(new AssessmentsDTO(
                                    reader.GetInt32(assessmentIDIndex),
                                    reader.GetInt32(specializationIDIndex),
                                    reader.GetString(specializationNameIndex),
                                    AssessmentsDTO.StringToEnumMapping(reader.GetString(assessmentTypeIndex)),
                                    reader.GetString(assessmentNameIndex),
                                    reader.GetDecimal(totalMarksIndex),
                                    reader.GetDecimal(passingMarksIndex),
                                    reader.GetTimeSpan(durationIndex),
                                    reader.GetString(instructionsIndex),
                                    reader.GetInt32(createdByIndex),
                                    reader.GetString(userNameIndex),
                                    reader.GetDateTime(createdAtIndex),
                                    updatedAt: reader.IsDBNull(updatedAtIndex)
                                        ? (DateTime?)null
                                        : reader.GetDateTime(updatedAtIndex),
                                    reader.GetBoolean(isActiveIndex)
                                    )
                                );
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"SQL Error in GetAllAssessments: {sqlEx.Message}", sqlEx);
                        throw new ApplicationException($"Database error while loading assessments. Error: {sqlEx.Message}", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error Loadding the Assessments Data", ex);
                        throw new ApplicationException("Error Loadding the Assessments Data", ex);
                    }
                }
            }


            return paginatedResult;
        }
        public static async Task<PaginatedResult<AssessmentsDTO>> GetAllAssessmentsBySpecializationId
            (int specializationId, bool? isActive = null, int pageSize = 50, int pageNumber = 1)
        {
            if (specializationId <= 0) return null;

            PaginatedResult<AssessmentsDTO> paginatedResult = new PaginatedResult<AssessmentsDTO>()
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAssessmentsBySpecialization", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@SpecializationID", System.Data.SqlDbType.Int).Value = specializationId;
                    command.Parameters.Add("@IsActive", System.Data.SqlDbType.Bit).Value =
                        isActive.HasValue ? isActive.Value : DBNull.Value;
                    command.Parameters.Add("@PageNumber", System.Data.SqlDbType.Int).Value = pageNumber;
                    command.Parameters.Add("@PageSize", System.Data.SqlDbType.Int).Value = pageSize;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            int assessmentIDIndex = reader.GetOrdinal("AssessmentID");
                            int specializationIDIndex = reader.GetOrdinal("SpecializationID");
                            int specializationNameIndex = reader.GetOrdinal("SpecializationName");
                            int assessmentTypeIndex = reader.GetOrdinal("AssessmentType");
                            int assessmentNameIndex = reader.GetOrdinal("AssessmentName");
                            int totalMarksIndex = reader.GetOrdinal("TotalMarks");
                            int passingMarksIndex = reader.GetOrdinal("PassingMarks");
                            int durationIndex = reader.GetOrdinal("Duration");
                            int instructionsIndex = reader.GetOrdinal("Instructions");
                            int createdByIndex = reader.GetOrdinal("CreatedBy");
                            int userNameIndex = reader.GetOrdinal("UserName");
                            int createdAtIndex = reader.GetOrdinal("CreatedAt");
                            int updatedAtIndex = reader.GetOrdinal("UpdatedAt");
                            int isActiveIndex = reader.GetOrdinal("IsActive");
                            int totalCountIndex = reader.GetOrdinal("TotalCount");

                            bool isTotalCountSet = false;

                            while (await reader.ReadAsync())
                            {
                                if (!reader.HasRows)
                                    return paginatedResult;

                                // Set total count from the first row (using the SQL OVER() window function result)
                                if (!isTotalCountSet)
                                {
                                    paginatedResult.TotalCount = reader.GetInt32(totalCountIndex);
                                    isTotalCountSet = true;
                                }

                                paginatedResult.Items.Add(new AssessmentsDTO(
                                    reader.GetInt32(assessmentIDIndex),
                                    reader.GetInt32(specializationIDIndex),
                                    reader.GetString(specializationNameIndex),
                                    AssessmentsDTO.StringToEnumMapping(reader.GetString(assessmentTypeIndex)),
                                    reader.GetString(assessmentNameIndex),
                                    reader.GetDecimal(totalMarksIndex),
                                    reader.GetDecimal(passingMarksIndex),
                                    reader.GetTimeSpan(durationIndex),
                                    reader.GetString(instructionsIndex),
                                    reader.GetInt32(createdByIndex),
                                    reader.GetString(userNameIndex),
                                    reader.GetDateTime(createdAtIndex),
                                    reader.GetDateTime(updatedAtIndex),
                                    reader.GetBoolean(isActiveIndex)
                                    )
                                );
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"SQL Error in GetAllAssessments: {sqlEx.Message}", sqlEx);
                        throw new ApplicationException($"Database error while loading assessments. Error: {sqlEx.Message}", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error Loadding the Assessments Data", ex);
                        throw new ApplicationException("Error Loadding the Assessments Data", ex);
                    }
                }
            }


            return paginatedResult;
        }
        public static async Task<AssessmentsDTO> GetAssessmentByID(int AssessmentID)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAssessmentByID", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@AssessmentID", System.Data.SqlDbType.Int).Value = AssessmentID;
                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new AssessmentsDTO(
                                    reader.GetInt32(reader.GetOrdinal("AssessmentID")),
                                    reader.GetInt32(reader.GetOrdinal("SpecializationID")),
                                    reader.GetString(reader.GetOrdinal("SpecializationName")),
                                    AssessmentsDTO.StringToEnumMapping(reader.GetString(reader.GetOrdinal("AssessmentType"))),
                                    reader.GetString(reader.GetOrdinal("AssessmentName")),
                                    reader.GetDecimal(reader.GetOrdinal("TotalMarks")),
                                    reader.GetDecimal(reader.GetOrdinal("PassingMarks")),
                                    reader.GetTimeSpan(reader.GetOrdinal("Duration")),
                                    reader.GetString(reader.GetOrdinal("Instructions")),
                                    reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                                    reader.GetString(reader.GetOrdinal("UserName")),
                                    reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                                    reader.GetBoolean(reader.GetOrdinal("IsActive"))
                                );
                            }

                            return null; // No record found with the given AssessmentID
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"SQL Error in GetAssessmentByID: {sqlEx.Message}", sqlEx);
                        throw new ApplicationException($"Database error while loading assessment. Error: {sqlEx.Message}", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error Loadding the Assessment Data", ex);
                        throw new ApplicationException("Error Loadding the Assessment Data", ex);
                    }
                }
            }
        }
        public static async Task<AssessmentsDTO> GetAssessmentByAssessmentName(string AssessmentName)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAssessmentByName", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@AssessmentName", System.Data.SqlDbType.NVarChar).Value = AssessmentName;
                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new AssessmentsDTO(
                                    reader.GetInt32(reader.GetOrdinal("AssessmentID")),
                                    reader.GetInt32(reader.GetOrdinal("SpecializationID")),
                                    reader.GetString(reader.GetOrdinal("SpecializationName")),
                                    AssessmentsDTO.StringToEnumMapping(reader.GetString(reader.GetOrdinal("AssessmentType"))),
                                    reader.GetString(reader.GetOrdinal("AssessmentName")),
                                    reader.GetDecimal(reader.GetOrdinal("TotalMarks")),
                                    reader.GetDecimal(reader.GetOrdinal("PassingMarks")),
                                    reader.GetTimeSpan(reader.GetOrdinal("Duration")),
                                    reader.GetString(reader.GetOrdinal("Instructions")),
                                    reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                                    reader.GetString(reader.GetOrdinal("UserName")),
                                    reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                                    reader.GetBoolean(reader.GetOrdinal("IsActive"))
                                );
                            }

                            return null; // No record found with the given AssessmentID
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"SQL Error in GetAssessmentByID: {sqlEx.Message}", sqlEx);
                        throw new ApplicationException($"Database error while loading assessment. Error: {sqlEx.Message}", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error Loadding the Assessment Data", ex);
                        throw new ApplicationException("Error Loadding the Assessment Data", ex);
                    }
                }
            }
        }

        public static async Task<(int? insertedID, string status, string message)> AddNewAssessment(AssessmentCreationDTO assessment)
        {
            // Validate input
            if (assessment == null)
                return (null, "Error", "Assessment data cannot be null");

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewAssessment", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Output parameter
                    SqlParameter newIdParam = new SqlParameter
                    {
                        ParameterName = "@NewAssessmentID",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(newIdParam);

                    // Input parameters
                    command.Parameters.AddWithValue("@SpecializationID", assessment.SpecializationID);
                    command.Parameters.AddWithValue("@AssessmentName", assessment.AssessmentName);

                    // Convert enum to string for database
                    string assessmentTypeString = AssessmentsDTO.FromEnumToString(assessment.AssessmentType);
                    command.Parameters.AddWithValue("@AssessmentType", assessmentTypeString);

                    command.Parameters.AddWithValue("@TotalMark", assessment.TotalMark);
                    command.Parameters.AddWithValue("@PassingMark", assessment.PassingMark);

                    // Handle nullable Duration
                    if (assessment.Duration.HasValue)
                        command.Parameters.AddWithValue("@Duration", assessment.Duration.Value);
                    else
                        command.Parameters.AddWithValue("@Duration", DBNull.Value);

                    // Handle nullable Instructions
                    if (!string.IsNullOrWhiteSpace(assessment.Instructions))
                        command.Parameters.AddWithValue("@Instractions", assessment.Instructions);
                    else
                        command.Parameters.AddWithValue("@Instractions", DBNull.Value);

                    command.Parameters.AddWithValue("@CreatedByUserID", assessment.CreatedBy);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        // Get the output value
                        int? newId = newIdParam.Value != DBNull.Value
                            ? (int)newIdParam.Value
                            : (int?)null;

                        return (newId, "Success", "Assessment created successfully");
                    }
                    catch (SqlException ex)
                    {
                        // Handle specific SQL errors based on error numbers
                        string errorMessage = ex.Number switch
                        {
                            52001 => "Invalid Specialization ID",
                            52002 => "Specialization not found",
                            52003 => "Assessment name is required",
                            52004 => "Assessment name already exists",
                            52005 => "Invalid assessment type",
                            52006 => "Invalid total mark",
                            52007 => "Invalid passing mark",
                            52008 => "Passing mark exceeds total mark",
                            52009 => "Invalid duration",
                            52010 => "Invalid user ID",
                            52011 => "User not found",
                            52099 => "Database error occurred",
                            _ => $"SQL Error: {ex.Message}"
                        };

                        EventLogger.LogError($"Error adding assessment: {errorMessage}", ex);
                        return (null, "Error", errorMessage);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error Adding New Assessment", ex);
                        return (null, "Error", "Error Adding New Assessment");
                    }
                }
            }
        }
        public static async Task<(bool isSuccess, string status, string message, AssessmentsDTO updatedAssessment)>
            UpdateAssessment(AssessmentUpdateDTO assessment)
        {
            // ================ INPUT VALIDATION ================
            if (assessment == null)
                return (false, "Error", "Assessment data cannot be null", null);

            if (assessment.AssessmentID <= 0)
                return (false, "Error", "Invalid Assessment ID", null);

            if (assessment.ModifiedByUserID <= 0)
                return (false, "Error", "Valid modifier user ID is required", null);

            // Validate at least one field to update
            if (
                assessment.SpecializationID is null &&
                string.IsNullOrWhiteSpace(assessment.AssessmentName) &&
                assessment.AssessmentType is null &&
                assessment.TotalMark is null &&
                assessment.PassingMark is null &&
                assessment.Duration is null &&
                assessment.Instructions is null)
            {
                return (false, "Warning", "No fields provided to update", null);
            }

            if (assessment.AssessmentType.HasValue)
            {
                string[] validTypes = { "Presentation", "Quiz", "Assignment", "Project", "Practical Test", "Theory Exam" };
                string typeString = assessment.AssessmentType.Value.ToString();
                if (!validTypes.Contains(typeString))
                {
                    return (false, "Error",
                        $"Invalid assessment type. Must be one of: {string.Join(", ", validTypes)}", null);
                }
            }

            // Validate marks if both provided
            if (assessment.TotalMark.HasValue && assessment.PassingMark.HasValue)
            {
                if (assessment.TotalMark <= 0)
                    return (false, "Error", "Total mark must be greater than 0", null);

                if (assessment.PassingMark <= 0)
                    return (false, "Error", "Passing mark must be greater than 0", null);

                if (assessment.PassingMark > assessment.TotalMark)
                    return (false, "Error",
                        $"Passing mark ({assessment.PassingMark}) cannot exceed total mark ({assessment.TotalMark})", null);
            }
            else if (assessment.TotalMark.HasValue && assessment.TotalMark <= 0)
            {
                return (false, "Error", "Total mark must be greater than 0", null);
            }
            else if (assessment.PassingMark.HasValue && assessment.PassingMark <= 0)
            {
                return (false, "Error", "Passing mark must be greater than 0", null);
            }

            // ================ DATABASE OPERATION ================
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateAssessment", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Required parameters
                    command.Parameters.AddWithValue("@AssessmentID", assessment.AssessmentID);
                    command.Parameters.AddWithValue("@ModifiedByUserID", assessment.ModifiedByUserID);

                    // Optional parameters - pass DBNull if not provided
                    command.Parameters.AddWithValue("@SpecializationID",
                        assessment.SpecializationID ?? (object)DBNull.Value);

                    command.Parameters.AddWithValue("@AssessmentName",
                        string.IsNullOrWhiteSpace(assessment.AssessmentName) ?
                        DBNull.Value : assessment.AssessmentName.Trim());

                    command.Parameters.AddWithValue("@AssessmentType",
                        assessment.AssessmentType.HasValue ?
                        assessment.AssessmentType.Value.ToString() : DBNull.Value);

                    command.Parameters.AddWithValue("@TotalMark",
                        assessment.TotalMark ?? (object)DBNull.Value);

                    command.Parameters.AddWithValue("@PassingMark",
                        assessment.PassingMark ?? (object)DBNull.Value);

                    command.Parameters.AddWithValue("@Duration",
                        assessment.Duration ?? (object)DBNull.Value);

                    command.Parameters.AddWithValue("@Instructions",
                        string.IsNullOrWhiteSpace(assessment.Instructions) ?
                        DBNull.Value : assessment.Instructions.Trim());

                    try
                    {
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            // Read the result set
                            if (await reader.ReadAsync())
                            {
                                string status = reader.GetString(reader.GetOrdinal("Status"));
                                string message = reader.GetString(reader.GetOrdinal("Message"));

                                if (status == "No Changes")
                                {
                                    return (true, status, message, null);
                                }

                                // If Success Read the updated assessment
                                if (status == "Success" && reader.HasRows)
                                {
                                    AssessmentsDTO updated = new AssessmentsDTO
                                    {
                                        AssessmentID = reader.GetInt32(reader.GetOrdinal("AssessmentID")),
                                        SpecializationID = reader.GetInt32(reader.GetOrdinal("SpecializationID")),
                                        AssessmentName = reader.GetString(reader.GetOrdinal("AssessmentName")),
                                        AssessmentType = AssessmentsDTO.StringToEnumMapping(reader.GetString(reader.GetOrdinal("AssessmentType"))),
                                        TotalMark = reader.GetDecimal(reader.GetOrdinal("TotalMarks")),
                                        PassingMark = reader.GetDecimal(reader.GetOrdinal("PassingMarks")),
                                        Duration = reader.IsDBNull(reader.GetOrdinal("Duration"))
                                        ? (TimeSpan?)null
                                        : reader.GetTimeSpan(reader.GetOrdinal("Duration"))
                                    };

                                    return (true, status, message, updated);
                                }
                                return (true, status, message, null);
                            }

                            return (false, "Error", "No response from database", null);
                        }
                    }
                    catch (SqlException ex)
                    {
                        // Handle specific SQL errors based on error numbers
                        string errorMessage = ex.Number switch
                        {
                            53001 => "Assessment not found",
                            53003 => "Modified By User does not exist",
                            53004 => "Specialization ID must be a positive value",
                            53005 => "Specialization does not exist",
                            53006 => "Assessment Name cannot be empty",
                            53007 => $"Assessment with name '{assessment.AssessmentName}' already exists in this specialization",
                            53008 => "Invalid assessment type",
                            53009 => "Total Mark must be greater than 0",
                            53010 => "Passing Mark must be greater than 0",
                            53011 => $"Passing mark cannot exceed total mark",
                            53012 => "Duration must be between 5 minutes and 4 hours",
                            53013 => "Cannot update: Foreign key violation",
                            53014 => "Cannot update: Duplicate assessment name",
                            53099 => "Database error occurred",
                            _ => $"SQL Error: {ex.Message}"
                        };

                        EventLogger.LogError($"Error updating assessment: {errorMessage}", ex);
                        return (false, "Error", errorMessage, null);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error updating assessment", ex);
                        return (false, "Error", "Error updating assessment", null);
                    }
                }
            }
        }

        public static async Task<(bool isDeleted, string status, string message)> DeleteAssessment(int assessmentID)
        {
            if (assessmentID <= 0) return (false, "Error", "Assessment Id Must Positive non-null value");
            using(SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using(SqlCommand command = new SqlCommand("SP_DeleteAssessment", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@AssessmentID", SqlDbType.Int).Value = assessmentID;

                    try
                    {
                        await connection.OpenAsync();
                        using(SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            // Read the result set
                            if (await reader.ReadAsync())
                            {
                                int success = reader.GetInt32(reader.GetOrdinal("Success"));
                                string message = reader.GetString(reader.GetOrdinal("Message"));

                                if(success == 1)
                                {
                                    // Optional: Get additional info
                                    int oldStatus = reader.GetInt32(reader.GetOrdinal("OldStatus"));
                                    int deletedID = reader.GetInt32(reader.GetOrdinal("AssessmentID"));

                                    // Log the soft delete
                                    EventLogger.LogInfo($"Assessment {deletedID} soft deleted. Old status: {oldStatus}");

                                    return (true, "Success", message);
                                }

                                return (false, "Error", message);
                            }

                            return (false, "Error", "No response from database");
                        }
                    }
                    catch (SqlException ex) when (ex.Number == 56001 || ex.Number == 56002 || ex.Number == 56099)
                    {
                        string errorMessage = ex.Number switch
                        {
                            56001 => "Invalid Assessment ID",
                            56002 => "Assessment not found",
                            56099 => "Database error occurred during delete",
                            _ => ex.Message
                        };

                        EventLogger.LogError($"Error deleting assessment {assessmentID}: {errorMessage}", ex);
                        return (false, "Error", errorMessage);
                    }
                    catch (SqlException ex)
                    {
                        EventLogger.LogError($"SQL Error deleting assessment {assessmentID}: {ex.Message}", ex);
                        return (false, "Error", "A database error occurred");
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Unexpected error deleting assessment {assessmentID}", ex);
                        return (false, "Error", "An unexpected error occurred");
                    }
                }
            }
        }
    }
}
