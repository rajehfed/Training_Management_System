using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer.Helpers;

namespace TrainningCenter_DataAccessLayer
{
    public static class clsPerson
    {
        /// <summary>
        /// This method retrieves all active persons from the database.
        /// </summary>
        /// <returns>
        /// method returns a DataTable containing all active persons.
        /// </returns>
        public static async Task<DataTable> GetAllPeople()
        {
            DataTable dt = new DataTable();

            using(SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllActivePersons", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        EventLogger.LogInfo("Attempting to retrieve all people from database.");

                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }
                        EventLogger.LogInfo($"Successfully retrieved {dt.Rows.Count} people from database.");
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Failed to retrieve people from database.", ex);
                        return null;
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// this method adds a new person to the database.
        /// </summary>
        /// <param name="firstName">
        /// The first name of the person to be added.
        /// </param>
        /// <param name="lastName">
        /// the last name of the person to be added.
        /// </param>
        /// <param name="NationalNo">
        /// the national number of the person to be added.
        /// </param>
        /// <param name="Nationality">
        /// The Nationality of the person to be added.
        /// </param>
        /// <param name="DateOfBirth">
        /// the date of birth of the person to be added.
        /// </param>
        /// <param name="Gender">
        /// the gender of the person to be added.
        /// </param>
        /// <param name="PhoneNumber">
        /// the phone number of the person to be added.
        /// </param>
        /// <param name="Email">
        /// the email of the person to be added.
        /// </param>
        /// <param name="Address">
        /// the address of the person to be added.
        /// </param>
        /// <param name="ImagePath">
        /// the image path of the person to be added.
        /// </param>
        /// <returns>
        /// the method returns the ID of the newly added person if successful, otherwise -1.
        /// </returns>
        public static async Task<int> AddNewPerson(
            string firstName,
            string lastName,
            string NationalNo,
            string Nationality,
            DateTime DateOfBirth,
            char Gender,
            string PhoneNumber,
            string Email,
            string Address,
            string ImagePath
        )
        {
            int insertedID = -1;

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("SP_AddNewPerson", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50).Value = firstName;
                    command.Parameters.Add("@LastName", SqlDbType.NVarChar, 50).Value = lastName;
                    command.Parameters.Add("@NationalNo", SqlDbType.NVarChar, 50).Value = NationalNo;
                    command.Parameters.Add("@Nationality", SqlDbType.NVarChar, 50).Value = Nationality;
                    command.Parameters.Add("@DateOfBirth", SqlDbType.Date).Value = DateOfBirth;
                    command.Parameters.Add("@Gender", SqlDbType.NChar, 1).Value = Gender;
                    command.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20).Value = PhoneNumber;
                    command.Parameters.AddWithValue("@Email", (object)Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object)Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ImagePath", (object)ImagePath ?? DBNull.Value);

                    try
                    {
                        object result = await command.ExecuteScalarAsync();

                        if(result != null && int.TryParse(result.ToString(), out insertedID))
                        {
                            EventLogger.LogInfo($"Successfully added person: {firstName} {lastName} with ID: {insertedID}");
                            return insertedID;
                        }

                        EventLogger.LogWarning($"Failed to add person: {firstName} {lastName}. No ID returned.");
                        return -1;
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database error while adding person: {firstName} {lastName}", sqlEx);
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Unexpected error while adding person: {firstName} {lastName}", ex);
                        return -1;
                    }
                }
            }
        }

        /// <summary>
        /// This method updates an existing person's details in the database.
        /// </summary>
        /// <param name="PersonID">
        /// the ID of the person to be updated.
        /// </param>
        public static async Task<bool> UpdatePerson(
            int PersonID,
            string FirstName,
            string LastName,
            string NationalNo,
            string Nationality,
            DateTime DateOfBirth,
            char Gender,
            string PhoneNumber,
            string Email,
            string Address,
            string ImagePath
            )
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                await connection.OpenAsync();

                using(SqlCommand command = new SqlCommand("SP_UpdatePerson", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                    command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50).Value = FirstName;
                    command.Parameters.Add("@LastName", SqlDbType.NVarChar, 50).Value = LastName;
                    command.Parameters.Add("@NationalNo", SqlDbType.NVarChar, 50).Value = NationalNo;
                    command.Parameters.Add("@Nationality", SqlDbType.NVarChar, 50).Value = Nationality;
                    command.Parameters.Add("@DateOfBirth", SqlDbType.Date).Value = DateOfBirth;
                    command.Parameters.Add("@Gender", SqlDbType.NChar, 1).Value = Gender;
                    command.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20).Value = PhoneNumber;
                    command.Parameters.AddWithValue("@Email", (object)Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object)Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ImagePath", (object)ImagePath ?? DBNull.Value);

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int rowsAffected = reader["RowsAffected"] as int? ?? 0;
                                string status = reader["Status"].ToString();
                                string message = reader["Message"].ToString();

                                if(status == "SUCCESS")
                                {
                                    EventLogger.LogInfo($"Successfully updated person ID: {PersonID}. {message}");
                                    return rowsAffected > 0;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"Update failed for person ID {PersonID}: {message}");
                                    return false;
                                }
                            }
                            else
                            {
                                EventLogger.LogError("No result returned from SP_UpdatePerson.");
                                return false;
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError("Database error while updating person.", sqlEx);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Unexpected error while updating person.", ex);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a specific person by their ID from the database.
        /// </summary>
        /// <param name="personID">The ID of the person to retrieve.</param>
        /// <returns>
        /// Returns DataRow if person found, otherwise null.
        /// </returns>
        public static async Task<DataRow> GetPersonByID(int PersonID)
        {
            // التحقق من المدخلات
            if (PersonID <= 0) {
                EventLogger.LogWarning($"Invalid PersonID: {PersonID}. PersonID must be greater than 0.");
                return null;
            }
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = DataBaseSettings.GetConnection())
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand("SP_GetPersonByID", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;

                        EventLogger.LogInfo($"Attempting to retrieve person with ID: {PersonID} from database.");

                        using (SqlDataReader reader = await command.ExecuteReaderAsync()) 
                        {
                            dt.Load(reader);
                        }

                        if (dt.Rows.Count > 0) {
                            EventLogger.LogInfo($"Successfully retrieved person with ID: {PersonID}");
                            return dt.Rows[0];
                        }
                        else {
                            EventLogger.LogWarning($"No person found with ID: {PersonID}");
                            return null;
                        }
                    }
                }
            }
            catch(SqlException sqlEx) {
                EventLogger.LogError($"Database error while retrieving person with ID: {PersonID}", sqlEx);
                return null;
            }
            catch (Exception ex) {
                EventLogger.LogError($"Unexpected error while retrieving person with ID: {PersonID}", ex);
                return null;
            }
        }

        /// <summary>
        /// Retrieves a specific person by their NationlNo from the database.
        /// </summary>
        /// <param name="nationalNo">The National Number of the person to retrieve.</param>
        /// <returns>
        /// Returns DataRow if person found, otherwise null.
        /// </returns>
        public static async Task<DataRow> GetPersonByNationalNo(string NationalNo)
        {
            // التحقق من المدخلات
            if (string.IsNullOrEmpty(NationalNo))
            {
                EventLogger.LogWarning($"The National No Must Be not Empty or Null!!");
                return null;
            }
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = DataBaseSettings.GetConnection())
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand("SP_GetPersonByNationalNo", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@NationalNo", SqlDbType.NVarChar).Value = NationalNo;

                        EventLogger.LogInfo($"Attempting to retrieve person with National No: {NationalNo} from database.");

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }

                        if (dt.Rows.Count > 0)
                        {
                            EventLogger.LogInfo($"Successfully retrieved person with National No: {NationalNo}");
                            return dt.Rows[0];
                        }
                        else
                        {
                            EventLogger.LogWarning($"No person found with National No: {NationalNo}");
                            return null;
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                EventLogger.LogError($"Database error while retrieving person with National No: {NationalNo}", sqlEx);
                return null;
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"Unexpected error while retrieving person with National No: {NationalNo}", ex);
                return null;
            }
        }

        public static async Task<bool> CheckIfPersonExists(int personID)
        {
            try
            {
                using (SqlConnection connection = DataBaseSettings.GetConnection())
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Persons WHERE PersonID = @PersonID", connection))
                    {
                        command.Parameters.Add("@PersonID", SqlDbType.Int).Value = personID;

                        int count = (int)await command.ExecuteScalarAsync();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"Error checking if person with ID: {personID} exists", ex);
                return false;
            }
        }

        /// <summary>
        /// Deletes a person from the database by their ID
        /// </summary>
        /// <param name="iD">The ID of the person to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public static async Task<bool> DeletePerson(int ID)
        {
            if(ID <= 0)
            {
                EventLogger.LogWarning($"The ID No Must Be not negative or zero!!");
                return false;
            }
            int rowsAffected = 0;

            try
            {
                using(SqlConnection connection = DataBaseSettings.GetConnection())
                {
                    await connection.OpenAsync();

                    // First, check if the person exists
                    bool personExists = await CheckIfPersonExists(ID);
                    if (!personExists) {
                        EventLogger.LogWarning($"Attempted to delete non-existent person with ID: {ID}");
                        return false;
                    }
                    using (SqlCommand command = new SqlCommand("SP_DeletePerson", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@PersonID", SqlDbType.Int).Value = ID;

                        EventLogger.LogInfo($"Attempting to delete person with ID: {ID} from database.");

                        rowsAffected = await command.ExecuteNonQueryAsync();

                        bool isDelete = rowsAffected > 0;

                        if(isDelete)
                            EventLogger.LogInfo($"Successfully deleted person with ID: {ID}");
                        else
                            EventLogger.LogWarning($"No person was deleted. Person with ID: {ID} may not exist or has dependencies.");

                        return isDelete;
                    }
                }
            }
            catch(SqlException sqlEx) {
                EventLogger.LogError("An Error was occured", sqlEx);
                return false;
            }
            catch(Exception ex) {
                EventLogger.LogError("An Error was occured", ex);
                return false;
            }
        }
    }
}
