using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter_BusinessLayer.Helpers
{
    public class EventLogger
    {
        private const string SOURCE_NAME = "TrainingCenterApp";
        private const string LOG_NAME = "Application";

        // تهيئة Event Source
        static EventLogger()
        {
            try
            {
                if (!EventLog.SourceExists(SOURCE_NAME))
                {
                    EventLog.CreateEventSource(SOURCE_NAME, LOG_NAME);
                }
            }
            catch (Exception ex)
            {
                // يحتاج صلاحيات Administrator لإنشاء Event Source
                Console.WriteLine($"Failed to create event source: {ex.Message}");
            }
        }

        public static void LogError(string message, Exception ex = null)
        {
            try
            {
                string fullMessage = message;
                if (ex != null)
                {
                    fullMessage += $"\n\nException: {ex.Message} \nStackTrace: {ex.StackTrace}";
                }

                EventLog.WriteEntry(SOURCE_NAME, fullMessage, EventLogEntryType.Error);
            }
            catch (Exception)
            {
                // فشل تسجيل الحدث، لا يمكن فعل الكثير هنا
                // Fallback to console if event log fails
                //Console.WriteLine($"Error: {message}");
            }
        }
        public static void LogWarning(string message)
        {
            try
            {
                EventLog.WriteEntry(SOURCE_NAME, message, EventLogEntryType.Warning);
            }
            catch (Exception)
            {
                // فشل تسجيل الحدث، لا يمكن فعل الكثير هنا
                // Fallback to console if event log fails
                //Console.WriteLine($"Warning: {message}");
            }
        }
        public static void LogInfo(string message)
        {
            try
            {
                EventLog.WriteEntry(SOURCE_NAME, message, EventLogEntryType.Information);
            }
            catch (Exception)
            {
                // فشل تسجيل الحدث، لا يمكن فعل الكثير هنا
                // Fallback to console if event log fails
                //Console.WriteLine($"Info: {message}");
            }
        }
    }
}
