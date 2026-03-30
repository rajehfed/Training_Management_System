using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer.Helpers;
using TrainningCenter_DataAccessLayer;
using TrainningCenter_Entities;
using static TrainningCenter_DataAccessLayer.clsGroups;

namespace TrainingCenter_BusinessLayer
{
    /// <summary>
    /// Represents a Group entity in the Business Logic Layer (BLL).
    /// This class handles data mapping and business operations (CRUD) for a training group.
    /// </summary>
    public class Group
    {
        enum enMode { AddNew = 0, Update = 1 }
        private enMode _Mode = enMode.AddNew;
        public int? GroupID { get; set; }
        public int SpecializationID { get; set; }
        public int TrainerID { get; set; }
        public int CenterID { get; set; }
        public string GroupName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxTrainees { get; set; }
        public int CurrentTrainees { get; set; }
        public string Schedule { get; set; }
        public string Room { get; set; }
        public enStatus Status { get; set; }
        public decimal BaseFee { get; set; }
        public string StatusName {
            get
            {
                switch (Status)
                {
                    case enStatus.stSuspended: return "Suspended";
                    case enStatus.stActive: return "Active";
                    case enStatus.stCompleted: return "Completed";
                    case enStatus.stCancelled: return "Cancelled";
                    default: return "Active";
                }
            }
        }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdateAt { get; set; }
        public bool IsActive { get; set; }

        public Specialization SpecializationOfGroup;
        public enStatus ConvertStatusStringToEnum(string statusName)
        {
            switch (statusName)
            {
                case "Suspended": return enStatus.stSuspended;
                case "Active": return enStatus.stActive;
                case "Completed": return enStatus.stCompleted;
                case "Cancelled": return enStatus.stCancelled;
                default: return enStatus.stActive;
            }
        }
        public Group()
        {
            this.GroupID = -1;
            this.SpecializationID = -1;
            this.TrainerID = -1;
            this.CenterID = -1;
            this.GroupName = string.Empty;
            this.StartDate = DateTime.Now;
            this.EndDate = DateTime.Now.AddDays(1);
            this.MaxTrainees = 0;
            this.CurrentTrainees = 0;
            this.Schedule = string.Empty;
            this.Room = string.Empty;
            this.Status = (enStatus)enStatus.stActive;
            this.CreatedAt = DateTime.Now;
            this.IsActive = true;
            this.BaseFee = 0m;

            _Mode = enMode.AddNew;
        }
        private Group(DataRow row)
        {
            this.GroupID = (int)row["GroupID"];
            this.SpecializationID = (int)row["SpecializationID"];
            this.TrainerID = (int)row["TrainerID"];
            this.CenterID = (int)row["CenterID"];
            this.GroupName = row["GroupName"].ToString();
            this.StartDate = Convert.ToDateTime(row["StartDate"]);
            this.EndDate = Convert.ToDateTime(row["EndDate"]);
            this.MaxTrainees = (int)row["MaxTrainees"];
            this.CurrentTrainees = (int)row["CurrentTrainees"];
            this.Schedule = row["Schedule"].ToString();
            this.Room = row["Room"].ToString();
            this.Status = ConvertStatusStringToEnum(row["Status"].ToString());
            this.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
            this.IsActive = (bool)row["IsActive"];
            this.BaseFee = Convert.ToDecimal(row["BaseFee"]);

            _Mode = enMode.Update;
        }
        private Group(GroupDTO dto)
        {
            this.GroupID = dto.GroupID;
            this.SpecializationID = dto.SpecializationID;
            this.TrainerID = dto.TrainerID.Value;
            this.CenterID = dto.CenterID.Value;
            this.GroupName = dto.GroupName;
            this.StartDate = dto.StartDate;
            this.EndDate = dto.EndDate;
            this.MaxTrainees = dto.MaxTrainees;
            this.CurrentTrainees = dto.CurrentTrainees;
            this.Schedule = dto.Schedule;
            this.Room = dto.Room;
            this.Status = ConvertStatusStringToEnum(dto.Status);
            this.CreatedAt = dto.CreatedAt;
            this.LastUpdateAt = dto.UpdatedAt;
            this.IsActive = dto.IsActive;
            this.BaseFee = dto.BaseFee;

            _Mode = enMode.Update;
        }

