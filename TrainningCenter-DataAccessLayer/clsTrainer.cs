using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer.Helpers;
using TrainningCenter_Entities;

namespace TrainningCenter_DataAccessLayer
{
    public static class clsTrainer
    {
        /// <summary>
        /// Retrieves a comprehensive list of all trainers from the database.
        /// </summary>
        /// <returns>
        /// A List of <see cref="TrainerDTO"/> containing detailed trainer information.
        /// Returns an empty list if no trainers are found.
        /// </returns>
        /// <exception cref="ApplicationException">Thrown on database connection failure or SQL errors.</exception>
        public static async Task<List<TrainerDTO>> GetAllTrainees()
        {
            List<TrainerDTO> dtos = new List<TrainerDTO>();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllTrainees", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            int trainerIDIndex = reader.GetOrdinal("TrainerID");
                            int personIDIndex = reader.GetOrdinal("PersonID");
                            int trainerNameIndex = reader.GetOrdinal("TrainerName");
                            int experienceYearsIndex = reader.GetOrdinal("ExperienceYears");
                            int qualificationsIndex = reader.GetOrdinal("Qualifications");
                            int specializationIndex = reader.GetOrdinal("Specialization");
                            int hireDateIndex = reader.GetOrdinal("HireDate");
                            int salaryIndex = reader.GetOrdinal("Salary");
                            int ratingIndex = reader.GetOrdinal("Rating");
                            int biographyIndex = reader.GetOrdinal("Biography");
                            int centerIDIndex = reader.GetOrdinal("CenterID");
                            int centerIndex = reader.GetOrdinal("Center");
                            int createdAtIndex = reader.GetOrdinal("CreatedAt");
                            int isActiveIndex = reader.GetOrdinal("IsActive");
                            int updatedAtIndex = reader.GetOrdinal("UpdatedAt");

                            int firstNameIndex = reader.GetOrdinal("FirstName");
                            int lastNameIndex = reader.GetOrdinal("LastName");
                            int nationalNoIndex = reader.GetOrdinal("NationalNo");
                            int nationalityIndex = reader.GetOrdinal("Nationality");
                            int dateOfBirthIndex = reader.GetOrdinal("DateOfBirth");
                            int genderIndex = reader.GetOrdinal("Gender");
                            int phoneIndex = reader.GetOrdinal("PhoneNumber");
                            int emailIndex = reader.GetOrdinal("Email");
                            int addressIndex = reader.GetOrdinal("Address");
                            int imageAddressIndex = reader.GetOrdinal("ImagePath");
                            int personCreatedAtIndex = reader.GetOrdinal("PersonCreatedAt");
                            int personUpdatedAtIndex = reader.GetOrdinal("PersonUpdatedAt");
                            int isActivePersonIndex = reader.GetOrdinal("IsActivePerson");

                            while (await reader.ReadAsync())
                            {
                                dtos.Add(new TrainerDTO
                                {
                                    TrainerID = reader.GetInt32(trainerIDIndex),
                                    PersonID = reader.GetInt32(personIDIndex),
                                    TrainerName = reader.GetString(trainerNameIndex),
                                    ExperienceYears = reader.IsDBNull(experienceYearsIndex) ? 0 : reader.GetInt32(experienceYearsIndex),
                                    Qualifications = reader.GetString(qualificationsIndex),
                                    Specialization = reader.GetString(specializationIndex),
                                    HireDate = reader.GetDateTime(hireDateIndex),
                                    Salary = reader.GetDecimal(salaryIndex),
                                    Rating = reader.GetDecimal(ratingIndex),
                                    Biography = reader.IsDBNull(biographyIndex) ? null : reader.GetString(biographyIndex),
                                    CenterID = reader.GetInt32(centerIDIndex),
                                    Center = reader.GetString(centerIndex),
                                    CreatedAt = reader.GetDateTime(createdAtIndex),
                                    IsActive = reader.GetBoolean(isActiveIndex),
                                    UpdatedAt = reader.IsDBNull(updatedAtIndex) ? DateTime.MinValue : reader.GetDateTime(updatedAtIndex),
                                    CurrentPerson = new PersonDTO
                                    {
                                        Id = reader.GetInt32(personIDIndex),
                                        // ✅ Add null checks for ALL string fields:
                                        FirstName = reader.IsDBNull(firstNameIndex)
                                                    ? string.Empty : reader.GetString(firstNameIndex),
                                        LastName = reader.IsDBNull(lastNameIndex)
                                                    ? string.Empty : reader.GetString(lastNameIndex),
                                        NationalNo = reader.IsDBNull(nationalNoIndex)
                                                    ? string.Empty : reader.GetString(nationalNoIndex),
                                        Nationality = reader.IsDBNull(nationalityIndex)
                                                    ? string.Empty : reader.GetString(nationalityIndex),
                                        DateOfBirth = reader.IsDBNull(dateOfBirthIndex)
                                                    ? DateTime.MinValue : reader.GetDateTime(dateOfBirthIndex),
                                        Gender = reader.IsDBNull(genderIndex)
                                                ? "M" : reader.GetString(genderIndex), // Default to "M" if null
                                        Phone = reader.IsDBNull(phoneIndex)
                                                ? string.Empty : reader.GetString(phoneIndex),
                                        Email = reader.IsDBNull(emailIndex)
                                                ? string.Empty : reader.GetString(emailIndex),
                                        Address = reader.IsDBNull(addressIndex)
                                                ? string.Empty : reader.GetString(addressIndex),
                                        ImagePath = reader.IsDBNull(imageAddressIndex)
                                                    ? string.Empty : reader.GetString(imageAddressIndex),
                                        PersonCreatedAt = reader.GetDateTime(personCreatedAtIndex),
                                        PersonUpdatedAt = reader.IsDBNull(personUpdatedAtIndex)
                                                        ? DateTime.MinValue : reader.GetDateTime(personUpdatedAtIndex),
                                        IsActivePerson = reader.GetBoolean(isActivePersonIndex)
                                    }
                                });
                            }

                            return dtos;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError("Database Error While try to retieve the Trainees Data", sqlEx);
                        throw new ApplicationException("Database Error While try to retieve the Trainees Data", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("An Error Ocured While try to retieve the Trainees Data", ex);
                        throw new ApplicationException("An Error Ocured While try to retieve the Trainees Data", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Registers a new Trainer in the system.
        /// </summary>
        /// <param name="PersonID">The ID of the existing Person record.</param>
        /// <param name="experienceYears">Number of years of professional experience.</param>
        /// <param name="qualifications">Degrees or certifications held.</param>
        /// <param name="Specialization">Primary area of expertise.</param>
        /// <param name="HireDate">Date of employment start.</param>
        /// <param name="Salary">Monthly or yearly compensation.</param>
        /// <param name="Rating">Initial performance rating (0.0 to 5.0).</param>
        /// <param name="Biography">Short bio or description.</param>
        /// <param name="CenterID">ID of the Training Center (nullable).</param>
        /// <returns>The new <c>TrainerID</c> if successful, otherwise <c>null</c>.</returns>
        public static async Task<int?> AddNewTrainer(
            int PersonID,
            int experienceYears,
            string qualifications,
            string Specialization,
            DateTime HireDate,
            decimal Salary,
            decimal Rating,
            string Biography,
            int? CenterID
        )
        {
            int? insertedID = null;

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewTrainer", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                    command.Parameters.Add("@ExperienceYears", SqlDbType.Int).Value = experienceYears;
                    command.Parameters.Add("@Qualifications", SqlDbType.NVarChar, 500).Value = qualifications;
                    command.Parameters.Add("@Specialization", SqlDbType.NVarChar, 100).Value = Specialization;
                    command.Parameters.Add("@HireDate", SqlDbType.Date).Value = HireDate;
                    command.Parameters.Add("@Salary", SqlDbType.Decimal).Value = Salary;
                    command.Parameters.Add("@Rating", SqlDbType.Decimal).Value = Rating;
                    command.Parameters.Add("@Biography", SqlDbType.NText).Value = Biography;
                    command.Parameters.Add("@CenterID", SqlDbType.Int).Value = CenterID;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal("TrainerID")))
                                {
                                    insertedID = reader.GetInt32(reader.GetOrdinal("TrainerID"));
                                }

                                string status = reader.GetString(reader.GetOrdinal("Status"));
                                string message = reader.GetString(reader.GetOrdinal("Message"));

                                if (status == "SUCCESS")
                                {
                                    EventLogger.LogInfo($"Successfully Added Trainer with ID: {insertedID}");
                                    return insertedID;
                                }
                                else
                                {
                                    // Now we can log the EXACT reason from SQL!
                                    EventLogger.LogWarning($"Failed to add Trainer. Reason: {message}");
                                    return null;
                                }
                            }
                        }

