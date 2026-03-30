using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer.Helpers;

namespace TrainningCenter_DataAccessLayer
{
    public static class clsSubscription
    {
        public static async Task<SubscriptionAddResult> AddNewSubscription(
            int StudentID,
            int GroupID,
            DateTime DateOfSubscription,
            string PaymentStatus,
            string Status,
            int CreatedByUserID
        )
        {
            var result = new SubscriptionAddResult();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewMember", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@StudentID", SqlDbType.Int).Value = StudentID;
                    command.Parameters.Add("@GroupID", SqlDbType.Int).Value = GroupID;
                    command.Parameters.Add("@DateOfSubscription", SqlDbType.Date).Value = DateOfSubscription;
                    command.Parameters.Add("@PaymentStatus", SqlDbType.NVarChar, 20).Value = PaymentStatus;
                    command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = Status;
                    command.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value = CreatedByUserID;

                    try
                    {
                        await connection.OpenAsync();

                        using(SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();

                                // Read the results from Stored Procedure
                                if (!reader.IsDBNull(reader.GetOrdinal("SubscriptionID")))
                                    result.SubscriptionID = reader.GetInt32(reader.GetOrdinal("SubscriptionID"));

                                if (!reader.IsDBNull(reader.GetOrdinal("StudentName")))
                                    result.StudentName = reader.GetString(reader.GetOrdinal("StudentName"));

                                if (!reader.IsDBNull(reader.GetOrdinal("GroupID")))
                                    result.GroupID = reader.GetInt32(reader.GetOrdinal("GroupID"));

                                if (!reader.IsDBNull(reader.GetOrdinal("AttendanceRecordsCreated")))
                                    result.AttendanceRecordsCreated = reader.GetInt32(reader.GetOrdinal("AttendanceRecordsCreated"));

                                if (!reader.IsDBNull(reader.GetOrdinal("Message")))
                                    result.Message = reader.GetString(reader.GetOrdinal("Message"));

                                result.IsSuccess = true;
                            }
                            else
                            {
                                result.IsSuccess = false;
                                result.ErrorMessage = "No data returned from stored procedure.";
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = "A database error occurred while adding member!";
                        result.Exception = sqlEx;

                        EventLogger.LogError("A Database Error Occurred While Adding Member!", sqlEx);

                        return result;
                    }
                    catch (Exception ex)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = "An error occurred while adding member!";
                        result.Exception = ex;

                        EventLogger.LogError("An Error Occurred While Adding Member!", ex);
                        return result;
                    }
                }
            }

            return result;
        }

        public static async Task<bool> RemoveMember(int SubscriptionID)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_RemoveMember", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@SubscriptionID", SqlDbType.Int).Value = SubscriptionID;

                    try
                    {
                        await connection.OpenAsync();

                        // 1. Use ExecuteScalar because your SP returns "SELECT 1" on success
                        object result = await command.ExecuteScalarAsync();

                        // 2. Check if we got "1" back
                        if (result != null && int.TryParse(result.ToString(), out int rows))
                        {
                            return rows > 0;
                        }

                        return false;
                    }
                    catch (SqlException ex)
                    {
                        EventLogger.LogError($"Database error deleting subscription {SubscriptionID}", ex);
                        throw new ApplicationException($"Database error deleting subscription {SubscriptionID}", ex); // Rethrow to let UI handle messages like "Subscription not found"
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"General error deleting subscription {SubscriptionID}", ex);
                        throw new ApplicationException($"General error deleting subscription {SubscriptionID}", ex);
                    }
                }
            }
        }

        public static async Task<bool> UpdateSubscription(
            int SubscriptionID, 
            string PaymentStatus, 
            string Status, 
            decimal Grade
        ) {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_RemoveMember", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@SubscriptionID", SqlDbType.Int).Value = SubscriptionID;
                    command.Parameters.Add("@PaymentStatus", SqlDbType.NVarChar, 20).Value = PaymentStatus;
                    command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = Status;
                    command.Parameters.Add("@Grade", SqlDbType.Decimal).Value = Grade;

                    try
                    {
                        await connection.OpenAsync();

                        // 1. Use ExecuteScalar because your SP returns "SELECT 1" on success
                        object result = await command.ExecuteScalarAsync();

                        // 2. Check if we got "1" back
                        if (result != null && int.TryParse(result.ToString(), out int rows))
                        {
                            return rows > 0;
                        }

                        return false;
                    }
                    catch(SqlException ex)
                    {
                        EventLogger.LogError($"Database error Updating subscription {SubscriptionID}", ex);
                        throw new ApplicationException($"Database error Updating subscription {SubscriptionID}", ex);
                    }
                    catch(Exception ex)
                    {
                        EventLogger.LogError($"General error Updating subscription {SubscriptionID}", ex);
                        throw new ApplicationException($"General error Updating subscription {SubscriptionID}", ex);
                    }
                }
            }
        }

        public static async Task<bool> IsStudentEnrolled(int StudentID, int GroupID)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_IsStudentEnrolled", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@StudentID", SqlDbType.Int).Value = StudentID;
                    command.Parameters.Add("@GroupID", SqlDbType.Int).Value = GroupID;

                    try
                    {
                        await connection.OpenAsync();

                        // 1. Use ExecuteScalar to get the single BIT value
                        object result = await command.ExecuteScalarAsync();

                        // 2. Safely convert to boolean
                        if (result != null && bool.TryParse(result.ToString(), out bool isEnrolled))
                        {
                            return isEnrolled;
                        }

                        return false;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error checking enrollment for Student {StudentID} in Group {GroupID}", ex);
                        // In case of error, assume false or rethrow depending on your safety needs
                        return false;
                    }
                }
            }
        }

        public class SubscriptionAddResult
        {
            public bool IsSuccess { get; set; }
            public int? SubscriptionID { get; set; }
            public string StudentName { get; set; }
            public int GroupID { get; set; }
            public int AttendanceRecordsCreated { get; set; }
            public string Message { get; set; }
            public string ErrorMessage { get; set; }
            public int ReturnCode { get; set; }
            public Exception Exception { get; set; }
        }
    }
}
