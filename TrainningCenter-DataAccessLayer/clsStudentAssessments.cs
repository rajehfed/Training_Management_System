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
    public static class clsStudentAssessments
    {
        public static async Task<List<GradebookItemDTO>> GetGradebookAsync(int assessmentID, int groupID)
        {
            var result = new List<GradebookItemDTO>();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_StudentAssessments_GetGradebook", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@AssessmentID", System.Data.SqlDbType.Int).Value = assessmentID;
                    command.Parameters.Add("@GroupID", System.Data.SqlDbType.Int).Value = groupID;

                    try
                    {
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while(await reader.ReadAsync())
                            {
                                result.Add(new GradebookItemDTO
                                {
                                    StudentID = reader.GetInt32(reader.GetOrdinal("StudentID")),
                                    StudentName = reader.GetString(reader.GetOrdinal("StudentName")),
                                    StudentNumber = reader.GetString(reader.GetOrdinal("StudentNumber")),
                                    AssessmentID = reader.GetInt32(reader.GetOrdinal("AssessmentID")),
                                    AssessmentName = reader.GetString(reader.GetOrdinal("AssessmentName")),
                                    TotalMarks = reader.GetDecimal(reader.GetOrdinal("TotalMarks")),
                                    StudentAssessmentID = reader.IsDBNull(reader.GetOrdinal("StudentAssessmentID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("StudentAssessmentID")),
                                    MarksObtained = reader.IsDBNull(reader.GetOrdinal("MarksObtained")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("MarksObtained")),
                                    Status = GradebookItemDTO.ParseStatus(reader.GetString(reader.GetOrdinal("Status"))),
                                    Feedback = reader.IsDBNull(reader.GetOrdinal("Feedback")) ? null : reader.GetString(reader.GetOrdinal("Feedback")),
                                    AssessmentDate = reader.GetDateTime(reader.GetOrdinal("AssessmentDate")),
                                    IsDirty = false // Critical: Freshly loaded data isn't dirty
                                });
                            }
                        }
                    }
                    catch(SqlException ex)
                    {
                        EventLogger.LogError("Database error while fetching gradebook.", ex);
                        throw new Exception("Database error while fetching gradebook.", ex);
                    }
                    catch(Exception ex)
                    {
                        EventLogger.LogError("An Error while fetching gradebook.", ex);
                        throw new Exception("An Error while fetching gradebook.", ex);
                    }
                }

                return result;
            }
        }
        public static async Task<int> UpsertGradeAsync(GradebookItemDTO item, int evaluatorId)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_StudentAssessments_Upsert", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Input Parameters
                    cmd.Parameters.AddWithValue("@StudentID", item.StudentID);
                    cmd.Parameters.AddWithValue("@AssessmentID", item.AssessmentID);
                    cmd.Parameters.AddWithValue("@MarksObtained", (object)item.MarksObtained ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", GradebookItemDTO.GetStatusDisplay(item.Status) ?? "Pending");
                    cmd.Parameters.AddWithValue("@EvaluatedBy", evaluatorId);
                    cmd.Parameters.AddWithValue("@Feedback", (object)item.Feedback ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AssessmentDate", item.AssessmentDate);

                    // Output Parameter
                    var outputId = new SqlParameter("@StudentAssessmentID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outputId);

                    await connection.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();

                    return (int)outputId.Value;
                }
            }
        }
    }
}
