using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer.Helpers;
using TrainningCenter_Entities;

namespace TrainningCenter_DataAccessLayer
{
    public static class clsAttendance
    {
        public static async Task<AttendanceResultDTO> GetAllAttendanceBySessionID(int SessionID, bool IncludeCancelledSessions = false)
        {
            if (SessionID <= 0)
                throw new ArgumentException("Session ID must be greater than 0.", nameof(SessionID));

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAttendanceBySessionID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@SessionID", SqlDbType.Int).Value = SessionID;
                    command.Parameters.Add("@IncludeCancelledSessions", SqlDbType.Bit).Value = IncludeCancelledSessions;

                    try
                    {
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            AttendanceResultDTO dto = new AttendanceResultDTO
                            {
                                Attendances = new List<AttendanceDetailDTO>()
                            };

                            int attendanceIDOrdinal = reader.GetOrdinal("AttendanceID");
                            int studentIDOrdinal = reader.GetOrdinal("StudentID");
                            int sessionIDOrdinal = reader.GetOrdinal("SessionID");
                            int attendanceStatusOrdinal = reader.GetOrdinal("AttendanceStatus");

                            int arrivalTimeOrdinal = reader.GetOrdinal("ArrivalTime");

                            int notesOrdinal = reader.GetOrdinal("Notes");
                            int recordedByOrdinal = reader.GetOrdinal("RecordedBy");
                            int recordedAtOrdinal = reader.GetOrdinal("RecordedAt");

                            int studentNumberOrdinal = reader.GetOrdinal("StudentNumber");
                            int studentFirstNameOrdinal = reader.GetOrdinal("FirstName");
                            int studentLastNameOrdinal = reader.GetOrdinal("LastName");
                            int studentEmailOrdinal = reader.GetOrdinal("Email");
                            int studentPhoneOrdinal = reader.GetOrdinal("PhoneNumber");

                            int sessionDateOrdinal = reader.GetOrdinal("SessionDate");
                            int sessionStartTimeOrdinal = reader.GetOrdinal("StartTime");
                            int sessionEndTimeOrdinal = reader.GetOrdinal("EndTime");
                            int sessionTopicOrdinal = reader.GetOrdinal("SessionTopic");

                            while (await reader.ReadAsync())
                            {
                                dto.Attendances.Add(new AttendanceDetailDTO
                                {
                                    AttendanceID = reader.GetInt32(attendanceIDOrdinal),
                                    StudentID = reader.GetInt32(studentIDOrdinal),
                                    SessionID = reader.GetInt32(sessionIDOrdinal),
                                    AttendanceStatus = reader.GetString(attendanceStatusOrdinal),
                                    ArrivalTime = reader.IsDBNull(arrivalTimeOrdinal) ? null : (TimeSpan?)reader.GetTimeSpan(arrivalTimeOrdinal),
                                    Notes = reader.IsDBNull(notesOrdinal) ? null : reader.GetString(notesOrdinal),
                                    RecordedBy = reader.IsDBNull(recordedByOrdinal) ? null : (int?)reader.GetInt32(recordedByOrdinal),
                                    RecordedAt = reader.IsDBNull(recordedAtOrdinal) ? null : (DateTime?)reader.GetDateTime(recordedAtOrdinal),
                                    StudentNumber = reader.GetString(studentNumberOrdinal),
                                    StudentFirstName = reader.GetString(studentFirstNameOrdinal),
                                    StudentLastName = reader.GetString(studentLastNameOrdinal),
                                    StudentEmail = reader.GetString(studentEmailOrdinal),
                                    StudentPhone = reader.GetString(studentPhoneOrdinal),
                                    SessionDate = reader.IsDBNull(sessionDateOrdinal) ? null : (DateTime?)reader.GetDateTime(sessionDateOrdinal),
                                    SessionStartTime = reader.IsDBNull(sessionStartTimeOrdinal) ? null : (TimeSpan?)reader.GetTimeSpan(sessionStartTimeOrdinal),
                                    SessionEndTime = reader.IsDBNull(sessionEndTimeOrdinal) ? null : (TimeSpan?)reader.GetTimeSpan(sessionEndTimeOrdinal),
                                    SessionTopic = reader.GetString(sessionTopicOrdinal)
                                });
                            }

                            if (await reader.NextResultAsync() && await reader.ReadAsync())
                            {
                                int totalRecordsOrdinal = reader.GetOrdinal("TotalAttendanceRecords");
                                int totalStudentsOrdinal = reader.GetOrdinal("TotalStudents");
                                int totalPresentOrdinal = reader.GetOrdinal("PresentCount");
                                int totalAbsentOrdinal = reader.GetOrdinal("AbsentCount");
                                int totalLateOrdinal = reader.GetOrdinal("LateCount");
                                int totalExcusedOrdinal = reader.GetOrdinal("ExcusedCount");
                                int attendancePercentageOrdinal = reader.GetOrdinal("AttendancePercentage");

                                dto.TotalAttendanceRecords = reader.IsDBNull(totalRecordsOrdinal) ? 0 : reader.GetInt32(totalRecordsOrdinal);
                                dto.TotalStudents = reader.IsDBNull(totalStudentsOrdinal) ? 0 : reader.GetInt32(totalStudentsOrdinal);
                                dto.PresentCount = reader.IsDBNull(totalPresentOrdinal) ? 0 : reader.GetInt32(totalPresentOrdinal);
                                dto.AbsentCount = reader.IsDBNull(totalAbsentOrdinal) ? 0 : reader.GetInt32(totalAbsentOrdinal);
                                dto.LateCount = reader.IsDBNull(totalLateOrdinal) ? 0 : reader.GetInt32(totalLateOrdinal);
                                dto.ExcusedCount = reader.IsDBNull(totalExcusedOrdinal) ? 0 : reader.GetInt32(totalExcusedOrdinal);
                                dto.AttendancePercentage = reader.IsDBNull(attendancePercentageOrdinal) ? 0 : reader.GetDouble(attendancePercentageOrdinal);
                            }
                            return dto;
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("An error occurred while retrieving attendance records by SessionID.", ex);
                        throw new ApplicationException("An error occurred while retrieving attendance records by SessionID.", ex);
                    }
                }
            }
        }
        public static async Task<StudentAttendanceResultDTO> GetAllAttendanceByStudentID(int StudentID, bool IncludeInactiveStudents = false, int TimeRange = 365)
        {
            if (StudentID <= 0)
                throw new ArgumentException("Student ID must be greater than 0.", nameof(StudentID));

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAttendanceByStudentID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@StudentID", SqlDbType.Int).Value = StudentID;
                    command.Parameters.Add("@IncludeInactiveStudents", SqlDbType.Bit).Value = IncludeInactiveStudents;
                    command.Parameters.Add("@TimeRange", SqlDbType.Int).Value = TimeRange;

                    try
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            StudentAttendanceResultDTO dto = new StudentAttendanceResultDTO
                            {
                                Attendances = new List<AttendanceDetailDTO>(),
                                StudentID = StudentID
                            };

                            while (await reader.ReadAsync())
                            {
                                dto.Attendances.Add(new AttendanceDetailDTO
                                {
                                    AttendanceID = reader.GetInt32(reader.GetOrdinal("AttendanceID")),
                                    SessionID = reader.GetInt32(reader.GetOrdinal("SessionID")),
                                    StudentID = StudentID,
                                    AttendanceStatus = reader.GetString(reader.GetOrdinal("AttendanceStatus")),
                                    ArrivalTime = reader.IsDBNull(reader.GetOrdinal("ArrivalTime"))
                                        ? null
                                        : (TimeSpan?)reader.GetTimeSpan(reader.GetOrdinal("ArrivalTime")),
                                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                                    RecordedAt = reader.IsDBNull(reader.GetOrdinal("RecordedAt")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("RecordedAt")),
                                    RecordedBy = reader.IsDBNull(reader.GetOrdinal("RecordedBy")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("RecordedBy")),
                                    SessionDate = reader.GetDateTime(reader.GetOrdinal("SessionDate")),
                                    SessionStartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                                    SessionEndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime")),
                                    SessionTopic = reader.GetString(reader.GetOrdinal("Topic")),
                                    SessionType = reader.GetString(reader.GetOrdinal("SessionType")),
                                    SessionLocation = reader.GetString(reader.GetOrdinal("Location")),
                                    SessionStatus = reader.GetString(reader.GetOrdinal("SessionStatus")),
                                    TrainerID = reader.IsDBNull(reader.GetOrdinal("TrainerID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("TrainerID")),
                                    TrainerName = reader.GetString(reader.GetOrdinal("TrainerName")),
                                    GroupName = reader.GetString(reader.GetOrdinal("GroupName")),
                                    SpecializationID = reader.IsDBNull(reader.GetOrdinal("SpecializationID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("SpecializationID")),
                                    LateMinutes = reader.IsDBNull(reader.GetOrdinal("LateMinutes")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("LateMinutes"))
                                });
                            }

                            // ✅ Result Set الثاني: Stats
                            if (await reader.NextResultAsync() && await reader.ReadAsync())
                            {
                                dto.TotalSessionsAttended = reader.GetInt32(reader.GetOrdinal("TotalSessionsAttended"));
                                dto.PresentCount = reader.GetInt32(reader.GetOrdinal("PresentCount"));
                                dto.AbsentCount = reader.GetInt32(reader.GetOrdinal("AbsentCount"));
                                dto.LateCount = reader.GetInt32(reader.GetOrdinal("LateCount"));
                                dto.ExcusedCount = reader.GetInt32(reader.GetOrdinal("ExcusedCount"));
                                dto.AttendancePercentage = reader.GetDouble(reader.GetOrdinal("AttendancePercentage"));
                                dto.AverageLateMinutes = reader.IsDBNull(reader.GetOrdinal("AverageLateMinutes"))
                                                             ? null
                                                             : (double?)reader.GetDouble(reader.GetOrdinal("AverageLateMinutes"));
                            }

                            return dto;
                        }
                    }
                    catch (SqlException ex)
                    {
                        EventLogger.LogError("A SQL error occurred while retrieving attendance records by StudentID.", ex);
                        throw new ApplicationException("A SQL error occurred while retrieving attendance records by StudentID.", ex);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("An Error occurred while retrieving attendance records by StudentID.", ex);
                        throw new ApplicationException("An Error occurred while retrieving attendance records by StudentID.", ex);
                    }
                }
            }
        }
        public static async Task<AttendanceDetailDTO> GetAttendanceBySessionAndStudent(int StudentID, int SessionID)
        {
            if (StudentID <= 0)
                throw new ArgumentException("Invalid StudentID", nameof(StudentID));

            if (SessionID <= 0)
                throw new ArgumentException("Invalid SessionID", nameof(SessionID));

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAttendanceBySessionAndStudent", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@SessionID", SqlDbType.Int).Value = SessionID;
                    command.Parameters.Add("@StudentID", SqlDbType.Int).Value = StudentID;

                    try
                    {
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {

                            if (await reader.ReadAsync())
                            {
                                int attendanceIDOrdinal = reader.GetOrdinal("AttendanceID");
                                int studentIDOrdinal = reader.GetOrdinal("StudentID");
                                int sessionIDOrdinal = reader.GetOrdinal("SessionID");
                                int attendanceStatusOrdinal = reader.GetOrdinal("AttendanceStatus");

                                int arrivalTimeOrdinal = reader.GetOrdinal("ArrivalTime");

                                int notesOrdinal = reader.GetOrdinal("Notes");
                                int recordedAtOrdinal = reader.GetOrdinal("RecordedAt");
                                int recordedByOrdinal = reader.GetOrdinal("RecordedBy");

                                int sessionDateOrdinal = reader.GetOrdinal("SessionDate");
                                int sessionStartTimeOrdinal = reader.GetOrdinal("StartTime");
                                int sessionEndTimeOrdinal = reader.GetOrdinal("EndTime");
                                int sessionTopicOrdinal = reader.GetOrdinal("Topic");
                                int sessionTypeOrdinal = reader.GetOrdinal("SessionType");
                                int locationOrdinal = reader.GetOrdinal("Location");
                                int sessionStatusOrdinal = reader.GetOrdinal("SessionStatus");

                                int trainerIDOrdinal = reader.GetOrdinal("TrainerID");
                                int trainerNameOrdinal = reader.GetOrdinal("TrainerName");
                                int groupNameOrdinal = reader.GetOrdinal("GroupName");
                                int specializationIDOrdinal = reader.GetOrdinal("SpecializationID");

                                int lateMinutesOrdinal = reader.GetOrdinal("LateMinutes");

                                int attendanceExistsOrdinal = reader.GetOrdinal("AttendanceExists");
                                bool attendanceExists = reader.GetInt32(attendanceExistsOrdinal) == 1;

                                if (!attendanceExists)
                                {
                                    return new AttendanceDetailDTO
                                    {
                                        AttendanceExists = false,

                                        StudentID = StudentID,
                                        SessionID = SessionID,

                                        SessionDate = reader.GetDateTime(sessionDateOrdinal),
                                        SessionStartTime = reader.GetTimeSpan(sessionStartTimeOrdinal),
                                        SessionEndTime = reader.GetTimeSpan(sessionEndTimeOrdinal),
                                        SessionTopic = reader.GetString(sessionTopicOrdinal),
                                        SessionType = reader.GetString(sessionTypeOrdinal),
                                        SessionLocation = reader.GetString(locationOrdinal),
                                        SessionStatus = reader.GetString(sessionStatusOrdinal),

                                        TrainerID = reader.IsDBNull(trainerIDOrdinal) ? null : (int?)reader.GetInt32(trainerIDOrdinal),
                                        TrainerName = reader.GetString(trainerNameOrdinal),
                                        GroupName = reader.GetString(groupNameOrdinal),
                                        SpecializationID = reader.IsDBNull(specializationIDOrdinal) ? null : (int?)reader.GetInt32(specializationIDOrdinal),

                                        AttendanceStatus = "NotRecorded",
                                        ArrivalTime = null,
                                        LateMinutes = null
                                    };
                                }

                                return new AttendanceDetailDTO
                                {
                                    AttendanceExists = true,

                                    AttendanceID = reader.GetInt32(attendanceIDOrdinal),
                                    StudentID = reader.GetInt32(studentIDOrdinal),
                                    SessionID = reader.GetInt32(sessionIDOrdinal),
                                    AttendanceStatus = reader.GetString(attendanceStatusOrdinal),

                                    ArrivalTime = reader.IsDBNull(arrivalTimeOrdinal) ? null : (TimeSpan?)reader.GetTimeSpan(arrivalTimeOrdinal),
                                    Notes = reader.IsDBNull(notesOrdinal) ? null : reader.GetString(notesOrdinal),
                                    RecordedAt = reader.IsDBNull(recordedAtOrdinal) ? null : (DateTime?)reader.GetDateTime(recordedAtOrdinal),
                                    RecordedBy = reader.IsDBNull(recordedByOrdinal) ? null : (int?)reader.GetInt32(recordedByOrdinal),
                                    LateMinutes = reader.IsDBNull(lateMinutesOrdinal) ? null : (int?)reader.GetInt32(lateMinutesOrdinal),

                                    SessionDate = reader.GetDateTime(sessionDateOrdinal),
                                    SessionStartTime = reader.GetTimeSpan(sessionStartTimeOrdinal),
                                    SessionEndTime = reader.GetTimeSpan(sessionEndTimeOrdinal),
                                    SessionTopic = reader.GetString(sessionTopicOrdinal),
                                    SessionType = reader.GetString(sessionTypeOrdinal),
                                    SessionLocation = reader.GetString(locationOrdinal),
                                    SessionStatus = reader.GetString(sessionStatusOrdinal),

                                    TrainerID = reader.IsDBNull(trainerIDOrdinal) ? null : (int?)reader.GetInt32(trainerIDOrdinal),
                                    TrainerName = reader.GetString(trainerNameOrdinal),
                                    GroupName = reader.GetString(groupNameOrdinal),
                                    SpecializationID = reader.IsDBNull(specializationIDOrdinal) ? null : (int?)reader.GetInt32(specializationIDOrdinal)
                                };
                            }

                            return null;
                        }
                    }
                    catch (SqlException ex)
                    {
                        EventLogger.LogError("A SQL error occurred while retrieving attendance record by StudentID and SessionID.", ex);
                        throw new ApplicationException("A SQL error occurred while retrieving attendance record by StudentID and SessionID.", ex);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("An Error occurred while retrieving attendance record by StudentID and SessionID.", ex);
                        throw new ApplicationException("An Error occurred while retrieving attendance record by StudentID and SessionID.", ex);
                    }
                }
            }
        }

        public static async Task<SessionAttendanceFullResultDTO>
            GetAttendanceBySessionWithStudents(int sessionID)
        {
            var result = new SessionAttendanceFullResultDTO
            {
                Students = new List<SessionAttendanceStudentDTO>(),
                IsSuccess = false
            };

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAttendanceBySessionWithStudents", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@SessionID", sessionID);

                    try
                    {
                        await connection.OpenAsync();

                        using(SqlDataReader reader = await command.ExecuteReaderAsync())
                        { 
                            while(await reader.ReadAsync())
                            {
                                var student = new SessionAttendanceStudentDTO
                                {
                                    StudentID = reader.GetInt32(reader.GetOrdinal("StudentID")),
                                    StudentNumber = reader.GetString(reader.GetOrdinal("StudentNumber")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName =  reader.GetString(reader.GetOrdinal("LastName")),
                                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                                    Email = reader.GetString(reader.GetOrdinal("Email")),
                                    Phone = reader.GetString(reader.GetOrdinal("PhoneNumber")),

                                    AttendanceStatus = reader.GetString(reader.GetOrdinal("AttendanceStatus")),
                                    HasAttendanceRecord = reader.GetBoolean(reader.GetOrdinal("HasAttendanceRecord")),

                                    SessionID = reader.GetInt32(reader.GetOrdinal("SessionID")),
                                    SessionDate = reader.GetDateTime(reader.GetOrdinal("SessionDate")),
                                    Topic = reader.GetString(reader.GetOrdinal("Topic")),
                                    SubscriptionStatus = reader.GetString(reader.GetOrdinal("SubscriptionStatus")),
                                };

                                student.AttendanceID = reader.IsDBNull(reader.GetOrdinal("AttendanceID"))
                                    ? (int?)null
                                    : reader.GetInt32(reader.GetOrdinal("AttendanceID"));

                                student.ArrivalTime = reader.IsDBNull(reader.GetOrdinal("ArrivalTime"))
                                    ? (TimeSpan?)null
                                    : reader.GetTimeSpan(reader.GetOrdinal("ArrivalTime"));

                                student.StartTime = reader.IsDBNull(reader.GetOrdinal("StartTime"))
                                    ? (TimeSpan?)null
                                    : reader.GetTimeSpan(reader.GetOrdinal("StartTime"));

                                student.EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime"))
                                    ? (TimeSpan?)null
                                    : reader.GetTimeSpan(reader.GetOrdinal("EndTime"));

                                student.SubscriptionID = reader.IsDBNull(reader.GetOrdinal("SubscriptionID"))
                                    ? (int?)null
                                    : reader.GetInt32(reader.GetOrdinal("SubscriptionID"));

                                student.DateOfSubscription = reader.IsDBNull(reader.GetOrdinal("DateOfSubscription"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime(reader.GetOrdinal("DateOfSubscription"));

                                student.DaysSinceSubscription = reader.IsDBNull(reader.GetOrdinal("DaysSinceSubscription"))
                                    ? (int?)null
                                    : reader.GetInt32(reader.GetOrdinal("DaysSinceSubscription"));

                                result.Students.Add(student);
                            }

                            // Read the next result set
                            if(await reader.NextResultAsync() && await reader.ReadAsync())
                            {
                                result.AttendanceStatistics = new SessionAttendanceStatisticsDTO
                                {
                                    TotalStudents = reader.GetInt32(reader.GetOrdinal("TotalStudents")),
                                    PresentCount = reader.GetInt32(reader.GetOrdinal("PresentCount")),
                                    AbsentCount = reader.GetInt32(reader.GetOrdinal("AbsentCount")),
                                    LateCount = reader.GetInt32(reader.GetOrdinal("LateCount")),
                                    ExcusedCount = reader.GetInt32(reader.GetOrdinal("ExcusedCount")),
                                    NotMarkedCount = reader.GetInt32(reader.GetOrdinal("NotMarkedCount")),
                                    PresentPercentage = (float)reader.GetDouble(reader.GetOrdinal("PresentPercentage")),
                                    AbsentPercentage = (float)reader.GetDouble(reader.GetOrdinal("AbsentPercentage")),
                                    SessionID = reader.GetInt32(reader.GetOrdinal("SessionID")),
                                    SessionDate = reader.GetDateTime(reader.GetOrdinal("SessionDate")),
                                    SessionStatus = reader.GetString(reader.GetOrdinal("SessionStatus"))
                                };

                                if(result.AttendanceStatistics.TotalStudents > 0)
                                {
                                    result.AttendanceStatistics.LatePercentage = 
                                        (float)result.AttendanceStatistics.LateCount / result.AttendanceStatistics.TotalStudents * 100;

                                    result.AttendanceStatistics.ExcusedPercentage =
                                        (float)result.AttendanceStatistics.ExcusedCount / result.AttendanceStatistics.TotalStudents * 100;

                                    result.AttendanceStatistics.NotMarkedPercentage =
                                        (float)result.AttendanceStatistics.NotMarkedCount / result.AttendanceStatistics.TotalStudents * 100;
                                }

                                result.IsSuccess = true;
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        result.ErrorMessage = $"Database error: {sqlEx.Message}";
                        EventLogger.LogError("Error in GetAttendanceBySessionWithStudents", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        result.ErrorMessage = $"Error: {ex.Message}";
                        EventLogger.LogError("Error in GetAttendanceBySessionWithStudents", ex);
                    }
                }
            }

            return result;
        }
        public static async Task<AttendanceOperationResultDTO> AddAttendance(CreateUpdateAttendanceDTO AttendaceParams)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_AddAttendance", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@StudentID", SqlDbType.Int).Value = AttendaceParams.StudentID;
                    command.Parameters.Add("@SessionID", SqlDbType.Int).Value = AttendaceParams.SessionID;
                    command.Parameters.Add("@AttendanceStatus", SqlDbType.NVarChar, 20).Value = AttendaceParams.AttendanceStatus;
                    command.Parameters.Add("@ArrivalTime", SqlDbType.Time).Value = (object)AttendaceParams.ArrivalTime ?? DBNull.Value;
                    command.Parameters.Add("@Notes", SqlDbType.NVarChar, -1).Value = (object)AttendaceParams.Notes ?? DBNull.Value;
                    command.Parameters.Add("@RecordedBy", SqlDbType.Int).Value = AttendaceParams.RecordedBy;
                    SqlParameter outputIdParam = new SqlParameter("@NewAttendanceID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputIdParam);
                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        int newAttendanceID = (int)outputIdParam.Value;
                        return new AttendanceOperationResultDTO
                        {
                            Success = true,
                            AttendanceID = newAttendanceID,
                            Message = "Attendance record added successfully."
                        };
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("An error occurred while adding a new attendance record.", ex);
                        return new AttendanceOperationResultDTO
                        {
                            Success = false,
                            AttendanceID = null,
                            Message = ex.Message
                        };
                    }
                }
            }
        }
        public static async Task<AttendanceOperationResultDTO> UpdateAttendance(CreateUpdateAttendanceDTO AttendaceParams)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateAttendance", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@AttendanceID", SqlDbType.Int).Value = AttendaceParams.AttendanceID;
                    command.Parameters.Add("@AttendanceStatus", SqlDbType.NVarChar, 50).Value = AttendaceParams.AttendanceStatus;
                    command.Parameters.Add("@ArrivalTime", SqlDbType.Time).Value = (object)AttendaceParams.ArrivalTime ?? DBNull.Value;
                    command.Parameters.Add("@Notes", SqlDbType.NVarChar, -1).Value = (object)AttendaceParams.Notes ?? DBNull.Value;
                    command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = AttendaceParams.RecordedBy;

                    try
                    {
                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new AttendanceOperationResultDTO
                            {
                                Success = true,
                                AttendanceID = AttendaceParams.AttendanceID,
                                Message = "Attendance record updated successfully."
                            };
                        }
                        else
                        {
                            return new AttendanceOperationResultDTO
                            {
                                Success = false,
                                AttendanceID = null,
                                Message = "No attendance record was updated."
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("An error occurred while updating the attendance record.", ex);
                        return new AttendanceOperationResultDTO
                        {
                            Success = false,
                            AttendanceID = null,
                            Message = ex.Message
                        };
                    }
                }
            }
        }
        public static async Task<AttendanceUpsertResultDTO> UpsertAttendance(CreateUpdateAttendanceDTO dto)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_UpsertAttendance", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@SessionID", SqlDbType.Int).Value = dto.SessionID;
                    command.Parameters.Add("@StudentID", SqlDbType.Int).Value = dto.StudentID;
                    command.Parameters.Add("@AttendanceStatus", SqlDbType.NVarChar, 20).Value = dto.AttendanceStatus;
                    command.Parameters.Add("@ArrivalTime", SqlDbType.Time).Value = (object)dto.ArrivalTime ?? DBNull.Value;
                    command.Parameters.Add("@Notes", SqlDbType.NVarChar, 500).Value = (object)dto.Notes ?? DBNull.Value;
                    command.Parameters.Add("@RecordedBy", SqlDbType.Int).Value = dto.RecordedBy;

                    try
                    {
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {

                            if (await reader.ReadAsync())
                            {
                                return new AttendanceUpsertResultDTO
                                {
                                    Success = true,
                                    AttendanceID = reader.GetInt32(reader.GetOrdinal("AttendanceID")),
                                    Action = reader.GetString(reader.GetOrdinal("Action"))
                                };
                            }
                        }

                        return new AttendanceUpsertResultDTO
                        {
                            Success = false,
                            ErrorMessage = "No result returned from SP_UpsertAttendance."
                        };
                    }
                    catch (SqlException ex)
                    {
                        EventLogger.LogError("UpsertAttendance SQL error.", ex);
                        return new AttendanceUpsertResultDTO
                        {
                            Success = false,
                            ErrorMessage = ex.Message
                        };
                    }
                }
            }
        }
        public static async Task<AttendanceDeleteResultDTO> DeleteAttendance(int attendanceID)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_DeleteAttendance", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@AttendanceID", SqlDbType.Int).Value = attendanceID;

                    try
                    {
                        await connection.OpenAsync();

                        object result = await command.ExecuteScalarAsync();

                        return new AttendanceDeleteResultDTO
                        {
                            Success = Convert.ToInt32(result) == 1
                        };
                    }
                    catch (SqlException ex)
                    {
                        EventLogger.LogError("DeleteAttendance SQL error.", ex);
                        return new AttendanceDeleteResultDTO
                        {
                            Success = false,
                            ErrorMessage = ex.Message
                        };
                    }
                }
                ;
            }
        }
        public static async Task<AttendanceStatisticsDTO> GetAttendanceStatistics(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? groupID = null)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAttendanceStatistics", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@StartDate", SqlDbType.Date).Value = (object)startDate ?? DBNull.Value;
                    command.Parameters.Add("@EndDate", SqlDbType.Date).Value = (object)endDate ?? DBNull.Value;
                    command.Parameters.Add("@GroupID", SqlDbType.Int).Value = (object)groupID ?? DBNull.Value;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {

                        if (!await reader.ReadAsync())
                            return null;

                        return new AttendanceStatisticsDTO
                        {
                            TotalSessions = reader.GetInt32(0),
                            TodaySessions = reader.GetInt32(1),
                            CompletedSessions = reader.GetInt32(2),
                            UpcomingSessions = reader.GetInt32(3),

                            TotalAttendanceRecords = reader.GetInt32(4),
                            TotalPresent = reader.GetInt32(5),
                            TotalAbsent = reader.GetInt32(6),
                            TotalLate = reader.GetInt32(7),
                            TotalExcused = reader.GetInt32(8),

                            AttendanceRate = reader.GetDouble(9)
                        };
                    }
                }
            }
        }
        public static async Task<StudentAttendanceRateDTO> GetStudentAttendanceRate(int studentID)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {

                using (SqlCommand command = new SqlCommand("SP_GetStudentAttendanceRate", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@StudentID", SqlDbType.Int).Value = studentID;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {


                        if (!await reader.ReadAsync())
                            return null;

                        return new StudentAttendanceRateDTO
                        {
                            StudentID = reader.GetInt32(0),
                            TotalSessionsRecorded = reader.GetInt32(1),
                            PresentCount = reader.GetInt32(2),
                            LateCount = reader.GetInt32(3),
                            AbsentCount = reader.GetInt32(4),
                            ExcusedCount = reader.GetInt32(5),
                            AttendanceRate = reader.GetDouble(6)
                        };
                    }
                }
            }
        }
    }
}
