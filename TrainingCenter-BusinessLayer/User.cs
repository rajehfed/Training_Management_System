using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer.Helpers;
using TrainningCenter_DataAccessLayer;

namespace TrainingCenter_BusinessLayer
{
    public class User
    {
        public enum enRole { eNone = 0, eAdmin = 1, eTrainer = 2, eStudent = 4, eReceptionist = 8, eManager = 16 }
        public enum enMode { AddNew = 1, Update = 2 }
        private enMode _Mode = enMode.AddNew;
        public int UserId { get; set; }
        public int PersonId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public enRole Role { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Person PersonUser { get; set; }
        public User()
        {
            this.UserId = -1;
            this.PersonId = -1;
            this.Username = string.Empty;
            this.Role = enRole.eNone;
            this.RoleName = "NONE";
            this.IsActive = true;

            this.PersonUser = new Person();

            _Mode = enMode.AddNew;
        }
        private User(DataRow userRow)
        {
            UserId = (int)userRow["UserID"];
            PersonId = (int)userRow["PersonID"];
            Username = userRow["UserName"].ToString();
            RoleName = userRow["Role"].ToString();
            IsActive = (bool)userRow["IsActive"];
            LastLogin = userRow["LastLogin"] == DBNull.Value ? default(DateTime) : Convert.ToDateTime(userRow["LastLogin"]);
            CreatedAt = userRow["CreatedAt"] == DBNull.Value ? default(DateTime) : Convert.ToDateTime(userRow["CreatedAt"]);
            UpdatedAt = userRow["UpdatedAt"] == DBNull.Value ? default(DateTime) : Convert.ToDateTime(userRow["UpdatedAt"]);
            RoleName = (string)userRow["Role"];
            Role = RoleFromRoleString(RoleName);

            _Mode = enMode.Update;
        }
        public async static Task<List<User>> GetAllUser()
        {
            DataTable dt = await clsUser.GetAllActiveUsers();

            return dt.Rows.Cast<DataRow>()
                .Select(row => new User(row))
                .ToList() ?? new List<User>();
        }
        public static async Task<User> FromDataRowAsync(DataRow row)
        {
            // First We need to constract the User
            User user = new User(row);

            user.PersonUser = await Person.Find(user.PersonId);

            return user;
        }
        private string RoleFromRoleEnum(enRole role)
        {
            switch (role)
            {
                case enRole.eStudent:
                    return "Student";

                case enRole.eAdmin:
                    return "Admin";

                case enRole.eTrainer:
                    return "Trainer";

                case enRole.eReceptionist:
                    return "Receptionist";

                case enRole.eManager:
                    return "Manager";

                default:
                    return "None";
            }
        }
        private enRole RoleFromRoleString(string role)
        {
            switch (role)
            {
                case "Student":
                    return enRole.eStudent;

                case "Admin":
                    return enRole.eAdmin;

                case "Trainer":
                    return enRole.eTrainer;

                case "Receptionist":
                    return enRole.eReceptionist;

                case "Manager":
                    return enRole.eManager;

                default:
                    return enRole.eNone;
            }
        }

        /// <summary>
        /// Asynchronously finds and returns a <see cref="User"/> object 
        /// based on the specified UserID.
        /// </summary>
        /// <param name="userID">The unique identifier of the user to retrieve.</param>
        /// <returns>
        /// A <see cref="User"/> object containing user information if found; 
        /// otherwise, <c>null</c> if the provided ID is invalid or no user exists.
        /// </returns>
        /// <remarks>
        /// This method first validates the UserID, then calls 
        /// <see cref="clsUser.GetUserById(int)"/> to fetch the user data from 
        /// the database, and finally maps the result to a <see cref="User"/> 
        /// object using <see cref="FromDataRowAsync(DataRow)"/>.
        /// </remarks>
        /// <example>
        /// Example usage:
        /// <code>
        /// User user = await User.Find(3);
        /// if (user != null)
        ///     Console.WriteLine($"Welcome back, {user.Username}!");
        /// else
        ///     Console.WriteLine("User not found.");
        /// </code>
        /// </example>
        public static async Task<User> Find(int userID)
        {
            if (userID <= 0)
                return null;

            DataRow userRow = await clsUser.GetUserById(userID);

            return await FromDataRowAsync(userRow);
        }

        /// <summary>
        /// Asynchronously adds a new user to the database and updates the UserId 
        /// property with the newly created user's ID.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the user was successfully added (UserId != -1); 
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method internally calls <see cref="clsUser.AddNewUser"/> 
        /// to insert a new record and assign the resulting UserId to the current instance.
        /// </remarks>
        private async Task<bool> _AddNewUser()
        {
            this.UserId = await clsUser.AddNewUser(this.PersonId, this.Username, this.Password, this.RoleName);

            return this.UserId != -1;
        }
        /// <summary>
        /// Asynchronously updates the existing user information in the database.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the user was successfully updated; 
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Updates the <see cref="UpdatedAt"/> timestamp before calling 
        /// <see cref="clsUser.UpdateUser"/>. 
        /// Logs success, warnings, or errors using <see cref="EventLogger"/>.
        /// </remarks>
        private async Task<bool> _UpdateUser()
        {
            try
            {
                this.UpdatedAt = DateTime.Now;
                bool isUpdated = await clsUser.UpdateUser(
                    UserId,
                    PersonId,
                    Username,
                    Password,
                    RoleName
                );

                if (isUpdated)
                    EventLogger.LogInfo($"User ID {this.UserId} updated successfully.");
                else
                    EventLogger.LogWarning($"UpdateUser returned false for ID {this.UserId}.");

                return isUpdated;
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"Error updating User (ID {this.UserId}): {ex}");

                return false;
            }
        }
        public async Task<bool> Save()
        {
            switch (_Mode)
            {
                case enMode.AddNew:
                    if (await _AddNewUser())
                    {
                        _Mode = enMode.Update;
                        return true;
                    }
                    else
                        return false;

                case enMode.Update:
                    return await _UpdateUser();
            }

            return false;
        }
        public async Task<bool> IsUserExists(string username, string password)
        {
            return await clsUser.IsUserExists(username, password);
        }

