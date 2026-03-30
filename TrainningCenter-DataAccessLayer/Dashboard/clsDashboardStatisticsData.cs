using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer.Helpers;
using TrainningCenter_Entities;

namespace TrainningCenter_DataAccessLayer.Dashboard
{
    public static class clsDashboardStatisticsData
    {
        public static async Task<int> GetTotalStudentsAsync()
        {
            int total = 0;

            using (SqlConnection conn = DataBaseSettings.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM Students WHERE IsActive = 1";

                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    try
                    {
                        await conn.OpenAsync();
                        object result = await command.ExecuteScalarAsync();

                        if (result is not null && int.TryParse(result.ToString(), out int count))
                        {
                            total = count;
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("A Database Error While Loading the Count to the Total Students", ex);
                    }
                }
            }

            return total;
        }
        public static async Task<int> GetActiveGroupsAsync()
        {
            int total = 0;

            using (SqlConnection conn = DataBaseSettings.GetConnection())
            {
                string query = @"SELECT COUNT(*) FROM Groups 
                                 WHERE IsActive = 1 
                                 AND (EndDate IS NULL OR EndDate >= GETDATE())";

                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    try
                    {
                        await conn.OpenAsync();
                        object result = await command.ExecuteScalarAsync();

                        if (result is not null && int.TryParse(result.ToString(), out int count))
                        {
                            total = count;
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("A Database Error While Loading the Count to the Active Groups", ex);
                    }
                }
            }

            return total;
        }
        public static async Task<decimal> GetRevenueThisMonthAsync()
        {
            decimal revenue = 0;

            using (SqlConnection conn = DataBaseSettings.GetConnection())
            {
                string query = @"SELECT ISNULL(SUM(Amount), 0)
                                    FROM 
	                                    Payments P
                                    INNER JOIN 
	                                    [dbo].Subscriptions S
                                    ON 
	                                    P.SubscriptionID = S.SubscriptionID
                                    WHERE MONTH(PaymentDate) = MONTH(GETDATE())
	                                    AND YEAR(PaymentDate) = YEAR(GETDATE())
	                                    AND PaymentStatus = 'Completed'";

                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    try
                    {
                        await conn.OpenAsync();
                        object result = await command.ExecuteScalarAsync();

                        if (result is not null && decimal.TryParse(result.ToString(), out decimal amount))
                        {
                            revenue = amount;
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("A Database Error While Loading the revenue this mounth", ex);
                    }
                }
            }

            return revenue;
        }
        public static async Task<decimal> GetRevenueLastMonthAsync()
        {
            decimal revenue = 0;

            using (SqlConnection conn = DataBaseSettings.GetConnection())
            {
                string query = @"SELECT ISNULL(SUM(Amount), 0) 
                                FROM Payments P
                                INNER JOIN [dbo].Subscriptions S
                                ON P.SubscriptionID = S.SubscriptionID
                                WHERE MONTH(PaymentDate) = MONTH(DATEADD(month, -1, GETDATE()))
                                    AND YEAR(PaymentDate) = YEAR(DATEADD(month, -1, GETDATE()))
                                    AND PaymentStatus = 'Completed'";
                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    try
                    {
                        await conn.OpenAsync();
                        object result = await command.ExecuteScalarAsync();

                        if (result is not null && decimal.TryParse(result.ToString(), out decimal amount))
                        {
                            revenue = amount;
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("A Database Error While Loading the revenue last mounth", ex);
                    }
                }
            }

            return revenue;
        }
        public static async Task<decimal> GetAttendanceRateAsync()
        {
            decimal rate = 0;

            using (SqlConnection conn = DataBaseSettings.GetConnection())
            {
                string query = @"SELECT ISNULL(
                                    CAST(
                                        SUM(CASE WHEN AttendanceStatus = 'Present' THEN 1 ELSE 0 END) * 100.0 / 
                                        NULLIF(COUNT(*), 0)
                                        AS DECIMAL(5,2)
                                    ), 0)
                                FROM Attendance
                                WHERE RecordedAt >= DATEADD(day, -30, GETDATE())";

                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    try
                    {
                        await conn.OpenAsync();
                        object result = await command.ExecuteScalarAsync();

                        if (result is not null && decimal.TryParse(result.ToString(), out decimal percentage))
                        {
                            rate = percentage;
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("A Database Error While Loading the Attendance rate", ex);
                    }
                }
            }

