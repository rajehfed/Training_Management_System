using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using TrainningCenter_DataAccessLayer.Helpers;
using TrainningCenter_Entities;
using static System.Collections.Specialized.BitVector32;

namespace TrainningCenter_DataAccessLayer
{
    public static class clsSession
    {
        /// <summary>
        /// Get All Sessions
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public static async Task<SessionsInfoDTO> GetAll()
        {
            var dto = new SessionsInfoDTO
            {
                Sessions = new List<SessionDTO>()
            };

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetAllSessions", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            // قراءة جميع الصفوف
                            while (await reader.ReadAsync())
                            {
                                var session = new TrainningCenter_Entities.SessionDTO
                                {
                                    // البيانات الأساسية
                                    SessionID = reader.GetInt32(reader.GetOrdinal("SessionID")),
                                    GroupID = reader.GetInt32(reader.GetOrdinal("GroupID")),
                                    SessionDate = reader.GetDateTime(reader.GetOrdinal("SessionDate")),

                                    // CORRECTED: Reading SQL TIME column as TimeSpan
                                    StartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                                    EndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime")),

                                    SessionType = reader.GetString(reader.GetOrdinal("SessionType")),
                                    Location = reader.GetString(reader.GetOrdinal("Location")),
                                    Topic = reader.GetString(reader.GetOrdinal("Topic")),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    GroupName = reader.GetString(reader.GetOrdinal("GroupName")),
                                    SpecializationID = reader.GetInt32(reader.GetOrdinal("SpecializationID")),

                                    // بيانات المدرب (معالجة NULL)
                                    TrainerID = reader.IsDBNull(reader.GetOrdinal("TrainerID"))
                                    ? (int?)null
                                    : reader.GetInt32(reader.GetOrdinal("TrainerID")),

                                    TrainerFirstName = reader.IsDBNull(reader.GetOrdinal("TrainerFirstName"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("TrainerFirstName")),

                                    TrainerLastName = reader.IsDBNull(reader.GetOrdinal("TrainerLastName"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("TrainerLastName")),

                                    // الملاحظات (قد تكون NULL)
                                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("Notes")),

                                    // الإحصائيات لكل جلسة (handled by efficient SP now)
                                    TotalStudents = reader.IsDBNull(reader.GetOrdinal("TotalStudents"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("TotalStudents")),

                                    PresentCount = reader.IsDBNull(reader.GetOrdinal("PresentCount"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("PresentCount")),

                                    AbsentCount = reader.IsDBNull(reader.GetOrdinal("AbsentCount"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("AbsentCount")),

                                    LateCount = reader.IsDBNull(reader.GetOrdinal("LateCount"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("LateCount")),

                                    ExcusedCount = reader.IsDBNull(reader.GetOrdinal("ExcusedCount"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("ExcusedCount"))
                                };

                                dto.Sessions.Add(session);
                            }

                            // حساب الإجماليات من جميع الجلسات
                            if (dto.Sessions.Any())
                            {
                                dto.TotalStudents = dto.Sessions.Sum(s => s.TotalStudents);
                                dto.PresentCount = dto.Sessions.Sum(s => s.PresentCount);
                                dto.AbsentCount = dto.Sessions.Sum(s => s.AbsentCount);
                                dto.LateCount = dto.Sessions.Sum(s => s.LateCount);
                                dto.ExcusedCount = dto.Sessions.Sum(s => s.ExcusedCount);
                            }
                        }

                        return dto;
                    }
                    catch (SqlException ex)
                    {
                        EventLogger.LogError("Database Error While trying to retrieve The sessions data!", ex);
                        throw new ApplicationException("Database Error While trying to retrieve The sessions data!", ex);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError("Error While trying to retrieve The sessions data!", ex);
                        throw new ApplicationException("Error While trying to retrieve The sessions data!", ex);
                    }
                }
            }
        }

        /// <summary>
        /// استرجاع جلسة تدريبية بواسطة المعرف
        /// </summary>
        /// <param name="sessionID">معرف الجلسة (رقم موجب)</param>
        /// <returns>كائن SessionDTO يحتوي على بيانات الجلسة، أو null إذا لم توجد</returns>
        /// <exception cref="SqlException">خطأ في الاتصال بقاعدة البيانات</exception>
        public static async Task<SessionDTO> GetSessionByID(int SessionID)
        {
            SessionDTO dto = null;
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetSessionByID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@SessionID", SqlDbType.Int).Value = SessionID;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                dto = new SessionDTO
                                {
                                    // البيانات الأساسية
                                    SessionID = reader.GetInt32(reader.GetOrdinal("SessionID")),
                                    GroupID = reader.GetInt32(reader.GetOrdinal("GroupID")),
                                    SessionDate = reader.GetDateTime(reader.GetOrdinal("SessionDate")),

                                    // CORRECTED: Reading SQL TIME column as TimeSpan
                                    StartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                                    EndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime")),

                                    SessionType = reader.GetString(reader.GetOrdinal("SessionType")),
                                    Location = reader.GetString(reader.GetOrdinal("Location")),
                                    Topic = reader.GetString(reader.GetOrdinal("Topic")),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    GroupName = reader.GetString(reader.GetOrdinal("GroupName")),
                                    SpecializationID = reader.GetInt32(reader.GetOrdinal("SpecializationID")),

                                    // بيانات المدرب (معالجة NULL)
                                    TrainerID = reader.IsDBNull(reader.GetOrdinal("TrainerID"))
                                    ? (int?)null
                                    : reader.GetInt32(reader.GetOrdinal("TrainerID")),

                                    TrainerFirstName = reader.IsDBNull(reader.GetOrdinal("TrainerFirstName"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("TrainerFirstName")),

                                    TrainerLastName = reader.IsDBNull(reader.GetOrdinal("TrainerLastName"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("TrainerLastName")),

                                    // الملاحظات (قد تكون NULL)
                                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("Notes")),

                                    // الإحصائيات لكل جلسة (handled by efficient SP now)
                                    TotalStudents = reader.IsDBNull(reader.GetOrdinal("TotalStudents"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("TotalStudents")),

                                    PresentCount = reader.IsDBNull(reader.GetOrdinal("PresentCount"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("PresentCount")),

                                    AbsentCount = reader.IsDBNull(reader.GetOrdinal("AbsentCount"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("AbsentCount")),

                                    LateCount = reader.IsDBNull(reader.GetOrdinal("LateCount"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("LateCount")),

                                    ExcusedCount = reader.IsDBNull(reader.GetOrdinal("ExcusedCount"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("ExcusedCount"))
                                };
                            }

                            return dto;
                        }
                    }
                    catch (SqlException ex)
                    {
                        EventLogger.LogError($"Database Error While trying to retrieve The session with ID {SessionID} data!", ex);
                        throw new ApplicationException($"Database Error While trying to retrieve The session with ID {SessionID} data!", ex);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error While trying to retrieve The session With ID {SessionID} data!", ex);
                        throw new ApplicationException($"Error While trying to retrieve The session With ID {SessionID} data!", ex);
                    }
                }
            }
        }

        /// <summary>
        /// استرجاع جميع جلسات مجموعة معينة
        /// </summary>
        /// <param name="groupID">معرف المجموعة</param>
        /// <param name="activeOnly">true لعرض الجلسات النشطة فقط (الافتراضي)</param>
        /// <param name="includeStats">true لتضمين إحصائيات الحضور (الافتراضي)</param>
        /// <returns>قائمة من SessionDTO تحتوي على جلسات المجموعة</returns>
        public static async Task<List<SessionDTO>> GetSessionsByGroupID(int GroupID, bool ActiveOnly = true, bool IncludeStats = true)
        {
            var sessions = new List<SessionDTO>();

            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_GetSessionsByGroupID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@GroupID", SqlDbType.Int).Value = GroupID;
                    command.Parameters.Add("@ActiveOnly", SqlDbType.Bit).Value = ActiveOnly;
                    command.Parameters.Add("@IncludeStats", SqlDbType.Bit).Value = IncludeStats;

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var session = new SessionDTO
                                {
                                    // البيانات الأساسية
                                    SessionID = reader.GetInt32(reader.GetOrdinal("SessionID")),
                                    GroupID = reader.GetInt32(reader.GetOrdinal("GroupID")),
                                    SessionDate = reader.GetDateTime(reader.GetOrdinal("SessionDate")),

                                    // CORRECTED: Reading SQL TIME column as TimeSpan
                                    StartTime = reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                                    EndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime")),

                                    SessionType = reader.GetString(reader.GetOrdinal("SessionType")),
                                    Location = reader.GetString(reader.GetOrdinal("Location")),
                                    Topic = reader.GetString(reader.GetOrdinal("Topic")),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    GroupName = reader.GetString(reader.GetOrdinal("GroupName")),
                                    SpecializationID = reader.GetInt32(reader.GetOrdinal("SpecializationID")),

                                    // بيانات المدرب (معالجة NULL)
                                    TrainerID = reader.IsDBNull(reader.GetOrdinal("TrainerID"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("TrainerID")),

                                    TrainerFirstName = reader.IsDBNull(reader.GetOrdinal("TrainerFirstName"))
                                        ? string.Empty
                                        : reader.GetString(reader.GetOrdinal("TrainerFirstName")),

                                    TrainerLastName = reader.IsDBNull(reader.GetOrdinal("TrainerLastName"))
                                        ? string.Empty
                                        : reader.GetString(reader.GetOrdinal("TrainerLastName")),

                                    // الملاحظات (قد تكون NULL)
                                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes"))
                                        ? string.Empty
                                        : reader.GetString(reader.GetOrdinal("Notes")),


                                    // الإحصائيات لكل جلسة (handled by efficient SP now)
                                    TotalStudents = reader.IsDBNull(reader.GetOrdinal("TotalStudents"))
                                        ? 0
                                        : reader.GetInt32(reader.GetOrdinal("TotalStudents")),

                                    PresentCount = reader.IsDBNull(reader.GetOrdinal("PresentCount"))
                                        ? 0
                                        : reader.GetInt32(reader.GetOrdinal("PresentCount")),

                                    AbsentCount = reader.IsDBNull(reader.GetOrdinal("AbsentCount"))
                                        ? 0
                                        : reader.GetInt32(reader.GetOrdinal("AbsentCount")),

                                    LateCount = reader.IsDBNull(reader.GetOrdinal("LateCount"))
                                        ? 0
                                        : reader.GetInt32(reader.GetOrdinal("LateCount")),

                                    ExcusedCount = reader.IsDBNull(reader.GetOrdinal("ExcusedCount"))
                                        ? 0
                                        : reader.GetInt32(reader.GetOrdinal("ExcusedCount"))
                                };

                                sessions.Add(session);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        EventLogger.LogError($"Database Error While trying to retrieve sessions with Group ID {GroupID} data!", ex);
                        throw new ApplicationException($"Database Error While trying to retrieve sessions with Group ID {GroupID} data!", ex);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Error While trying to retrieve sessions With Group ID {GroupID} data!", ex);
                        throw new ApplicationException($"Error While trying to retrieve sessions With Group ID {GroupID} data!", ex);
                    }
                }
            }

            return sessions;
        }

        /// <summary>
        /// إضافة جلسة تدريبية جديدة
        /// </summary>
        /// <param name="session">كائن SessionDTO يحتوي على بيانات الجلسة الجديدة</param>
        /// <returns>معرف الجلسة المضافة، أو null إذا فشلت الإضافة</returns>
        /// <exception cref="ValidationException">خطأ في التحقق من البيانات (تعارض مواعيد، إلخ)</exception>
        public static async Task<int?> AddNewSession(SessionDTO session)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_AddSession", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // المعاملات المطلوبة
                    command.Parameters.AddWithValue("@GroupID", session.GroupID);
                    command.Parameters.AddWithValue("@SessionDate", session.SessionDate);
                    command.Parameters.AddWithValue("@StartTime", session.StartTime);
                    command.Parameters.AddWithValue("@EndTime", session.EndTime);
                    command.Parameters.AddWithValue("@SessionType", session.SessionType);
                    command.Parameters.AddWithValue("@Location", session.Location);
                    command.Parameters.AddWithValue("@Topic", session.Topic);

                    // المعاملات الاختيارية
                    command.Parameters.AddWithValue("@TrainerID",
                        session.TrainerID.HasValue ? (object)session.TrainerID.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@Status", session.Status ?? "Scheduled");
                    command.Parameters.AddWithValue("@Notes",
                        string.IsNullOrEmpty(session.Notes) ? DBNull.Value : (object)session.Notes);
                    command.Parameters.AddWithValue("@IsActive", session.IsActive);
                    command.Parameters.AddWithValue("@CreatedByUserID", 
                        session.CreatedByUserID.HasValue ? (object)session.CreatedByUserID.Value : DBNull.Value);

                    // معامل الإخراج
                    var outputParam = new SqlParameter("@NewSessionID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputParam);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        return outputParam.Value != DBNull.Value
                            ? (int)outputParam.Value
                            : (int?)null;
                    }
                    catch (SqlException ex) when (ex.Number >= 51000 && ex.Number <= 51100)
                    {
                        // أخطاء التحقق المخصصة
                        EventLogger.LogError($"Validation failed: {ex.Message}", ex);
                        throw new ApplicationException($"Validation failed: {ex.Message}", ex);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.LogError($"Validation failed: {ex.Message}", ex);
                        throw new ApplicationException($"Validation failed: {ex.Message}", ex);
                    }
                }
            }

        }

