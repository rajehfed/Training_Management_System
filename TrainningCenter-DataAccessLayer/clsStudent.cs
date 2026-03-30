using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer.Helpers;
using TrainningCenter_Entities;

namespace TrainningCenter_DataAccessLayer
{
    /// <summary>
    /// Handles all database operations related to Students.
    /// Uses asynchronous execution for high performance and responsiveness.
    /// </summary>
    public static class clsStudent
    {
        /// <summary>
        /// Retrieves a comprehensive list of all students from the database.
        /// </summary>
        /// <returns>
        /// A List of <see cref="StudentDTO"/> objects containing student details. 
        /// Returns an empty list if no students are found.
        /// </returns>
        /// <exception cref="ApplicationException">Thrown when a database connection or execution error occurs.</exception>
        public static async Task<List<StudentDTO>> GetAllStudents()
        {
            List<StudentDTO> dtos = new List<StudentDTO>();
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllStudents", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            // Look up column ordinals once outside the loop for performance
                            int studentIDIndex = reader.GetOrdinal("StudentID");
                            int personIDIndex = reader.GetOrdinal("PersonID");
                            int studentNameIndex = reader.GetOrdinal("StudentName");
                            int studentNoIndex = reader.GetOrdinal("StudentNumber");
                            int admissionDateIndex = reader.GetOrdinal("AdmissionDate");
                            int sudentStatusIndex = reader.GetOrdinal("StudentStatus");
                            int emergencyContactIndex = reader.GetOrdinal("EmergencyContact");
                            int createdAtIndex = reader.GetOrdinal("CreatedAt");
                            int updatedAtIndex = reader.GetOrdinal("UpdatedAt");
                            int isActiveIndex = reader.GetOrdinal("IsActive");

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
                                // Use safe checks for nullable columns (EmergencyContact and UpdatedAt)
                                DateTime? lastUpdatedAt = reader.IsDBNull(updatedAtIndex)
                                                            ? (DateTime?)null
                                                            : reader.GetDateTime(updatedAtIndex);

                                DateTime? personLastUpdatedAt = reader.IsDBNull(personUpdatedAtIndex)
                                                                ? (DateTime?)null
                                                                : reader.GetDateTime(personUpdatedAtIndex);

                                string emergencyContact = reader.IsDBNull(emergencyContactIndex)
                                                       ? null
                                                       : reader.GetString(emergencyContactIndex);

                                dtos.Add(new StudentDTO
                                {
                                    StudentID = reader.GetInt32(studentIDIndex),
                                    PersonID = reader.GetInt32(personIDIndex),
                                    StudentName = reader.GetString(studentNameIndex),
                                    StudentNo = reader.GetString(studentNoIndex),
                                    AdmissionDate = reader.GetDateTime(admissionDateIndex),
                                    StudentStatus = reader.GetString(sudentStatusIndex),
                                    EmergencyContact = emergencyContact,
                                    CreatedAt = reader.GetDateTime(createdAtIndex),
                                    LastUpdatedAt = lastUpdatedAt,
                                    IsActive = reader.GetBoolean(isActiveIndex),
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
                        }

                        return dtos;
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError("Database Error While try to Retieving the Students List", sqlEx);
                        throw new ApplicationException("Database Error While try to Retieving the Students List", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("An Error While try to Retieving the Students List", ex);
                        throw new ApplicationException("An Error While try to Retieving the Students List", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Retrieves a specific student by their unique identifier.
        /// </summary>
        /// <param name="studentID">The unique ID of the student to retrieve.</param>
        /// <returns>A populated <see cref="StudentDTO"/> if found, otherwise an empty/default DTO.</returns>
        public static async Task<StudentDTO> GetStudentByID(int studentID)
        {
            StudentDTO dto = new StudentDTO();
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetStudentByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@StudentID", SqlDbType.Int).Value = studentID;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            // Look up column ordinals once outside the loop for performance
                            int studentIDIndex = reader.GetOrdinal("StudentID");
                            int personIDIndex = reader.GetOrdinal("PersonID");
                            int studentNameIndex = reader.GetOrdinal("StudentName");
                            int studentNoIndex = reader.GetOrdinal("StudentNumber");
                            int admissionDateIndex = reader.GetOrdinal("AdmissionDate");
                            int sudentStatusIndex = reader.GetOrdinal("StudentStatus");
                            int emergencyContactIndex = reader.GetOrdinal("EmergencyContact");
                            int createdAtIndex = reader.GetOrdinal("CreatedAt");
                            int updatedAtIndex = reader.GetOrdinal("UpdatedAt");
                            int isActiveIndex = reader.GetOrdinal("IsActive");

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
                                // Use safe checks for nullable columns (EmergencyContact and UpdatedAt)
                                DateTime? lastUpdatedAt = reader.IsDBNull(updatedAtIndex)
                                                            ? (DateTime?)null
                                                            : reader.GetDateTime(updatedAtIndex);

                                string emergencyContact = reader.IsDBNull(emergencyContactIndex)
                                                       ? null
                                                       : reader.GetString(emergencyContactIndex);

                                dto = new StudentDTO
                                {
                                    StudentID = reader.GetInt32(studentIDIndex),
                                    PersonID = reader.GetInt32(personIDIndex),
                                    StudentName = reader.GetString(studentNameIndex),
                                    StudentNo = reader.GetString(studentNoIndex),
                                    AdmissionDate = reader.GetDateTime(admissionDateIndex),
                                    StudentStatus = reader.GetString(sudentStatusIndex),
                                    EmergencyContact = emergencyContact,        // Handle null/DBNull
                                    CreatedAt = reader.GetDateTime(createdAtIndex),
                                    LastUpdatedAt = lastUpdatedAt,        // Handle null/DBNull
                                    IsActive = reader.GetBoolean(isActiveIndex),
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
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database Error While try to Retieving the Student with ID: {studentID}", sqlEx);
                        throw new ApplicationException($"Database Error While try to Retieving the Student with ID: {studentID}", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"An Error While try to Retieving the Student with ID {studentID}", ex);
                        throw new ApplicationException($"An Error While try to Retieving the Student with ID {studentID}", ex);
                    }
                }
            }

            return dto;
        }

        /// <summary>
        /// Inserts a new student record into the database.
        /// </summary>
        /// <param name="PersonID">The ID of the existing Person record.</param>
        /// <param name="StudentNo">The unique student registration number.</param>
        /// <param name="AdmissionDate">The date the student joined.</param>
        /// <param name="StudentStatus">Current status (e.g., Active, Graduated).</param>
        /// <param name="EmergencyContact">Contact info for emergencies.</param>
        /// <returns>
        /// The new <c>StudentID</c> if successful; otherwise, <c>null</c>.
        /// </returns>
        public static async Task<int?> AddNewStudent(
            int PersonID,
            string StudentNo,
            DateTime AdmissionDate,
            string StudentStatus,
            string EmergencyContact
        )
        {
            int? insertedID = null;

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_AddNewStudent", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                    command.Parameters.Add("@StudentNumber", SqlDbType.NVarChar, 500).Value = StudentNo;
                    command.Parameters.Add("@AdmissionDate", SqlDbType.Date).Value = AdmissionDate;
                    command.Parameters.Add("@StudentStatus", SqlDbType.NVarChar, 20).Value = StudentStatus;
                    command.Parameters.Add("@EmergencyContact", SqlDbType.NVarChar, 200).Value = EmergencyContact;

                    try
                    {
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal("StudentID")))
                                {
                                    insertedID = reader.GetInt32(reader.GetOrdinal("StudentID"));
                                }

                                string status = reader.GetString(reader.GetOrdinal("Status"));
                                string message = reader.GetString(reader.GetOrdinal("Message"));

                                if (status == "SUCCESS" && insertedID.HasValue)
                                {
                                    EventLogger.LogInfo($"Successfully added Student with ID: {insertedID}");
                                    return insertedID;
                                }
                                else
                                {
                                    // Now we can log the EXACT reason from SQL!
                                    EventLogger.LogWarning($"Failed to add Student. Reason: {message}");
                                    return null;
                                }
                            }

                            EventLogger.LogWarning($"Failed to add Student. No ID returned.");
                            return null;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError("Database error while adding Student", sqlEx);
                        throw new ApplicationException("Database error while adding Student", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("An error Occured while adding Student", ex);
                        throw new ApplicationException("An error Occured while adding Student", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Updates an existing student's details.
        /// </summary>
        /// <returns>True if the update was successful, False otherwise.</returns>
        public static async Task<bool> UpdateStudent(
            int StudentID,
            int PersonID,
            string StudentNo,
            DateTime AdmissionDate,
            string StudentStatus,
            string EmergencyContact,
            bool IsActive
        )
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateStudent", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@StudentID", SqlDbType.Int).Value = StudentID;
                    command.Parameters.Add("@PersonID", SqlDbType.Int).Value = PersonID;
                    command.Parameters.Add("@StudentNumber", SqlDbType.NVarChar, 500).Value = StudentNo;
                    command.Parameters.Add("@AdmissionDate", SqlDbType.Date).Value = AdmissionDate;
                    command.Parameters.Add("@StudentStatus", SqlDbType.NVarChar, 20).Value = StudentStatus;
                    command.Parameters.Add("@EmergencyContact", SqlDbType.NVarChar, 200).Value = EmergencyContact;
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
                                    EventLogger.LogInfo($"Successfully updated Student with ID: {StudentID}. {message}");
                                    return rowsAffected > 0;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"Update failed for Student ID {StudentID}: {message}");
                                    return false;
                                }
                            }
                            else
                            {
                                EventLogger.LogWarning($"No response from database while updating Student ID {StudentID}");
                                return false;
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database error while Updating Student ID: {StudentID}", sqlEx);
                        throw new ApplicationException($"Database error while Updating Student ID: {StudentID}", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"An error Occured while Updating Student ID: {StudentID}", ex);
                        throw new ApplicationException($"An error Occured while Updating Student ID: {StudentID}", ex);
                    }
                }
            }
        }
        /// <summary>
        /// Deletes a student record. Supports soft or hard delete based on SP logic.
        /// </summary>
        /// <param name="StudentID">The ID of the student to delete.</param>
        /// <param name="ForceDelete">If true, bypasses checks (if supported by SP).</param>
        /// <returns>True if deleted successfully.</returns>
        public static async Task<bool> DeleteStudent(int StudentID, bool ForceDelete = false)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_DeleteStudent", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@StudentID", SqlDbType.Int).Value = StudentID;
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
                                    EventLogger.LogInfo($"Successfully deleted Student with ID: {StudentID}. {message}");
                                    return rowsAffected > 0;
                                }
                                else
                                {
                                    EventLogger.LogWarning($"Deletion failed for Student ID {StudentID}: {message}");
                                    return false;
                                }
                            }
                            else
                            {
                                EventLogger.LogWarning($"No response from database while deleting Student ID {StudentID}");
                                return false;
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        EventLogger.LogError($"Database error while Deleting Student ID: {StudentID}", sqlEx);
                        throw new ApplicationException($"Database error while Deleting Student ID: {StudentID}", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"An error Occured while Deleting Student ID: {StudentID}", ex);
                        throw new ApplicationException($"An error Occured while Deleting Student ID: {StudentID}", ex);
                    }
                }
            }
        }
    }
}