            return rate;
        }
        public static async Task<int> GetSessionsTodayAsync()
        {
            int total = 0;

            using (SqlConnection conn = DataBaseSettings.GetConnection())
            {
                string query = @"SELECT COUNT(*) FROM Sessions 
                                WHERE CAST(SessionDate AS DATE) = CAST(GETDATE() AS DATE)
                                    AND IsActive = 1";

                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    try
                    {
                        await conn.OpenAsync();
                        object result = await command.ExecuteScalarAsync();

                        if (result is not null && int.TryParse(result.ToString(), out int count))
                        {
                            total = count;
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("A Database Error While Loading the Count to the Sessions of Today", ex);
                    }
                }
            }

            return total;
        }
        public static async Task<int> GetNewStudentsThisMonthAsync()
        {
            int total = 0;

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                string query = @"SELECT COUNT(*) 
                                FROM Subscriptions 
                                WHERE MONTH(DateOfSubscription) = MONTH(GETDATE())
                                    AND YEAR(DateOfSubscription) = YEAR(GETDATE())
                                    AND IsActive = 1";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync();

                        if (result != null && int.TryParse(result.ToString(), out int count))
                        {
                            total = count;
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("A Database Error While Typing The Data!", ex);
                    }
                }
            }

            return total;
        }
        public static async Task<int> GetNewStudentsLastMonthAsync()
        {
            int total = 0;

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                string query = @"SELECT COUNT(*) 
                                FROM Subscriptions
                                WHERE MONTH(DateOfSubscription) = MONTH(DATEADD(month, -1, GETDATE()))
                                  AND YEAR(DateOfSubscription) = YEAR(DATEADD(month, -1, GETDATE()))";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        await connection.OpenAsync();
                        object result = await command.ExecuteScalarAsync();

                        if (result != null && int.TryParse(result.ToString(), out int count))
                        {
                            total = count;
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("A Database Error While Typing The Data!", ex);
                    }
                }
            }

            return total;
        }
        public static async Task<List<ActivityLogDTO>> GetRecentActivities(int topCount)
        {
            var activities = new List<ActivityLogDTO>();

            using (SqlConnection conn = DataBaseSettings.GetConnection())
            {
                string query = @"SELECT TOP (@TopCount) 
                                    l.LogID, 
                                    l.ActionType, 
                                    l.TableName, 
                                    l.ActionDate, 
                                    l.Details,
                                    u.UserName as PerformedBy
                                FROM SystemLogs l
                                LEFT JOIN Users u ON l.UserID = u.UserID
                                ORDER BY l.ActionDate DESC";

                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    command.Parameters.Add("@TopCount", System.Data.SqlDbType.Int).Value = topCount;

                    try
                    {
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            int logIdIndex = reader.GetOrdinal("LogID");
                            int actionTypeIndex = reader.GetOrdinal("ActionType");
                            int tableNameIndex = reader.GetOrdinal("TableName");
                            int actionDateIndex = reader.GetOrdinal("ActionDate");
                            int detailsIndex = reader.GetOrdinal("Details");
                            int performedByIndex = reader.GetOrdinal("PerformedBy");

                            while (await reader.ReadAsync())
                            {
                                activities.Add(new ActivityLogDTO
                                {
                                    LogId = reader.GetInt32(logIdIndex),
                                    ActionType = reader.GetString(actionTypeIndex),
                                    TableName = reader.GetString(tableNameIndex),
                                    ActionDate = reader.GetDateTime(actionDateIndex),
                                    Details = reader.IsDBNull(detailsIndex) ? null : reader.GetString(detailsIndex),
                                    PerformedBy = reader.IsDBNull(performedByIndex) ? null : reader.GetString(performedByIndex)
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("A Database Error Was Occurred!!", ex);
                        throw;
                    }
                }

                return activities;
            }
        }
        public static async Task<List<UpcomingEventDTO>> GetUpcomingEvents()
        {
            var events = new List<UpcomingEventDTO>();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                string query = @"SELECT 
                                    EventID, 
                                    Title, 
                                    StartDate, 
                                    EndDate, 
                                    Category, 
                                    MainColor, 
                                    BgColor
                                FROM UpcomingEvents 
                                WHERE IsActive = 1
                                ORDER BY StartDate ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            int eventIdIndex = reader.GetOrdinal("EventID");
                            int titleIndex = reader.GetOrdinal("Title");
                            int startDateIndex = reader.GetOrdinal("StartDate");
                            int endDateIndex = reader.GetOrdinal("EndDate");
                            int categoryIndex = reader.GetOrdinal("Category");
                            int mainColorIndex = reader.GetOrdinal("MainColor");
                            int bgColorIndex = reader.GetOrdinal("BgColor");

                            while (await reader.ReadAsync())
                            {
                                events.Add(new UpcomingEventDTO
                                {
                                    EventId = reader.GetInt32(eventIdIndex),
                                    Title = reader.GetString(titleIndex),
                                    StartDate = reader.GetDateTime(startDateIndex),
                                    EndDate = reader.IsDBNull(endDateIndex) ? null : reader.GetDateTime(endDateIndex),
                                    Category = reader.IsDBNull(categoryIndex) ? null : reader.GetString(categoryIndex),
                                    MainColor = reader.IsDBNull(mainColorIndex) ? null : reader.GetString(mainColorIndex),
                                    BgColor = reader.IsDBNull(bgColorIndex) ? null : reader.GetString(bgColorIndex)
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("A Database Error Was Occurred!!", ex);
                        throw;
                    }
                }
            }
            return events;
        }
    }
}