        public static async Task<(bool Success, string Message)> UpdateSession(SessionDTO session)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_UpdateSession", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // المعاملات المطلوبة
                    command.Parameters.AddWithValue("@SessionID", session.SessionID);
                    command.Parameters.AddWithValue("@GroupID", session.GroupID);
                    command.Parameters.AddWithValue("@SessionDate", session.SessionDate);
                    command.Parameters.AddWithValue("@StartTime", session.StartTime);
                    command.Parameters.AddWithValue("@EndTime", session.EndTime);
                    command.Parameters.AddWithValue("@SessionType", session.SessionType);
                    command.Parameters.AddWithValue("@Location", session.Location);
                    command.Parameters.AddWithValue("@Topic", session.Topic);
                    command.Parameters.AddWithValue("@Status", session.Status);
                    command.Parameters.AddWithValue("@IsActive", session.IsActive);

                    // المعاملات الاختيارية
                    command.Parameters.AddWithValue("@TrainerID",
                        session.TrainerID.HasValue ? (object)session.TrainerID.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@Notes",
                        string.IsNullOrEmpty(session.Notes) ? DBNull.Value : (object)session.Notes);

                    try
                    {
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return (
                                    Success: reader.GetBoolean(reader.GetOrdinal("Success")),
                                    Message: reader.GetString(reader.GetOrdinal("Message"))
                                );
                            }

