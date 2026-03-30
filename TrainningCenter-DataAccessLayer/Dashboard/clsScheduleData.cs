using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer.Helpers;
using TrainningCenter_Entities;

namespace TrainningCenter_DataAccessLayer.Dashboard
{
    public static class clsScheduleData
    {
        public static async Task<List<TodaySessionModelDTO>> GetTodayScheduleAsync()
        {
            List<TodaySessionModelDTO> sessions = new List<TodaySessionModelDTO>();

            using (SqlConnection conn = DataBaseSettings.GetConnection())
            {
                string query = @"SELECT 
                                s.SessionID,
                                FORMAT(s.StartTime, 'h:mm tt') AS Time,
                                s.Topic AS Title,
                                g.GroupName,
                                ISNULL(s.Location, '') AS Location
                                FROM Sessions s
                                INNER JOIN Groups g ON s.GroupID = g.GroupID
                                WHERE s.SessionDate >= CAST(GETDATE() AS DATE) 
                                    AND s.SessionDate < DATEADD(day, 1, CAST(GETDATE() AS DATE))
                                    AND s.IsActive = 1
                                ORDER BY s.StartTime";

                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    try
                    {
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                sessions.Add(new TodaySessionModelDTO
                                {
                                    SessionID = (int)reader["SessionID"],
                                    Time = reader["Time"].ToString(),
                                    Title = reader["Title"].ToString(),
                                    GroupName = reader["GroupName"].ToString(),
                                    Location = reader["Location"].ToString()
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("A Database Error While Try to load the Today Schedule!", ex);
                    }
                }
            }

            return sessions;
        }
    }
}
