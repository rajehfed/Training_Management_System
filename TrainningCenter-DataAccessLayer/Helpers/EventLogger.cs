using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace TrainningCenter_DataAccessLayer.Helpers
{
    public static class EventLogger
    {
        //private const string SOURCE_NAME = "TrainingCenterApp";
        //private const string LOG_NAME = "Application";

        private const string SOURCE_NAME = "TrainingCenterApp";
        private const string LOG_NAME = "Application";
        private const string FILE_LOG = "log.txt";

        // تهيئة Event Source
        static EventLogger()
        {
            try
            {
                if (!EventLog.SourceExists(SOURCE_NAME)) {
                    EventLog.CreateEventSource(SOURCE_NAME, LOG_NAME);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(FILE_LOG,
                    $"{DateTime.Now}: Failed to create event source: {ex.Message}{Environment.NewLine}");

            }
        }

        public static void LogError(string message, Exception ex = null)
        {
            string formattedMessage = $"{message}\n\nException:\n{ex.ToString()}";
            LogToEventViewer(formattedMessage, EventLogEntryType.Error);
        }
        public static void LogWarning(string message)
        {
            LogToEventViewer(message, EventLogEntryType.Warning);
        }
        public static void LogInfo(string message)
        {
            LogToEventViewer(message, EventLogEntryType.Information);
        }

        private static void LogToEventViewer(string message, EventLogEntryType type)
        {
            try
            {
                // 1. Try writing to Windows Event Log
                if (!EventLog.SourceExists(SOURCE_NAME))
                {
                    EventLog.CreateEventSource(SOURCE_NAME, "Application");
                }
                EventLog.WriteEntry(SOURCE_NAME, message, type);
            }
            catch (Exception)
            {
                // 2. If Event Log fails (Permission denied), write to a local Text File instead
                // This prevents the app from crashing!
                LogToFile(message, type);
            }
        }

        private static void LogToFile(string message, EventLogEntryType type)
        {
            try
            {
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = Path.Combine(folderPath, $"Log_{DateTime.Now:yyyy-MM-dd}.txt");
                string logContent = $"[{DateTime.Now:HH:mm:ss}] [{type}] {message}{Environment.NewLine}--------------------------------{Environment.NewLine}";

                File.AppendAllText(filePath, logContent);
            }
            catch
            {
                // If even file logging fails, just write to Debug Output window (Last Resort)
                Debug.WriteLine($"FAILED TO LOG: {message}");
            }
        }
    }
}