                            return (false, "No result returned from stored procedure");
                        }
                    }
                    catch (SqlException ex) when (ex.Number >= 52000 && ex.Number <= 52100)
                    {
                        EventLogger.LogError($"Update validation failed: {ex.Message}", ex);
                        throw new ApplicationException($"Update validation failed: {ex.Message}", ex);
                    }
                }
            }
        }

        public static async Task<(bool Success, string Message, int SessionID)> 
            CancelSession(int SessionID, int? CancelledByUser = null, string CancellationReason = null)
        {
            using (SqlConnection connection = DataBaseSettings.GetConnection())
            {
                using (SqlCommand command = new SqlCommand("SP_CancelSession", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@SessionID", SqlDbType.Int).Value = SessionID;
                    command.Parameters.Add("@CancelledBy", SqlDbType.Int).Value = CancelledByUser.HasValue
                        ? (object)CancelledByUser.Value
                        : DBNull.Value;
                    command.Parameters.Add("@CancellationReason", SqlDbType.NVarChar, 500).Value = string.IsNullOrEmpty(CancellationReason)
                        ? DBNull.Value
                        : (object)CancellationReason;

                    try
                    {
                        await connection.OpenAsync();

                        using(SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if(await reader.ReadAsync())
                            {
                                return (
                                    Success: reader.GetBoolean(reader.GetOrdinal("Success")),
                                    Message: reader.GetString(reader.GetOrdinal("Message")),
                                    SessionID: reader.GetInt32(reader.GetOrdinal("SessionID"))
                                );
                            }

                            return (false, "No result returned", 0);
                        }
                    }
                    catch (SqlException ex) when (ex.Number >= 53000 && ex.Number <= 53100)
                    {
                        EventLogger.LogError($"Cancellation failed: {ex.Message}", ex);
                        throw new ApplicationException($"Cancellation failed: {ex.Message}", ex);
                    }
                }
            }
        }
    }
}