                        EventLogger.LogWarning($"Failed to add Trainer. No ID Returned");
                        return null;
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError("Database Error While Adding the New Trainer", sqlEx);
                        throw new ApplicationException("Database Error While Adding the New Trainer", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error While Adding the New Trainer", ex);
                        throw new ApplicationException("Error While Adding the New Trainer", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Updates an existing Trainer's profile details.
        /// </summary>
        /// <returns><c>true</c> if update was successful; otherwise <c>false</c>.</returns>
        public static async Task<bool> UpdateTrainer(
            int TrainerID,
            int PersonID,
            int ExperienceYears,
            string Qualifications,
            string Specialization,
            DateTime HireDate,
            decimal Salary,
            decimal Rating,
            string Biography,
            int? CenterID,
            bool IsActive
        )
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateTrainer", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@TrainerID", SqlDbType.Int).Value = TrainerID;
                    command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                    command.Parameters.Add("@ExperienceYears", SqlDbType.Int).Value = ExperienceYears;
                    command.Parameters.Add("@Qualifications", SqlDbType.NVarChar, 500).Value = Qualifications;
                    command.Parameters.Add("@Specialization", SqlDbType.NVarChar, 100).Value = Specialization;
                    command.Parameters.Add("@HireDate", SqlDbType.Date).Value = HireDate;
                    command.Parameters.Add("@Salary", SqlDbType.Decimal).Value = Salary;
                    command.Parameters.Add("@Rating", SqlDbType.Decimal).Value = Rating;
                    command.Parameters.Add("@Biography", SqlDbType.NText).Value = Biography;
                    command.Parameters.Add("@CenterID", SqlDbType.Int).Value = CenterID;
                    command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = IsActive;

                    try
                    {
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int rowsAffected = reader.GetInt32(reader.GetOrdinal("RowsAffected"));
                                string status = reader.GetString(reader.GetOrdinal("Status"));
                                string message = reader.GetString(reader.GetOrdinal("Message"));

                                if (status == "SUCCESS")
                                {
                                    EventLogger.LogInfo($"Successfully updated Trainer ID: {TrainerID}. {message}");
                                    return rowsAffected > 0;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"Update failed for Trainer ID {TrainerID}: {message}");
                                    return false;
                                }
                            }
                            else
                            {
                                EventLogger.LogError("No result returned from SP_UpdateTrainer.");
                                return false;
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database error while Updating Trainer: {TrainerID}", sqlEx);
                        throw new ApplicationException($"Database error while Updating Trainer: {TrainerID}", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error while Updating Trainer: {TrainerID}", ex);
                        throw new ApplicationException($"Error while Updating Trainer: {TrainerID}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Fetches a specific Trainer by their unique ID.
        /// </summary>
        /// <param name="TrainerID">The ID of the trainer to retrieve.</param>
        /// <returns>A <see cref="TrainerDTO"/> populated with data, or <c>null</c> if not found.</returns>
        public static async Task<TrainerDTO> GetTrainerByID(int TrainerID)
        {
            TrainerDTO dto = null;

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetTrainerByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@TrainerID", SqlDbType.Int).Value = TrainerID;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            int trainerIDIndex = reader.GetOrdinal("TrainerID");
                            int personIDIndex = reader.GetOrdinal("PersonID");
                            int experienceYearsIndex = reader.GetOrdinal("ExperienceYears");
                            int qualificationsIndex = reader.GetOrdinal("Qualifications");
                            int specializationIndex = reader.GetOrdinal("Specialization");
                            int hireDateIndex = reader.GetOrdinal("HireDate");
                            int salaryIndex = reader.GetOrdinal("Salary");
                            int ratingIndex = reader.GetOrdinal("Rating");
                            int biographyIndex = reader.GetOrdinal("Biography");
                            int centerIDIndex = reader.GetOrdinal("CenterID");
                            int createdAtIndex = reader.GetOrdinal("CreatedAt");
                            int isActiveIndex = reader.GetOrdinal("IsActive");
                            int updatedAtIndex = reader.GetOrdinal("UpdatedAt");

                            int firstNameIndex = reader.GetOrdinal("FirstName");
                            int lastNameIndex = reader.GetOrdinal("LastName");
                            int nationalNoIndex = reader.GetOrdinal("NationalNo");
                            int nationalityIndex = reader.GetOrdinal("Nationality");
                            int dateOfBirthIndex = reader.GetOrdinal("DateOfBirth");
                            int genderIndex = reader.GetOrdinal("Gender");
                            int phoneIndex = reader.GetOrdinal("PhoneNumber");
                            int emailIndex = reader.GetOrdinal("Email");
                            int addressIndex = reader.GetOrdinal("Address");
                            int imageAddressIndex = reader.GetOrdinal("ImagePath");
                            int personCreatedAtIndex = reader.GetOrdinal("PersonCreatedAt");
                            int personUpdatedAtIndex = reader.GetOrdinal("PersonUpdatedAt");
                            int isActivePersonIndex = reader.GetOrdinal("IsActivePerson");

                            if (await reader.ReadAsync())
                            {
                                dto = new TrainerDTO
                                {
                                    TrainerID = TrainerID,
                                    PersonID = reader.GetInt32(personIDIndex),
                                    ExperienceYears = reader.GetInt32(experienceYearsIndex),
                                    Qualifications = reader.GetString(qualificationsIndex),
                                    Specialization = reader.GetString(specializationIndex),
                                    HireDate = reader.GetDateTime(hireDateIndex),
                                    Salary = reader.GetDecimal(salaryIndex),
                                    Rating = reader.GetDecimal(ratingIndex),
                                    Biography = reader.GetString(biographyIndex),
                                    CenterID = reader.IsDBNull(centerIDIndex)
                                        ? -1
                                        : reader.GetInt32(centerIDIndex),
                                    CreatedAt = reader.GetDateTime(createdAtIndex),
                                    IsActive = reader.GetBoolean(isActiveIndex),
                                    UpdatedAt = reader.IsDBNull(updatedAtIndex)
                                        ? DateTime.MinValue
                                        : reader.GetDateTime(updatedAtIndex),
                                    CurrentPerson = new PersonDTO
                                    {
                                        Id = reader.GetInt32(personIDIndex),
                                        // ✅ Add null checks for ALL string fields:
                                        FirstName = reader.IsDBNull(firstNameIndex)
                                                    ? string.Empty : reader.GetString(firstNameIndex),
                                        LastName = reader.IsDBNull(lastNameIndex)
                                                    ? string.Empty : reader.GetString(lastNameIndex),
                                        NationalNo = reader.IsDBNull(nationalNoIndex)
                                                    ? string.Empty : reader.GetString(nationalNoIndex),
                                        Nationality = reader.IsDBNull(nationalityIndex)
                                                    ? string.Empty : reader.GetString(nationalityIndex),
                                        DateOfBirth = reader.IsDBNull(dateOfBirthIndex)
                                                    ? DateTime.MinValue : reader.GetDateTime(dateOfBirthIndex),
                                        Gender = reader.IsDBNull(genderIndex)
                                                ? "M" : reader.GetString(genderIndex), // Default to "M" if null
                                        Phone = reader.IsDBNull(phoneIndex)
                                                ? string.Empty : reader.GetString(phoneIndex),
                                        Email = reader.IsDBNull(emailIndex)
                                                ? string.Empty : reader.GetString(emailIndex),
                                        Address = reader.IsDBNull(addressIndex)
                                                ? string.Empty : reader.GetString(addressIndex),
                                        ImagePath = reader.IsDBNull(imageAddressIndex)
                                                    ? string.Empty : reader.GetString(imageAddressIndex),
                                        PersonCreatedAt = reader.GetDateTime(personCreatedAtIndex),
                                        PersonUpdatedAt = reader.IsDBNull(personUpdatedAtIndex)
                                                        ? DateTime.MinValue : reader.GetDateTime(personUpdatedAtIndex),
                                        IsActivePerson = reader.GetBoolean(isActivePersonIndex)
                                    }
                                };
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        EventLogger.LogError($"Database Error during Trainer ID {TrainerID} retrieval.", ex);
                        throw new ApplicationException($"Database Error during Trainer ID {TrainerID} retrieval.", ex);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error during Trainer ID {TrainerID} retrieval.", ex);
                        throw new ApplicationException($"Error during Trainer ID {TrainerID} retrieval.", ex);
                    }
                }
            }

            return dto;
        }

        /// <summary>
        /// Removes a trainer from the system.
        /// </summary>
        /// <param name="TrainerID">The ID of the trainer to delete.</param>
        /// <param name="ForceDelete">If set to <c>true</c>, bypasses dependency checks (use with caution).</param>
        /// <returns><c>true</c> if deleted successfully.</returns>
        public static async Task<bool> DeleteTrainer(int TrainerID, bool ForceDelete = false)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_DeleteTrainer", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@TrainerID", SqlDbType.Int).Value = TrainerID;
                    command.Parameters.Add("@ForceDelete", SqlDbType.Bit).Value = ForceDelete;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int rowsAffected = reader.GetInt32(reader.GetOrdinal("RowsAffected"));
                                string status = reader.GetString(reader.GetOrdinal("Status"));
                                string message = reader.GetString(reader.GetOrdinal("Message"));