        /// <summary>
        /// Asynchronously finds and returns a User object based on username.
        /// </summary>
        /// <param name="username">The username to search for</param>
        /// <returns>A User object if found; otherwise null</returns>
        public static async Task<User> FindByUsername(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;

            DataRow userRow = await clsUser.GetUserByUsername(username);

            if (userRow == null) return null;

            return await FromDataRowAsync(userRow);
        }
        /// <summary>
        /// Asynchronously authenticates a user with username and password.
        /// </summary>
        /// <param name="username">The username to authenticate</param>
        /// <param name="password">The password to verify</param>
        /// <returns>
        /// A User object if authentication succeeds (credentials match and user is active);
        /// otherwise null
        /// </returns>
        public static async Task<User> Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            try
            {
                // Check if credentials are valid
                bool credentialsValid = await clsUser.IsUserExists(username, password);

                if (!credentialsValid)
                    return null;

                User user = await FindByUsername(username);

                // Verify user is active
                if (user != null && user.IsActive)
                    return user;

                return null;
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"Authentication error for username {username}: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Gets the role display name for UI purposes.
        /// </summary>
        public string RoleDisplayName
        {
            get
            {
                return RoleFromRoleEnum(Role);
            }
        }

        public string Initials
        {
            get
            {
                // 1. Check if PersonUser object exists to avoid NullReferenceException
                if (this.PersonUser == null) return "?";

                // 2. Safely get the first letter of FirstName and LastName
                char? f = !string.IsNullOrWhiteSpace(this.PersonUser.FirstName)
                          ? this.PersonUser.FirstName.Trim()[0] : (char?)null;

                char? l = !string.IsNullOrWhiteSpace(this.PersonUser.LastName)
                          ? this.PersonUser.LastName.Trim()[0] : (char?)null;

                if (f == null && l == null)
                    return "?";

                // 3. Return the combined initials in uppercase
                return $"{f}{l}".ToUpper();
            }
        }
        /// <summary>
        /// Checks if the user has a specific role.
        /// </summary>
        public bool HasRole(enRole role)
        {
            return (this.Role & role) == role;
        }
        /// <summary>
        /// Checks if the user has manager privileges.
        /// </summary>
        public bool IsManager
        {
            get { return HasRole(enRole.eManager); }
        }
        public async Task<bool> Delete()
        {
            try {
                return await clsUser.DeactivateUser(this.UserId);
            }
            catch (Exception ex) {
                EventLogger.LogError($"Error deleting User (ID {this.UserId}): {ex}");
                return false;
            }
        }
    }
}
