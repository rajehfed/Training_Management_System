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
    public static class clsActivity
    {
        public async static Task<List<ActivityDTO>> GetGroupRecentActivities(int groupID, int top = 20)
        {
            List<ActivityDTO> dtoList = new List<ActivityDTO>();
            using(SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using(SqlCommand command = new SqlCommand("SP_GetGroupRecentActivity", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@GroupID", SqlDbType.Int).Value = groupID;
                    command.Parameters.Add("@Top", SqlDbType.Int).Value = top;

                    try
                    {
                        await connection.OpenAsync();

                        using(SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            int activityIDIndex = reader.GetOrdinal("ActivityID");
                            int activityNameIndex = reader.GetOrdinal("ActivityType");
                            int descriptionIndex = reader.GetOrdinal("Description");
                            int timestampIndex = reader.GetOrdinal("Timestamp");
                            int createdByUserIndex = reader.GetOrdinal("CreatedUserID");
                            int usernameIndex = reader.GetOrdinal("Username");

                            while(await reader.ReadAsync())
                            {
                                dtoList.Add(
                                    new ActivityDTO
                                    {
                                        ActivityID = reader.GetInt32(activityIDIndex),
                                        ActivityType = reader.GetString(activityNameIndex),
                                        Description = reader.IsDBNull(descriptionIndex) ? string.Empty : reader.GetString(descriptionIndex),
                                        Timestamp = reader.GetDateTime(timestampIndex),
                                        CreatedByUserID = reader.GetInt32(createdByUserIndex),
                                        Username = reader.GetString(usernameIndex),
                                        GroupID = groupID
                                    });
                            }

                            return dtoList;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError("Database error during Activity Recent retrieval.", sqlEx);
                        throw new ApplicationException("Database error Activity Recent retrieval.", sqlEx);
                    }
                    catch(Exception ex)
                    {
                        EventLogger.LogError("Error during Activity Recent retrieval.", ex);
                        throw new ApplicationException("Error Activity Recent retrieval.", ex);
                    }
                }
            }
        }
    }
}