                                if (status == "SUCCESS")
                                {
                                    EventLogger.LogInfo($"Successfully Archived the Trainer With ID {TrainerID}.");
                                    return rowsAffected > 0;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"Failed to Archived Trainer with ID: {TrainerID}");
                                    return false;
                                }
                            }
                            else
                            {
                                // This case handles an unexpected result where the SP executed but returned no rows.
                                EventLogger.LogError($"SP_DeleteTrainer executed for ID {TrainerID} but returned no status row.");
                                return false;
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        EventLogger.LogError($"Database Error Archiving the Trainer {TrainerID}.", ex);
                        throw new ApplicationException($"Database Error Archiving the Trainer {TrainerID}.", ex);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error Archiving the Trainer {TrainerID}.", ex);
                        throw new ApplicationException($"Error Archiving the Trainer {TrainerID}.", ex);
                    }
                }
            }
        }

        public static async Task<bool> IsTrainerExists(int PersonID)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_IsTrainerExists", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                    try
                    {
                        await connection.OpenAsync();

                        // ExecuteScalar returns the first column of the first row
                        object result = await command.ExecuteScalarAsync();

                        if (result != null && bool.TryParse(result.ToString(), out bool exists))
                        {
                            return exists;
                        }

                        return false;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error checking if Trainer exists for PersonID {PersonID}", ex);
                        return false; // Default to false on error, or throw exception based on your policy
                    }
                }
            }
        }
    }
}