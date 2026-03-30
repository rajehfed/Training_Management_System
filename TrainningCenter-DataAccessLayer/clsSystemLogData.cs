using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter_DataAccessLayer
{
    public class clsSystemLogData
    {
        public static async Task LogAction(int userId, string actionType, string tableName, int recordId, string details = "")
        {
            using(SqlConnection conn = DataBaseSettings.GetConnection())
            {
                string query = @"INSERT INTO SystemLogs (UserID, ActionType, TableName, RecordID, Details)
                             VALUES (@UserID, @ActionType, @TableName, @RecordID, @Details)";

                using(SqlCommand command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.Parameters.AddWithValue("@ActionType", actionType);
                    command.Parameters.AddWithValue("@TableName", tableName);
                    command.Parameters.AddWithValue("@RecordID", recordId);
                    command.Parameters.AddWithValue("@Details", details);

                    await conn.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
