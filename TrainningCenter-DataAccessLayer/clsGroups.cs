using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer.Helpers;
using TrainningCenter_Entities;

namespace TrainningCenter_DataAccessLayer
{
    public static class clsGroups
    {
        public static async Task<DataTable> GetAllGroups()
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllGroups", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        EventLogger.LogInfo("Attempting to retrieve all Groups from database.");
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }

                        EventLogger.LogInfo($"Successfully retrieved {dt.Rows.Count} Groups from database.");
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error occurred while retrieving active Groups from database.", ex);
                        return null;
                    }
                }
            }

            return dt;
        }
        public static async Task<DataTable> GetGroupsBySpecialization(int specializationID)
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetGroupsBySpecialization", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@SpecializationID", SqlDbType.Int).Value = specializationID;

                    try
                    {
                        EventLogger.LogInfo("Attempting to retrieve all Groups from database.");
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }

                        EventLogger.LogInfo($"Successfully retrieved {dt.Rows.Count} Groups from database.");
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error occurred while retrieving active Groups from database.", ex);
                        return null;
                    }
                }
            }

            return dt;
        }
        public static async Task<int> GetStudentCountBySpecialization(int specializationID)
        {
            var dt = await GetGroupsBySpecialization(specializationID);
            if (dt == null || dt.Rows.Count == 0)
                return 0;

            int total = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (dt.Columns.Contains("CurrentTrainees"))
                {
                    total += Convert.ToInt32(row["CurrentTrainees"]);
                }
            }

            return total;
        }

        /// <summary>
        /// Converts the internal <see cref="enStatus"/> enum into the specific string representation required by the Database Check Constraint.
        /// </summary>
        /// <param name="status">The status enum value (e.g., stActive, stSuspended).</param>
        /// <returns>
        /// A string matching the database constraint (e.g., "Active", "Suspended"). 
        /// Returns "Active" by default if the enum is not recognized.
        /// </returns>
        public static string GetStatusName(enStatus status)
        {
            switch (status)
            {
                case enStatus.stSuspended: return "Suspended";
                case enStatus.stActive: return "Active";
                case enStatus.stCompleted: return "Completed";
                case enStatus.stCancelled: return "Cancelled";
                default: return "Active";
            }
        }

        /// <summary>
        /// Asynchronously adds a new training group to the database by executing the 'SP_AddNewGroup' stored procedure.
        /// </summary>
        /// <param name="SpecializationID">The foreign key for the Specialization.</param>
        /// <param name="TrainerID">The foreign key for the Trainer.</param>
        /// <param name="CenterID">The foreign key for the Training Center.</param>
        /// <param name="GroupName">The name of the group (must be unique within the center if enforced).</param>
        /// <param name="StartDate">The scheduled start date.</param>
        /// <param name="EndDate">The scheduled end date (must be after StartDate).</param>
        /// <param name="MaxTrainees">The maximum capacity of the group.</param>
        /// <param name="CurrentTrainees">The initial count of trainees (defaults to 0 if null).</param>
        /// <param name="Schedule">The text description of the schedule (e.g., "Mon-Wed 10am").</param>
        /// <param name="Room">The room assignment.</param>
        /// <param name="Status">The initial status of the group.</param>
        /// <returns>
        /// A <see cref="Task"/> containing the newly created <c>GroupID</c> if successful. 
        /// Returns <c>null</c> (or -1) if the operation fails or validation errors occur.
        /// </returns>
        /// <exception cref="SqlException">Logged via EventLogger if a database connection error occurs.</exception>
        public static async Task<int?> AddNewGroup(
            int SpecializationID,
            int TrainerID,
            int CenterID,
            string GroupName,
            DateTime StartDate,
            DateTime EndDate,
            int MaxTrainees,
            int CurrentTrainees,
            string Schedule,
            string Room,
            enStatus Status
        )
        {
            int? insertedID = null;

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("SP_AddNewGroup", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@SpecializationID", SqlDbType.Int).Value = SpecializationID;
                    command.Parameters.Add("@TrainerID", SqlDbType.Int).Value = TrainerID;
                    command.Parameters.Add("@CenterID", SqlDbType.Int).Value = CenterID;
                    command.Parameters.Add("@GroupName", SqlDbType.NVarChar, 50).Value = GroupName;
                    command.Parameters.Add("@StartDate", SqlDbType.Date).Value = StartDate;
                    command.Parameters.Add("@EndDate", SqlDbType.Date).Value = EndDate;
                    command.Parameters.Add("@MaxTrainees", SqlDbType.Int).Value = MaxTrainees;
                    command.Parameters.AddWithValue("@CurrentTrainees", (object)CurrentTrainees ?? 0);
                    command.Parameters.AddWithValue("@Schedule", (object)Schedule ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Room", (object)Room ?? DBNull.Value);
                    command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = GetStatusName(Status);

                    try
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Safely parse the GroupID (it might be DBNull if validation failed)
                                if (!reader.IsDBNull(reader.GetOrdinal("GroupID")))
                                {
                                    insertedID = reader.GetInt32(reader.GetOrdinal("GroupID"));
                                }

                                string status = reader["Status"].ToString();
                                string message = reader["Message"].ToString();

                                if (status == "SUCCESS" && insertedID.HasValue)
                                {
                                    EventLogger.LogInfo($"Successfully added Group: {GroupName} with ID: {insertedID}");
                                    return insertedID;
                                }
                                else
                                {
                                    // Now we can log the EXACT reason from SQL!
                                    EventLogger.LogWarning($"Failed to add Group: {GroupName}. Reason: {message}");
                                    return null;
                                }
                            }
                        }

                        EventLogger.LogWarning($"Failed to add Group: {GroupName}. No ID returned.");
                        return null;
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database error while adding Group: {GroupName}", sqlEx);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Unexpected error while adding Group: {GroupName}", ex);
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Asynchronously updates the details of an existing group using the 'SP_UpdateGroupByID' stored procedure.
        /// </summary>
        /// <param name="GroupID">The unique identifier of the group to be updated.</param>
        /// <param name="SpecializationID">The new Specialization ID.</param>
        /// <param name="TrainerID">The new Trainer ID.</param>
        /// <param name="CenterID">The new Center ID.</param>
        /// <param name="GroupName">The new name of the group.</param>
        /// <param name="StartDate">The new start date.</param>
        /// <param name="EndDate">The new end date.</param>
        /// <param name="MaxTrainees">The new maximum capacity.</param>
        /// <param name="CurrentTrainees">The current number of enrolled trainees.</param>
        /// <param name="Schedule">The updated schedule string.</param>
        /// <param name="Room">The updated room name.</param>
        /// <param name="Status">The updated status (Active, Cancelled, etc.).</param>
        /// <returns>
        /// Returns <c>true</c> if the update was successful and rows were affected.
        /// Returns <c>false</c> if the GroupID was not found or if database validation failed.
        /// </returns>
        public static async Task<bool> UpdateGroup(
            int GroupID,
            int SpecializationID,
            int TrainerID,
            int CenterID,
            string GroupName,
            DateTime StartDate,
            DateTime EndDate,
            int MaxTrainees,
            int CurrentTrainees,
            string Schedule,
            string Room,
            enStatus Status
            )
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("SP_UpdateGroupByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@GroupID", SqlDbType.Int).Value = GroupID;
                    command.Parameters.Add("@SpecializationID", SqlDbType.Int).Value = SpecializationID;
                    command.Parameters.Add("@TrainerID", SqlDbType.Int).Value = TrainerID;
                    command.Parameters.Add("@CenterID", SqlDbType.Int).Value = CenterID;
                    command.Parameters.Add("@GroupName", SqlDbType.NVarChar, 50).Value = GroupName;
                    command.Parameters.Add("@StartDate", SqlDbType.Date).Value = StartDate;
                    command.Parameters.Add("@EndDate", SqlDbType.Date).Value = EndDate;
                    command.Parameters.Add("@MaxTrainees", SqlDbType.Int).Value = MaxTrainees;
                    command.Parameters.AddWithValue("@CurrentTrainees", (object)CurrentTrainees ?? 0);
                    command.Parameters.AddWithValue("@Schedule", (object)Schedule ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Room", (object)Room ?? DBNull.Value);
                    command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = GetStatusName(Status);
                    command.Parameters.Add("@ModifiedByUserID", SqlDbType.Int).Value = 1; // TODO: Replace with actual user ID from context

                    try
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int rowsAffected = reader["RowsAffected"] as int? ?? 0;
                                string status = reader["Status"].ToString();
                                string message = reader["Message"].ToString();

                                if (status == "SUCCESS")
                                {
                                    EventLogger.LogInfo($"Successfully updated Group ID: {GroupID}. {message}");
                                    return rowsAffected > 0;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"Update failed for Group ID {GroupID}: {message}");
                                    return false;
                                }
                            }
                            else
                            {
                                EventLogger.LogError("No result returned from SP_UpdateGroupByID.");
                                return false;
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database error while Updating Group: {GroupName}", sqlEx);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Unexpected error while Updating Group: {GroupName}", ex);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a paginated list of Groups from the database based on specified filters.
        /// </summary>
        /// <remarks>
        /// This function executes the <c>SP_GetGroups</c> stored procedure, which returns 
        /// two result sets: the paginated list of groups (first set) and the total record count (second set).
        /// It uses <see cref="System.Data.SqlClient.SqlDataReader.NextResultAsync"/> to read both sets efficiently.
        /// </remarks>
        /// <param name="OnlyActive">If true, filters results to include only active groups (IsActive = 1).</param>
        /// <param name="SearchQuery">Optional search term used to filter by GroupName or Room.</param>
        /// <param name="StatusFilter">Optional status string used to filter results (e.g., "Active", "Pending").</param>
        /// <param name="pageNumber">The 1-based index of the page to retrieve.</param>
        /// <param name="pageSize">The maximum number of records per page.</param>
        /// <returns>A <see cref="GroupsPagedList"/> object containing the list of <see cref="GroupDTO"/>
        /// for the current page and the total count of records matching the filters.</returns>
        /// <exception cref="ApplicationException">Thrown when a database or general error occurs during retrieval.</exception>
        public static async Task<GroupsPagedList> GetGroupsList(
            bool OnlyActive,
            string SearchQuery,
            string StatusFilter,
            int pageNumber,
            int pageSize
        )
        {
            var PagedList = new GroupsPagedList
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SP_GetGroups", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@OnlyActive", SqlDbType.Bit).Value = OnlyActive;
                    cmd.Parameters.Add("@SearchQuery", SqlDbType.NVarChar, 100).Value = (object)SearchQuery ?? DBNull.Value;
                    cmd.Parameters.Add("@StatusFilter", SqlDbType.NVarChar, 20).Value = (object)StatusFilter ?? DBNull.Value;
                    cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
                    cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

                    try
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            // --- 1. Read FIRST Result Set ---
                            int groupIDIndex = reader.GetOrdinal("GroupID");
                            int groupNameIndex = reader.GetOrdinal("GroupName");
                            int statusIndex = reader.GetOrdinal("Status");
                            int scheduleIndex = reader.GetOrdinal("Schedule");
                            int roomIndex = reader.GetOrdinal("Room");
                            int startIndex = reader.GetOrdinal("StartDate");
                            int endIndex = reader.GetOrdinal("EndDate");
                            int currentTraineesIndex = reader.GetOrdinal("CurrentTrainees");
                            int maxTraineesIndex = reader.GetOrdinal("MaxTrainees");
                            int isActiveIndex = reader.GetOrdinal("IsActive");
                            int specializationNameIndex = reader.GetOrdinal("SpecializationName");
                            int trainerFullNameIndex = reader.GetOrdinal("TrainerName");
                            int centerNameIndex = reader.GetOrdinal("CenterName");
                            int createdAtIndex = reader.GetOrdinal("CreatedAt");
                            int descriptionIndex = reader.GetOrdinal("Description");
                            int activeMembersIndex = reader.GetOrdinal("ActiveMembers");
                            int updatedAtIndex = reader.GetOrdinal("UpdatedAt");
                            int baseFeeIndex = reader.GetOrdinal("BaseFee");

                            while (await reader.ReadAsync())
                            {
                                PagedList.Groups.Add(
                                    new GroupDTO
                                    {
                                        GroupID = reader.GetInt32(groupIDIndex),
                                        GroupName = reader.GetString(groupNameIndex),
                                        Status = reader.GetString(statusIndex),
                                        Schedule = reader.GetString(scheduleIndex),
                                        Room = reader.GetString(roomIndex),
                                        StartDate = reader.GetDateTime(startIndex),
                                        EndDate = reader.GetDateTime(endIndex),
                                        CurrentTrainees = reader.GetInt32(currentTraineesIndex),
                                        MaxTrainees = reader.GetInt32(maxTraineesIndex),
                                        IsActive = reader.GetBoolean(isActiveIndex),

                                        SpecializationName = reader.GetString(specializationNameIndex),
                                        CenterName = reader.GetString(centerNameIndex),
                                        CreatedAt = reader.GetDateTime(createdAtIndex),
                                        Description = reader.GetString(descriptionIndex),
                                        ActiveMembers = reader.GetInt32(activeMembersIndex),
                                        TrainerName = reader.GetString(trainerFullNameIndex),
                                        UpdatedAt = reader.IsDBNull(updatedAtIndex) ? DateTime.MinValue : reader.GetDateTime(updatedAtIndex),
                                        BaseFee = reader.IsDBNull(baseFeeIndex) ? 0 : reader.GetDecimal(baseFeeIndex),
                                    });
                            }


                            if (await reader.NextResultAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    // Reads the 'TotalCount' column from the single row result
                                    PagedList.TotalRecords = reader.GetInt32(0);
                                }
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError("Database error during group retrieval.", sqlEx);
                        throw new ApplicationException("Database error during group retrieval.", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error during group retrieval.", ex);
                        throw new ApplicationException("Error during group retrieval.", ex);
                    }
                }
            }

            return PagedList;
        }

        /// <summary>
        /// Retrieves a single Group Data Transfer Object (DTO) by its unique Group ID.
        /// </summary>
        /// <remarks>
        /// This function executes the <c>SP_GetGroupByID</c> stored procedure. 
        /// It correctly handles the single row result and returns <c>null</c> if no matching group is found.
        /// Performance optimization is achieved by calculating column ordinals once upon reading the result set.
        /// </remarks>
        /// <param name="groupID">The unique identifier of the Group to retrieve.</param>
        /// <returns>A <see cref="GroupDTO"/> object containing the group's details, or <c>null</c> if the group is not found or the input ID is invalid.</returns>
        /// <exception cref="ApplicationException">Thrown when a database or general error occurs during retrieval.</exception>
        public static async Task<GroupDTO> GetGroupByID(int groupID)
        {
            GroupDTO dto = null;

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SP_GetGroupByID", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@GroupID", SqlDbType.Int).Value = groupID;

                    try
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                dto = new GroupDTO
                                {
                                    GroupID = reader.GetInt32(reader.GetOrdinal("GroupID")),
                                    GroupName = reader.GetString(reader.GetOrdinal("GroupName")),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    Schedule = reader.GetString(reader.GetOrdinal("Schedule")),
                                    Room = reader.GetString(reader.GetOrdinal("Room")),
                                    StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                    EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                    CurrentTrainees = reader.GetInt32(reader.GetOrdinal("CurrentTrainees")),
                                    MaxTrainees = reader.GetInt32(reader.GetOrdinal("MaxTrainees")),
                                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                    TrainerID = reader.GetInt32(reader.GetOrdinal("TrainerID")),
                                    CenterID = reader.GetInt32(reader.GetOrdinal("CenterID")),

                                    SpecializationName = reader.GetString(reader.GetOrdinal("SpecializationName")),
                                    SpecializationID = reader.GetInt32(reader.GetOrdinal("SpecializationID")),
                                    CenterName = reader.GetString(reader.GetOrdinal("CenterName")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    ActiveMembers = reader.GetInt32(reader.GetOrdinal("ActiveMembers")),
                                    TrainerName = reader.IsDBNull(reader.GetOrdinal("TrainerName"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("TrainerName")),
                                    BaseFee = reader.IsDBNull(reader.GetOrdinal("BaseFee"))
                                              ? 0
                                              : reader.GetDecimal(reader.GetOrdinal("BaseFee")),
                                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                                        ? DateTime.MinValue
                                        : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                                };
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError("Database error during group retrieval.", sqlEx);
                        throw new ApplicationException("Database error during group retrieval.", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error during group retrieval.", ex);
                        throw new ApplicationException("Error during group retrieval.", ex);
                    }
                }
            }

            return dto;
        }

        public static async Task<List<GroupDTO>> GetGroupsBySpecializationDTO(int specializationID)
        {
            List<GroupDTO> dtoList = new List<GroupDTO>();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetGroupsBySpecialization", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@SpecializationID", SqlDbType.Int).Value = specializationID;

                    try
                    {
                        EventLogger.LogInfo("Attempting to retrieve all Groups from database.");
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var group = new GroupDTO();

                                // 1. Mandatory Fields (Integers/Booleans usually aren't null in DB)
                                group.GroupID = reader.GetInt32(reader.GetOrdinal("GroupID"));
                                group.SpecializationID = reader.GetInt32(reader.GetOrdinal("SpecializationID"));
                                group.TrainerID = reader.GetInt32(reader.GetOrdinal("TrainerID"));
                                group.CenterID = reader.GetInt32(reader.GetOrdinal("CenterID"));
                                group.GroupName = reader.IsDBNull(reader.GetOrdinal("GroupName")) ? "" : reader.GetString(reader.GetOrdinal("GroupName"));
                                group.StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate"));
                                group.EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"));
                                group.MaxTrainees = reader.GetInt32(reader.GetOrdinal("MaxTrainees"));
                                group.CurrentTrainees = reader.GetInt32(reader.GetOrdinal("CurrentTrainees"));
                                group.Schedule = reader.IsDBNull(reader.GetOrdinal("Schedule")) ? null : reader.GetString(reader.GetOrdinal("Schedule"));
                                group.Room = reader.IsDBNull(reader.GetOrdinal("Room")) ? null : reader.GetString(reader.GetOrdinal("Room"));
                                group.Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "Unknown" : reader.GetString(reader.GetOrdinal("Status"));
                                group.CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
                                group.IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));

                                dtoList.Add(group);
                            }
                        }
                        return dtoList;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error occurred while retrieving active Groups from database.", ex);
                        return new List<GroupDTO>();
                    }
                }
            }
        }

        /// <summary>
        /// Performs a soft delete operation on a Group record by setting the IsActive flag to false ('Archived').
        /// </summary>
        /// <remarks>
        /// This function executes the <c>SP_SoftDeleteGroup</c> stored procedure, which updates the Groups table 
        /// and logs the activity to the GroupActivities table. It processes the single status row returned by the procedure 
        /// to determine success or failure.
        /// </remarks>
        /// <param name="groupID">The unique identifier of the Group to archive.</param>
        /// <param name="userID">The ID of the user performing the deletion, used for auditing.</param>
        /// <returns>
        /// Returns <c>true</c> if the group was successfully archived or updated; 
        /// returns <c>false</c> if the operation failed (e.g., group already archived, invalid ID, or other database logic error).
        /// </returns>
        /// <exception cref="ApplicationException">Thrown when a database or general error occurs during the operation, 
        /// often wrapping an underlying <see cref="System.Data.SqlClient.SqlException"/>.</exception>
        public static async Task<bool> SoftDeleteGroup(int groupID, int userID)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("SP_SoftDeleteGroup", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@GroupID", SqlDbType.Int).Value = groupID;
                    cmd.Parameters.Add("@UpdatedByUserID", SqlDbType.Int).Value = userID;

                    try
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string status = reader.GetString(reader.GetOrdinal("Status"));
                                string message = reader.GetString(reader.GetOrdinal("Message"));

                                if (status == "SUCCESS")
                                {
                                    EventLogger.LogInfo($"Successfully Archived Group {groupID}");
                                    return true;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"Failed to Archived Group with ID: {groupID}");
                                    return false;
                                }
                            }
                            else
                            {
                                // This case handles an unexpected result where the SP executed but returned no rows.
                                EventLogger.LogError($"SP_SoftDeleteGroup executed for ID {groupID} but returned no status row.");
                                return false;
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database error Archiving the group {groupID}.", sqlEx);
                        throw new ApplicationException($"Database error Archiving the group {groupID}.", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error Archiving the group {groupID}.", ex);
                        throw new ApplicationException($"Error Archiving the group {groupID}.", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Permanently deletes a Group record and all associated dependent records (e.g., Subscriptions and Activities) from the database.
        /// </summary>
        /// <remarks>
        /// This function executes the <c>SP_HardDeleteGroup</c> stored procedure, which ensures complete removal
        /// of the group and prevents foreign key constraint violations by deleting dependent data first.
        /// **Use this function with caution as data loss is permanent.**
        /// </remarks>
        /// <param name="groupID">The unique identifier of the Group to permanently delete.</param>
        /// <returns>
        /// Returns <c>true</c> if the group was successfully deleted; 
        /// returns <c>false</c> if the deletion failed (e.g., group not found or a database error prevented deletion).
        /// </returns>
        /// <exception cref="ApplicationException">Thrown when a database or general error occurs during the operation, 
        /// often wrapping an underlying <see cref="System.Data.SqlClient.SqlException"/>.</exception>
        public static async Task<bool> HardDeleteGroup(int groupID, int userID)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("SP_DeleteGroup", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@GroupID", SqlDbType.Int).Value = groupID;

                    try
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string status = reader.GetString(reader.GetOrdinal("Status"));
                                string message = reader.GetString(reader.GetOrdinal("Message"));

                                if (status == "SUCCESS")
                                {
                                    EventLogger.LogInfo($"Successfully Deleted Group {groupID}");
                                    return true;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"Failed to Deleted Group with ID: {groupID}");
                                    return false;
                                }
                            }
                            else
                            {
                                // This case handles an unexpected result where the SP executed but returned no rows.
                                EventLogger.LogError($"SP_DeleteGroup executed for ID {groupID} but returned no status row.");
                                return false;
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database error Deleting the group {groupID}.", sqlEx);
                        throw new ApplicationException($"Database error Deleting the group {groupID}.", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error Deleting the group {groupID}.", ex);
                        throw new ApplicationException($"Error Deleting the group {groupID}.", ex);
                    }
                }
            }
        }
    }
}
