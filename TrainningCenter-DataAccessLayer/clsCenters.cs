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
    public static class clsCenters
    {
        public static async Task<IReadOnlyList<CenterDTO>> LoadAllCenters()
        {
            List<CenterDTO> centers = new List<CenterDTO>();
            using(SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetCenters", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        await connection.OpenAsync();
                        using(SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while(await reader.ReadAsync())
                            {
                                int centerIDIndex = reader.GetOrdinal("CenterID");
                                int centerNameIndex = reader.GetOrdinal("CenterName");
                                int centerAddressIndex = reader.GetOrdinal("Address");
                                int cityIndex = reader.GetOrdinal("City");
                                int phoneIndex = reader.GetOrdinal("Phone");
                                int emailIndex = reader.GetOrdinal("Email");
                                int managerIndex = reader.GetOrdinal("Manager");
                                int capacityIndex = reader.GetOrdinal("Capacity");
                                int facilitiesIndex = reader.GetOrdinal("Facilities");
                                int createdAtIndex = reader.GetOrdinal("CreatedAt");
                                int isActiveIndex = reader.GetOrdinal("IsActive");
                                int updatedAtIndex = reader.GetOrdinal("UpdatedAt");
                                int trainersCountIndex = reader.GetOrdinal("TrainersCount");
                                int groupsCountIndex = reader.GetOrdinal("GroupsCount");

                                centers.Add(new CenterDTO
                                {
                                    CenterID = reader.GetInt32(centerIDIndex),
                                    CenterName = reader.IsDBNull(centerNameIndex) ? null : reader.GetString(centerNameIndex),
                                    Address = reader.GetString(centerAddressIndex),
                                    City = reader.GetString(cityIndex),
                                    Phone = reader.GetString(phoneIndex),
                                    Email = reader.GetString(emailIndex),
                                    Manager = reader.GetString(managerIndex),
                                    Capacity = reader.GetInt32(capacityIndex),
                                    Facilities = reader.GetString(facilitiesIndex),
                                    CreatedAt = reader.GetDateTime(createdAtIndex),
                                    IsActive = reader.GetBoolean(isActiveIndex),
                                    UpdatedAt = reader.IsDBNull(updatedAtIndex) 
                                        ? (DateTime?)null 
                                        : reader.GetDateTime(updatedAtIndex),
                                    TrainersCount = reader.GetInt32(trainersCountIndex),
                                    GroupsCount = reader.GetInt32(groupsCountIndex)
                                });
                            }

                            return centers;
                        }
                    }
                    catch(SqlException sqlEx)
                    {
                        EventLogger.LogError("Database Error While try to retrieving the data", sqlEx);
                        throw new ApplicationException("Database Error While try to retrieving the data", sqlEx);
                    }
                    catch(Exception ex)
                    {
                        EventLogger.LogError("An Error While try to retrieving the data", ex);
                        throw new ApplicationException("An Error While try to retrieving the data", ex);
                    }
                }
            }
        }
        public static async Task<int?> AddNewCenter(
            string centerName,
            string address,
            string city,
            string phone,
            string email,
            string manager,
            int capacity,
            string facilities,
            bool isActive
        ) {
            int? insertedID = null;
            using(SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using(SqlCommand command = new SqlCommand("SP_AddNewCenter", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@CenterName", SqlDbType.NVarChar, 100).Value = centerName;
                    command.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = address;
                    command.Parameters.Add("@City", SqlDbType.NVarChar, 50).Value = city;
                    command.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value = phone;
                    command.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
                    command.Parameters.Add("@Manager", SqlDbType.NVarChar, 100).Value = manager;
                    command.Parameters.Add("@Capacity", SqlDbType.Int).Value = capacity;
                    command.Parameters.Add("@Facilities", SqlDbType.NVarChar, 100).Value = facilities;
                    command.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = DateTime.Now;
                    command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int centerIdIndex = reader.GetOrdinal("CenterID");
                                if (!reader.IsDBNull(centerIdIndex))
                                {
                                    insertedID = reader.GetInt32(centerIdIndex);
                                }
                            }

                            return insertedID;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError("Database Error While trying to add New Center", sqlEx);
                        throw new ApplicationException("Database Error While trying to add New Center", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error While trying to add New Center", ex);
                        throw new ApplicationException("Error While trying to add New Center", ex);
                    }
                }
            }
        }

        public static async Task<bool> UpdateCenter(
            int centerID,
            string centerName,
            string address,
            string city,
            string phone,
            string email,
            string manager,
            int capacity,
            string facilities,
            bool isActive)
        {
            int rowsAffected = 0;

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using(SqlCommand command = new SqlCommand("SP_UpdateCenter", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@CenterID", SqlDbType.Int).Value = centerID;
                    command.Parameters.Add("@CenterName", SqlDbType.NVarChar, 100).Value = centerName;
                    command.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = address;
                    command.Parameters.Add("@City", SqlDbType.NVarChar, 50).Value = city;
                    command.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value = phone;
                    command.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
                    command.Parameters.Add("@Manager", SqlDbType.NVarChar, 100).Value = manager;
                    command.Parameters.Add("@Capacity", SqlDbType.Int).Value = capacity;
                    command.Parameters.Add("@Facilities", SqlDbType.NVarChar, 100).Value = facilities;
                    command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();

                        return (rowsAffected > 0);
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database Error While trying to update Center ID:{centerID}", sqlEx);
                        throw new ApplicationException($"Database Error While trying to update Center ID:{centerID}", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error While trying to update Center ID:{centerID}", ex);
                        throw new ApplicationException($"Error While trying to update Center ID:{centerID}", ex);
                    }
                }
            }
        }

        public static async Task<bool> DeleteCenter(int centerID)
        {
            int rowsAffected = 0;

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_DeleteCenter", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@CenterID", SqlDbType.Int).Value = centerID;

                    try
                    {
                        await connection.OpenAsync();
                        rowsAffected = await command.ExecuteNonQueryAsync();

                        return (rowsAffected > 0);
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database Error While trying to delete Center ID:{centerID}", sqlEx);
                        throw new ApplicationException($"Database Error While trying to delete Center ID:{centerID}", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error While trying to delete Center ID:{centerID}", ex);
                        throw new ApplicationException($"Error While trying to delete Center ID:{centerID}", ex);
                    }
                }
            }
        }

        public static async Task<CenterDTO> GetCenterByID(int centerID)
        {
            CenterDTO center = null;
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetCenterByID", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CenterID", SqlDbType.Int).Value = centerID;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Mapping logic (same as in LoadAllCenters, but only mapping one row)
                                int centerNameIndex = reader.GetOrdinal("CenterName");
                                int centerAddressIndex = reader.GetOrdinal("Address");
                                int cityIndex = reader.GetOrdinal("City");
                                int phoneIndex = reader.GetOrdinal("Phone");
                                int emailIndex = reader.GetOrdinal("Email");
                                int managerIndex = reader.GetOrdinal("Manager");
                                int capacityIndex = reader.GetOrdinal("Capacity");
                                int facilitiesIndex = reader.GetOrdinal("Facilities");
                                int createdAtIndex = reader.GetOrdinal("CreatedAt");
                                int isActiveIndex = reader.GetOrdinal("IsActive");
                                int updatedAtIndex = reader.GetOrdinal("UpdatedAt");
                                int trainersCountIndex = reader.GetOrdinal("TrainersCount");
                                int groupsCountIndex = reader.GetOrdinal("GroupsCount");

                                center = new CenterDTO
                                {
                                    CenterID = centerID,
                                    CenterName = reader.IsDBNull(centerNameIndex) ? null : reader.GetString(centerNameIndex),
                                    Address = reader.GetString(centerAddressIndex),
                                    City = reader.GetString(cityIndex),
                                    Phone = reader.GetString(phoneIndex),
                                    Email = reader.GetString(emailIndex),
                                    Manager = reader.GetString(managerIndex),
                                    Capacity = reader.GetInt32(capacityIndex),
                                    Facilities = reader.IsDBNull(facilitiesIndex) ? null : reader.GetString(facilitiesIndex),
                                    CreatedAt = reader.GetDateTime(createdAtIndex),
                                    IsActive = reader.GetBoolean(isActiveIndex),
                                    UpdatedAt = reader.IsDBNull(updatedAtIndex)
                                        ? (DateTime?)null
                                        : reader.GetDateTime(updatedAtIndex),
                                    TrainersCount = reader.GetInt32(trainersCountIndex),
                                    GroupsCount = reader.GetInt32(groupsCountIndex)
                                };
                            }
                            return center;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database Error While trying to retrieve Center ID:{centerID}", sqlEx);
                        throw new ApplicationException($"Database Error While trying to retrieve Center ID:{centerID}", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error While trying to retrieve Center ID:{centerID}", ex);
                        throw new ApplicationException($"Error While trying to retrieve Center ID:{centerID}", ex);
                    }
                }
            }
        }
    }
}
