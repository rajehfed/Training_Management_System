using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer.Helpers;

namespace TrainningCenter_DataAccessLayer
{
    public static class clsUser
    {
        public enum enRole { eAdmin = 1, eTrainer = 2, eStudent = 4, eReceptionist = 8, eManager = 16 }
        /// <summary>
        /// this function will be retrieve all the data of all active Users
        /// </summary>
        /// <returns>Data Table that contains all active Users</returns>
        public static async Task<DataTable> GetAllActiveUsers()
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllActiveUsers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        EventLogger.LogInfo("Attempting to retrieve all Users from database.");

                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }
                        EventLogger.LogInfo($"Successfully retrieved {dt.Rows.Count} Users from database.");
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Failed to retrieve Users from database.", ex);
                        return null;
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// this method adds a new User to the database.
        /// </summary>
        /// <param name="personID">person ID that refers the User</param>
        /// <param name="username">Username of The User that will be added</param>
        /// <param name="passwordHash">The password of the User</param>
        /// <param name="role">the role of the user between those ('Admin', 'Trainer', 'Student', 'Receptionist', 'Manager')</param>
        /// <returns>the UserID that be inserted</returns>
        public static async Task<int> AddNewUser(
            int personID,
            string username,
            string passwordHash,
            string role
        )
        {
            int insertedID = -1;

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("SP_AddNewUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@PersonID", SqlDbType.Int).Value = personID;
                    command.Parameters.Add("@UserName", SqlDbType.NVarChar, 255).Value = username;
                    command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = passwordHash;
                    command.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = role;

                    try
                    {
                        object result = await command.ExecuteScalarAsync();

                        if (result != null && int.TryParse(result.ToString(), out insertedID))
                        {
                            EventLogger.LogInfo($"Successfully added User: {username} with ID: {insertedID}");
                            return insertedID;
                        }

                        EventLogger.LogWarning($"Failed to add User: {username} . No ID returned.");
                        return -1;
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database error while adding User: {username}", sqlEx);
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Unexpected error while adding User: {username}", ex);
                        return -1;
                    }
                }
            }
        }

        /// <summary>
        /// This method updates an existing User's details in the database.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="personID"></param>
        /// <param name="username"></param>
        /// <param name="passwordHash"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateUser(
            int userID,
            int personID,
            string username,
            string passwordHash,
            string role
        )
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("SP_UpdateUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;
                    command.Parameters.Add("@PersonID", SqlDbType.Int).Value = personID;
                    command.Parameters.Add("@UserName", SqlDbType.NVarChar, 255).Value = username;
                    command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = passwordHash;
                    command.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = role;

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int rowsAffected = reader["RowsAffected"] as int? ?? 0;
                                string status = reader["Status"].ToString();
                                string message = reader["Message"].ToString();

                                if (status == "SUCCESS")
                                {
                                    EventLogger.LogInfo($"Successfully updated User ID: {userID}. {message}");
                                    return rowsAffected > 0;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"Update failed for User ID {userID}: {message}");
                                    return false;
                                }
                            }
                            else
                            {
                                EventLogger.LogError("No result returned from SP_UpdateUser.");
                                return false;
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError("Database error while updating User.", sqlEx);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Unexpected error while updating User.", ex);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a user exists in the database (used for login).
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <param name="password">The password (hashed)</param>
        /// <returns>True if user exists, otherwise false</returns>
        public static async Task<bool> IsUserExists(string username, string password)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("SP_IsUserExistsByUsernameAndPassword", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@Username", SqlDbType.NVarChar, 255).Value = username;
                    command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = password;

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string status = reader["Status"].ToString();
                                string message = reader["Message"].ToString();

                                if (status == "SUCCESS")
                                {
                                    EventLogger.LogInfo($"User '{username}' exists in the DB. Message: {message}");
                                    return true;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"The User {username} not exists in the DB: {message}");
                                    return false;
                                }
                            }
                            else
                            {
                                EventLogger.LogError("No result returned from SP_IsUserExistsByUsernameAndPassword.");
                                return false;
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError("Database error while Retvieving User.", sqlEx);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Unexpected error while Retvieving User.", ex);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// This function checks if a user exists and logs them in.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="passwordHash">The hashed password</param>
        /// <returns>A DataTable That contains all the person Data</returns>
        public static async Task<DataTable> LoginUser(string username, string password)
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("SP_UserLogin", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Username", SqlDbType.NVarChar, 255).Value = username;
                    command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = password;

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            dt.Load(reader);

                            if (dt.Rows.Count > 0)
                            {
                                string status = dt.Rows[0]["Status"].ToString();
                                string message = dt.Rows[0]["Message"].ToString();

                                if (status == "SUCCESS")
                                {
                                    EventLogger.LogInfo($"User '{username}' exists in the DB. Message: {message}");
                                    return dt;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"The User {username} not exists in the DB: {message}");
                                    return null;
                                }
                            }
                            else
                            {
                                EventLogger.LogError("No result returned from SP_UserLogin.");
                                return null;
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError("Database error while Retvieving User.", sqlEx);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Unexpected error while Retvieving User.", ex);
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a single user's data from the database using their unique UserID.
        /// </summary>
        /// <param name="userID">The unique identifier of the user to retrieve.</param>
        /// <returns>
        /// A <see cref="DataRow"/> containing the user's details if found; 
        /// otherwise, <c>null</c> if no user exists with the provided ID 
        /// or if an error occurs during the database operation.
        /// </returns>
        /// <remarks>
        ///     This method asynchronously connects to the database and executes the 
        ///     stored procedure <b>SP_GetUserByID</b>. It logs every important step 
        ///     including success, warning, and error events using the <see cref="EventLogger"/>.
        /// </remarks>
        /// <example>
        ///     Example usage:
        ///     <code>
        ///         DataRow userRow = await clsUser.GetUserById(5);
        ///         if (userRow != null)
        ///         {
        ///             Console.WriteLine($"User found: {userRow["UserName"]}");
        ///         }
        ///         else
        ///         {
        ///             Console.WriteLine("User not found.");
        ///         }
        ///     </code>
        /// </example>
        public static async Task<DataRow> GetUserById(int userID)
        {
            if (userID <= 0)
            {
                EventLogger.LogWarning($"Invalid UserID: {userID}");
                return null;
            }

            DataTable dt = new DataTable();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetUserByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;

                    try
                    {
                        EventLogger.LogInfo($"Attempting to retrieve The User With Id: {userID} from database.");

                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }
                        EventLogger.LogInfo($"Successfully retrieved {dt.Rows.Count} Users from database.");
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Failed to retrieve the User with Id: {userID} from database.", ex);
                        return null;
                    }
                }
            }

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Retrieves a user from the database by username.
        /// </summary>
        /// <param name="username">The username to search for</param>
        /// <returns>DataRow containing user information if found; otherwise null</returns>
        public static async Task<DataRow> GetUserByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                EventLogger.LogWarning($"Invalid Username: {username}");
                return null;
            }

            DataTable dt = new DataTable();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetUserByUsername", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Username", SqlDbType.NVarChar, 255).Value = username;

                    try
                    {
                        EventLogger.LogInfo($"Attempting to retrieve user: {username} from database.");

                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }

                        EventLogger.LogInfo($"Successfully retrieved {dt.Rows.Count} user(s) from database.");
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Failed to retrieve user: {username} from database.", ex);
                        return null;
                    }
                }
            }

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }
        public static async Task<bool> DeactivateUser(int userID)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("SP_DeleteUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;
                    try
                    {
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0) {
                            EventLogger.LogInfo($"Successfully deactivated User ID: {userID}");
                            return true;
                        }
                        else {
                            EventLogger.LogWarning($"No User found with ID: {userID} to deactivate.");
                            return false;
                        }
                    }
                    catch (SqlException sqlEx) {
                        EventLogger.LogError("Database error while deactivating User.", sqlEx);
                        return false;
                    }
                    catch (Exception ex) {
                        EventLogger.LogError("Unexpected error while deactivating User.", ex);
                        return false;
                    }
                }
            }
        }
    }
}