        /// <summary>
        /// This function for create an instanse and fill all the Composed class like <see cref="Specialization"/> Asyncronesly
        /// </summary>
        /// <param name="row">this is a data row that should be comming from the static functions of the DAL</param>
        /// <returns></returns>
        private static async Task<Group> FromDataRowAsync(DataRow row)
        {
            // First we need to extract the Group
            Group group = new Group(row);

            group.SpecializationOfGroup = await Specialization.Find(group.SpecializationID);

            return group;
        }
        public static async Task<Group> FromDTOAsync(GroupDTO dto)
        {
            // First we need to extract the Group
            Group group = new Group(dto);

            group.SpecializationOfGroup = await Specialization.Find(group.SpecializationID);

            return group;
        }

        /// <summary>
        /// Asynchronously retrieves a list of all groups currently stored in the database.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> containing a <see cref="List{T}"/> of all <see cref="Group"/> objects.</returns>
        public static async Task<List<Group>> GetAllGroups()
        {
            var dt = await clsGroups.GetAllGroups();

            return dt.Rows.Cast<DataRow>()
                .Select(row => new Group(row))
                .ToList() ?? new List<Group>();
        }
        /// <summary>
        /// Asynchronously retrieves a list of groups filtered by a specific Specialization ID.
        /// </summary>
        /// <param name="SpecializationID">The ID of the specialization to filter by.</param>
        /// <returns>A <see cref="Task{TResult}"/> containing a <see cref="List{T}"/> of <see cref="Group"/> objects matching the specialization.</returns>
        public static async Task<List<Group>> GetGroupsBySpecialization(int SpecializationID)
        {
            var dt = await clsGroups.GetGroupsBySpecialization(SpecializationID);

            return dt.Rows.Cast<DataRow>()
                .Select(row => new Group(row))
                .ToList() ?? new List<Group>();
        }
        public static async Task<List<Group>> GetGroupsBySpecializationDTO(int SpecializationID)
        {
            var dt = await clsGroups.GetGroupsBySpecializationDTO(SpecializationID);

            return dt.Cast<GroupDTO>()
                .Select(dto => new Group(dto))
                .ToList() ?? new List<Group>();
        }
        /// <summary>
        /// Asynchronously calculates the total number of students currently enrolled across all groups belonging to a specific specialization.
        /// </summary>
        /// <param name="specializationID">The ID of the specialization to aggregate counts for.</param>
        /// <returns>A <see cref="Task{TResult}"/> containing the total count of students.</returns>
        public static async Task<int> GetStudentCountBySpecialization(int specializationID)
        {
            return await clsGroups.GetStudentCountBySpecialization(specializationID);
        }
        /// <summary>
        /// Asynchronously retrieves a paginated and filtered list of groups for display.
        /// </summary>
        /// <param name="onlyActive">If <c>true</c>, filters the list to include only active groups (IsActive = 1).</param>
        /// <param name="searchQuery">Optional text to search within group names or rooms.</param>
        /// <param name="statusFilter">Optional string status (e.g., "Active") to filter the list.</param>
        /// <param name="pageNumber">The 1-based index of the page to retrieve.</param>
        /// <param name="pageSize">The maximum number of records per page (default is 20).</param>
        /// <returns>A <see cref="GroupsPagedList"/> object containing the paged list of DTOs and the total record count.</returns>
        public static async Task<GroupsPagedList> GetAllGroupsByPage(
            bool onlyActive, 
            string searchQuery, 
            string statusFilter, 
            int pageNumber, 
            int pageSize = 20
        ) {
            var groupsList = await clsGroups.GetGroupsList(onlyActive, searchQuery, statusFilter, pageNumber, pageSize);

            return groupsList;
        }
        /// <summary>
        /// Asynchronously retrieves a specific Group entity from the database by its unique identifier.
        /// </summary>
        /// <param name="groupID">The unique Group ID to search for.</param>
        /// <returns>A <see cref="Group"/> object containing the mapped data, or <c>null</c> if the group is not found.</returns>
        public static async Task<Group> Find(int groupID)
        {
            var group = await clsGroups.GetGroupByID(groupID);

            if (group == null)
                return null;

            return await FromDTOAsync(group);
        }
        private async Task<bool> _UpdateGroup() {
            try
            {
                this.LastUpdateAt = DateTime.Now;
                bool isUpdated = await clsGroups.UpdateGroup(
                    this.GroupID.Value,
                    this.SpecializationID,
                    this.TrainerID,
                    this.CenterID,
                    this.GroupName,
                    this.StartDate,
                    this.EndDate,
                    this.MaxTrainees,
                    this.CurrentTrainees,
                    this.Schedule,
                    this.Room,
                    (enStatus)this.Status);

                if (isUpdated)
                {
                    EventLogger.LogInfo($"Group ID {this.GroupID} updated successfully.");
                    //var result = await 
                }
                else
                    EventLogger.LogWarning($"UpdateGroup returned false for ID {this.GroupID}.");

                return isUpdated;
            }
            catch(Exception ex)
            {
                EventLogger.LogError($"Error Updating Group (ID {this.GroupID}): {ex}");
                return false;
            }
        }
        private async Task<bool> _AddNewGroup()
        {
            this.GroupID = await clsGroups.AddNewGroup(
                this.SpecializationID, 
                this.TrainerID, 
                this.CenterID,
                this.GroupName, 
                this.StartDate, 
                this.EndDate, 
                this.MaxTrainees, 
                this.CurrentTrainees, 
                this.Schedule, 
                this.Room, 
                (enStatus)this.Status);

            return this.GroupID != null;
        }
        /// <summary>
        /// Saves the current Group instance to the database.
        /// Performs an INSERT operation if <see cref="GroupID"/> is null (Mode: AddNew)
        /// or an UPDATE operation if <see cref="GroupID"/> has a value (Mode: Update).
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> returning <c>true</c> if the save was successful; otherwise, <c>false</c>.</returns>
        public async Task<bool> Save()
        {
            switch (_Mode)
            {
                case enMode.AddNew:
                    if (await _AddNewGroup())
                    {
                        _Mode = enMode.Update;
                        return true;
                    }
                    else
                        return false;

                case enMode.Update:
                    return await _UpdateGroup();
            }

            return false;
        }
        /// <summary>
        /// Permanently deletes the current Group record and all dependent records from the database.
        /// This method should only be called on an existing group (i.e., <see cref="GroupID"/> must have a value).
        /// </summary>
        /// <remarks>
        /// This performs a **Hard Delete** via the DAL and results in permanent data loss.
        /// </remarks>
        /// <returns>A <see cref="Task{TResult}"/> returning <c>true</c> if the group was successfully deleted; otherwise, <c>false</c>.</returns>
        public async Task<bool> Delete(int createdByUser)
        {
            try
            {
                return await clsGroups.HardDeleteGroup(this.GroupID.Value, createdByUser);
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"Error deleting Group (ID {this.GroupID}): {ex}");
                return false;
            }
        }
        /// <summary>
        /// Performs a soft delete operation on the current Group by setting its IsActive flag to false (Archived).
        /// </summary>
        /// <param name="updatedByUserID">The ID of the user performing the soft delete, used for auditing.</param>
        /// <returns>A <see cref="Task{TResult}"/> returning <c>true</c> if the group was successfully inactivated; otherwise, <c>false</c>.</returns>
        public async Task<bool> Inactivate(int createdByUser)
        {
            try
            {
                return await clsGroups.SoftDeleteGroup(this.GroupID.Value, createdByUser);
            }
            catch (Exception ex)
            {
                // Corrected logging message to reflect Soft Delete/Inactivation
                EventLogger.LogError($"Error inactivating Group (ID {this.GroupID}): {ex}");
                return false;
            }
        }
    }
}
