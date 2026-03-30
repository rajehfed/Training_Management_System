using System;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Security;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace TrainningCenter.Helpers
{
    public static class Utils
    {
        private const string UserValueName = "UserName";
        private const string PasswordValueName = "Password";

        public static int CalculateAge(DateTime dateOfBirth)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age))
                age--;

            return age;
        }
        public static bool SaveCredentials(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username)) return false;
                if (password == null) password = string.Empty;

                byte[] plainBytes = Encoding.UTF8.GetBytes(password);
                byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
                string encryptedBase64 = Convert.ToBase64String(encryptedBytes);

                Registry.SetValue(AppPaths.keyPath, UserValueName, username, RegistryValueKind.String);
                Registry.SetValue(AppPaths.keyPath, PasswordValueName, encryptedBase64, RegistryValueKind.String);

                return true;
            }
            catch (Exception ex)
            {
                EventLogger.LogWarning($"An error occurred while saving credentials: {ex.Message}");
                return false;
            }
        }
        public static bool LoadCredentials(ref string username, ref string password)
        {
            try
            {
                username = Registry.GetValue(AppPaths.keyPath, UserValueName, null) as string;
                string encryptedBase64 = Registry.GetValue(AppPaths.keyPath, PasswordValueName, null) as string;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(encryptedBase64))
                    return false;

                // Unhashing the password
                byte[] encrypted = Convert.FromBase64String(encryptedBase64);
                byte[] plain = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                password = Encoding.UTF8.GetString(plain);

                return true;
            }
            catch (Exception ex)
            {
                EventLogger.LogWarning($"An Error Was Occured: {ex.Message}");
                return false;
            }
        }
        public static bool DeleteCredentials()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(AppPaths.keyPath, writable: true))
                {
                    if(key != null)
                    {
                        key.DeleteValue(UserValueName, false);
                        key.DeleteValue(PasswordValueName, false);
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                EventLogger.LogWarning($"Failed to delete credentials: {ex.Message}");
                return false;
            }
        }
        public static Brush GetBrush(string resourceKey)
        {
            // Tries to find the resourse across the entire application scope
            object resource = Application.Current.FindResource(resourceKey);

            // Check if the resource is found AND if it is a Brush type
            if(resource is Brush brush)
            {
                return brush;
            }

            // If not found or wrong type, return null or a fallback brush (e.g., Brushes.Black)
            return null;
        }
    }
}
