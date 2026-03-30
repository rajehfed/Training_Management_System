using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer.Helpers;

namespace TrainningCenter_DataAccessLayer
{
    public static class clsSpecialization
    {
        /// <summary>
        /// this function be retrieve all the data of all active Specializations
        /// </summary>
        /// <returns>Data Table that contains all active Specializations</returns>
        public static async Task<DataTable> GetAllActiveSpecializations()
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllActiveSpecializations", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        EventLogger.LogInfo("Attempting to retrieve all active Specializations from database.");
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }

                        EventLogger.LogInfo($"Successfully retrieved {dt.Rows.Count} Users from database.");
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error occurred while retrieving active Specializations from database.", ex);
                        return null;
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// this function be retrieve all the data of all Specializations
        /// </summary>
        /// <returns>Data Table that contains all Specializations</returns>
        public static async Task<DataTable> GetAllSpecializations()
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllSpecializations", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        EventLogger.LogInfo("Attempting to retrieve all Specializations from database.");
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }

                        EventLogger.LogInfo($"Successfully retrieved {dt.Rows.Count} Users from database.");
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error occurred while retrieving Specializations from database.", ex);
                        return null;
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Retrieves a single specialization by ID.
        /// </summary>
        /// <param name="specializationID">The unique identifier of the specialization</param>
        /// <returns>DataRow containing specialization information if found; otherwise null</returns>
        public static async Task<DataRow> GetSpecializationById(int SpecializationID)
        {
            if (SpecializationID <= 0)
            {
                EventLogger.LogWarning($"Invalid SpecializationID: {SpecializationID}");
                return null;
            }

            DataTable dt = new DataTable();

            using(SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using(SqlCommand command = new SqlCommand("SP_GetSpecialazationByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@SpecializationID", SqlDbType.Int).Value = SpecializationID;

                    try
                    {
                        EventLogger.LogInfo($"Attempting to retrieve specialization with ID: {SpecializationID}");

                        await connection.OpenAsync();

                        using(SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }

                        if(dt.Rows.Count > 0) {
                            EventLogger.LogInfo($"Successfully retrieved specialization ID: {SpecializationID}");
                            return dt.Rows[0];
                        }
                        else {
                            EventLogger.LogWarning($"No specialization found with ID: {SpecializationID}");
                            return null;
                        }
                    }
                    catch(Exception ex) {
                        EventLogger.LogError($"Failed to retrieve specialization ID: {SpecializationID}", ex);
                        return null;
                    }
                }
            }

        }

        /// <summary>
        /// Adds a new specialization to the database.
        /// </summary>
        /// <returns>The new SpecializationID if successful; otherwise -1</returns>
        public static async Task<int> AddNewSpecialization(
            string SpecializationName,
            string SpecializationDescription,
            string Category,
            string Level,
            int? Duration,
            decimal? Price,
            string Prerequisites,
            string CertificateTemplate
        )
        {
            int insertedID = -1;

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SP_AddNewSpecialization", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@SpecializationName", SqlDbType.NVarChar, 100).Value = SpecializationName;
                    cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value =
                        string.IsNullOrEmpty(SpecializationDescription) ? (object)DBNull.Value : SpecializationDescription;
                    cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 50).Value =
                        string.IsNullOrEmpty(Category) ? (object)DBNull.Value : Category;
                    cmd.Parameters.Add("@Level", SqlDbType.NVarChar, 20).Value =
                        string.IsNullOrEmpty(Level) ? (object)DBNull.Value : Level;
                    cmd.Parameters.Add("@Duration", SqlDbType.Int).Value =
                        Duration.HasValue ? (object)Duration.Value : DBNull.Value;
                    cmd.Parameters.Add("@Price", SqlDbType.Decimal).Value =
                        Price.HasValue ? (object)Price.Value : DBNull.Value;
                    cmd.Parameters.Add("@Prerequisites", SqlDbType.NVarChar, 500).Value =
                        string.IsNullOrEmpty(Prerequisites) ? (object)DBNull.Value : Prerequisites;
                    cmd.Parameters.Add("@CertificateTemplate", SqlDbType.NVarChar, 500).Value =
                        string.IsNullOrEmpty(CertificateTemplate) ? (object)DBNull.Value : CertificateTemplate;

                    // Output Parameter to get the inserted ID
                    SqlParameter outputParam = new SqlParameter("@NewSpecializationID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                        if (outputParam.Value != DBNull.Value)
                        {
                            insertedID = Convert.ToInt32(outputParam.Value);
                            EventLogger.LogInfo($"Successfully added specialization: {SpecializationName} with ID: {insertedID}");
                        }
                        else
                        {
                            EventLogger.LogWarning($"Failed to add specialization: {SpecializationName}. No ID returned.");
                        }

                        return insertedID;
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database error while adding specialization: {SpecializationName}", sqlEx);
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Unexpected error while adding specialization: {SpecializationName}", ex);
                        return -1;
                    }
                }
            }
        }

        /// <summary>
        /// Updates an existing specialization in the database.
        /// </summary>
        /// <returns>True if update was successful; otherwise false</returns>
        public static async Task<bool> UpdateSpecialization(
            int SpecializationID,
            string SpecializationName,
            string SpecializationDescription,
            string Category,
            string Level,
            int? Duration,
            decimal? Price,
            string Prerequisites,
            string CertificateTemplate,
            bool IsActive
            )
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SP_UpdateSpecialization", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@SpecializationID", SqlDbType.Int).Value = SpecializationID;
                    cmd.Parameters.Add("@SpecializationName", SqlDbType.NVarChar, 100).Value = SpecializationName;
                    cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value =
                        string.IsNullOrEmpty(SpecializationDescription) ? (object)DBNull.Value : SpecializationDescription;
                    cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 50).Value =
                        string.IsNullOrEmpty(Category) ? (object)DBNull.Value : Category;
                    cmd.Parameters.Add("@Level", SqlDbType.NVarChar, 20).Value =
                        string.IsNullOrEmpty(Level) ? (object)DBNull.Value : Level;
                    cmd.Parameters.Add("@Duration", SqlDbType.Int).Value =
                        Duration.HasValue ? (object)Duration.Value : DBNull.Value;
                    cmd.Parameters.Add("@Price", SqlDbType.Decimal).Value =
                        Price.HasValue ? (object)Price.Value : DBNull.Value;
                    cmd.Parameters.Add("@Prerequisites", SqlDbType.NVarChar, 500).Value =
                        string.IsNullOrEmpty(Prerequisites) ? (object)DBNull.Value : Prerequisites;
                    cmd.Parameters.Add("@CertificateTemplate", SqlDbType.NVarChar, 500).Value =
                        string.IsNullOrEmpty(CertificateTemplate) ? (object)DBNull.Value : CertificateTemplate;
                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = Convert.ToByte(IsActive);

                    try
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int rowsAffected = reader["RowsAffected"] as int? ?? 0;
                                string status = reader["Status"].ToString();
                                string message = reader["Message"].ToString();

                                if (status == "SUCCESS")
                                {
                                    EventLogger.LogInfo($"Successfully updated Specialization ID: {SpecializationID}. {message}");
                                    return rowsAffected > 0;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"Failed to update Specialization ID: {SpecializationID}. {message}");
                                    return false;
                                }
                            }
                            else
                            {
                                EventLogger.LogWarning($"No result returned when updating specialization ID: {SpecializationID}");
                                return false;
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database error while updating specialization ID: {SpecializationID}", sqlEx);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Unexpected error while updating specialization ID: {SpecializationID}", ex);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Soft deletes (deactivates) a specialization.
        /// </summary>
        /// <param name="specializationID">The ID of the specialization to delete</param>
        /// <returns>True if successful; otherwise false</returns>
        public static async Task<bool> DeleteSpecialization(int SpecializationID)
        {
            if (SpecializationID <= 0)
            {
                EventLogger.LogWarning($"The ID No Must Be not negative or zero!!");
                return false;
            }

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SP_DeleteSpecialization", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@SpecializationID", SqlDbType.Int).Value = SpecializationID;

                    try
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string status = reader["Status"].ToString();
                                string message = reader["Message"].ToString();

                                if (status == "SUCCESS")
                                {
                                    EventLogger.LogInfo($"Successfully deleted Specialization ID: {SpecializationID}");
                                    return true;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"Delete failed for Specialization ID {SpecializationID}: {message}");
                                    return false;
                                }
                            }
                        }

                        return false;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error deleting Specialization ID {SpecializationID}", ex);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Toggles the active status of a specialization.
        /// </summary>
        /// <param name="specializationID">The ID of the specialization</param>
        /// <returns>True if successful; otherwise false</returns>
        public static async Task<bool> ToggleSpecializationStatus(int SpecializationID)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("SP_ToggleSpecializationStatus", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@SpecializationID", SqlDbType.Int).Value = SpecializationID;

                    try
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string status = reader["Status"].ToString();
                                string message = reader["Message"].ToString();

                                if (status == "SUCCESS")
                                {
                                    EventLogger.LogInfo($"Successfully toggled status for Specialization ID: {SpecializationID}");
                                    return true;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"Toggle failed for Specialization ID {SpecializationID}: {message}");
                                    return false;
                                }
                            }
                        }

                        return false;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error toggling status for Specialization ID {SpecializationID}", ex);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Searches for specializations based on criteria.
        /// </summary>
        /// <returns>DataTable containing matching specializations</returns>
        public static async Task<DataTable> SearchSpecializations(
            string SearchTerm,
            string Category,
            string Level,
            bool? IsActive
            )
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_SearchSpecializations", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@SearchTerm", SqlDbType.NVarChar, 100).Value =
                        string.IsNullOrEmpty(SearchTerm) ? (object)DBNull.Value : SearchTerm;
                    cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 50).Value =
                        string.IsNullOrEmpty(Category) ? (object)DBNull.Value : Category;
                    cmd.Parameters.Add("@Level", SqlDbType.NVarChar, 20).Value =
                        string.IsNullOrEmpty(Level) ? (object)DBNull.Value : Level;
                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value =
                        IsActive.HasValue ? (object)IsActive.Value : DBNull.Value;

                    try
                    {
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }
                        EventLogger.LogInfo($"Search returned {dt.Rows.Count} specializations.");
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error searching specializations.", ex);
                        return null;
                    }
                }
            }

            return dt;
        }
    }
}
