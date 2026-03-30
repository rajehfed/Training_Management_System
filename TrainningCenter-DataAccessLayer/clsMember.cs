using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainningCenter_Entities;
using TrainningCenter_DataAccessLayer.Helpers;

namespace TrainningCenter_DataAccessLayer
{
    public static class clsMember
    {
        public static async Task<IReadOnlyList<MemberDTO>> GetGroupMembers(int GroupID, bool OnlyActiveSubscribers)
        {
            List<MemberDTO> members = new List<MemberDTO>();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetGroupMembers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@GroupID", SqlDbType.Int).Value = GroupID;
                    command.Parameters.Add("@OnlyActiveSubscriptions", SqlDbType.Bit).Value = OnlyActiveSubscribers;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            // 1. OPTIMIZATION: Get Ordinals ONCE before the loop
                            int studentIDIndex = reader.GetOrdinal("StudentID");
                            int fullNameIndex = reader.GetOrdinal("FullName");
                            int emailIndex = reader.GetOrdinal("Email");
                            int dateOfSubscriptionIndex = reader.GetOrdinal("DateOfSubscription");
                            int isActiveIndex = reader.GetOrdinal("IsActive");
                            int statusIndex = reader.GetOrdinal("Status");
                            int completionDateIndex = reader.GetOrdinal("CompletionDate");
                            int amountIndex = reader.GetOrdinal("Amount");
                            int phoneNumberIndex = reader.GetOrdinal("PhoneNumber");
                            int subscriptionIDIndex = reader.GetOrdinal("SubscriptionID");

                            // Check if Grade column exists in DTO/SP
                            int gradeIndex = reader.GetOrdinal("Grade");

                            while (await reader.ReadAsync())
                            {
                                members.Add(new MemberDTO
                                {
                                    GroupID = GroupID,
                                    SubscriptionID = reader.GetInt32(subscriptionIDIndex),
                                    StudentID = reader.GetInt32(studentIDIndex),

                                    // Safe String Reads
                                    FullName = reader.IsDBNull(fullNameIndex) ? "Unknown" : reader.GetString(fullNameIndex),
                                    Email = reader.IsDBNull(emailIndex) ? "" : reader.GetString(emailIndex),
                                    PhoneNumber = reader.IsDBNull(phoneNumberIndex) ? "" : reader.GetString(phoneNumberIndex),

                                    DateOfSubscription = reader.GetDateTime(dateOfSubscriptionIndex),
                                    IsActive = reader.GetBoolean(isActiveIndex),
                                    Status = reader.GetString(statusIndex),

                                    // 2. CRITICAL FIX: Handle Nullable Completion Date
                                    CompletionDate = reader.IsDBNull(completionDateIndex)
                                                     ? (DateTime?)null
                                                     : reader.GetDateTime(completionDateIndex),

                                    // Amount is likely 0 if null due to LEFT JOIN, but safe check is good
                                    Amount = reader.IsDBNull(amountIndex) ? 0 : reader.GetDecimal(amountIndex),

                                    // Handle Grade if your DTO has it
                                    Grade = reader.IsDBNull(gradeIndex) ? (decimal?)null : reader.GetDecimal(gradeIndex)
                                });
                            }

                            return members;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError("Database Error When Try to Excute SP_GetGroupMembers", sqlEx);
                        throw new ApplicationException("Database Error When Try to Excute SP_GetGroupMembers", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error When Try to Call SP_GetGroupMembers", ex);
                        throw new ApplicationException("Error When Try to Call SP_GetGroupMembers", ex);
                    }
                }
            }
        }
    }
}
