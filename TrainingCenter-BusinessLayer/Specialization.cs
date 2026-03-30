using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer.Helpers;
using TrainningCenter_DataAccessLayer;

namespace TrainingCenter_BusinessLayer
{
    public class Specialization
    {
        public enum enMode { AddNew = 0, Update = 1 }
        private enMode _Mode = enMode.AddNew;

        public int SpecializationID { get; set; }
        public string SpecializationName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public int? Duration { get; set; }
        public decimal? Price { get; set; }
        public string Prerequisites { get; set; }
        public string CertificateTemplate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }

        /// <summary>
        /// this constractor to create new instance from the Class
        /// </summary>
        public Specialization()
        {
            this.SpecializationID = -1;
            this.SpecializationName = string.Empty;
            this.Description = string.Empty;
            this.Category = string.Empty;
            this.Level = string.Empty;
            this.Duration = 0;
            this.Price = 00.00m;
            this.Prerequisites = string.Empty;
            this.IsActive = true;
            this.CertificateTemplate = string.Empty;
            this.CreatedAt = DateTime.Now;
            this.LastUpdatedAt = null;

            this._Mode = enMode.AddNew;
        }

        /// <summary>
        /// this Private constractor to update the Specialization
        /// </summary>
        /// <param name="SpecializationRow"></param>
        private Specialization(DataRow SpecializationRow)
        {
            this.SpecializationID = (int)SpecializationRow["SpecializationID"];
            this.SpecializationName = SpecializationRow["SpecializationName"].ToString();
            this.Description = SpecializationRow["Description"] == DBNull.Value
                ? string.Empty
                : SpecializationRow["Description"].ToString();
            this.Category = SpecializationRow["Category"] == DBNull.Value
                ? string.Empty
                : SpecializationRow["Category"].ToString();
            this.Level = SpecializationRow["Level"] == DBNull.Value
                ? string.Empty
                : SpecializationRow["Level"].ToString();
            this.Duration = SpecializationRow["Duration"] == DBNull.Value
                ? 0
                : Convert.ToInt32(SpecializationRow["Duration"]);
            this.Price = SpecializationRow["Price"] == DBNull.Value
                ? 00.00m
                : Convert.ToDecimal(SpecializationRow["Price"]);
            this.Prerequisites = SpecializationRow["Prerequisites"] == DBNull.Value
                ? string.Empty
                : SpecializationRow["Prerequisites"].ToString();
            this.CertificateTemplate = SpecializationRow["CertificateTemplate"] == DBNull.Value
                ? string.Empty
                : SpecializationRow["CertificateTemplate"].ToString();
            this.IsActive = (bool)SpecializationRow["IsActive"];
            this.CreatedAt = Convert.ToDateTime(SpecializationRow["CreatedAt"]);
            this.LastUpdatedAt = SpecializationRow["UpdatedAt"] == DBNull.Value
                ? null
                : (DateTime?)SpecializationRow["UpdatedAt"];

            _Mode = enMode.Update;
        }

        /// <summary>
        /// Asynchronously retrieves a list of all specializations.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="Specialization"/> objects representing all available specializations. If no specializations are found,
        /// returns an empty list.</returns>
        public static async Task<List<Specialization>> GetAllSpecializations()
        {
            DataTable dt = await clsSpecialization.GetAllSpecializations();

            return dt.Rows.Cast<DataRow>()
                .Select(row => new Specialization(row))
                .ToList() ?? new List<Specialization>();
        }

        /// <summary>
        /// Asynchronously retrieves a list of all active specializations.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of <see
        /// cref="Specialization"/> objects representing the active specializations. The list will be empty if no active
        /// specializations are found.</returns>
        public static async Task<List<Specialization>> GetAllActiveSpecializations()
        {
            DataTable dt = await clsSpecialization.GetAllActiveSpecializations();

            return dt.Rows.Cast<DataRow>()
                .Select(row => new Specialization(row))
                .ToList() ?? new List<Specialization>();
        }

        /// <summary>
        /// Helper method to add a new specialization to the database.
        /// </summary>
        /// <returns>
        /// boolean value indicating whether the addition was successful.
        /// </returns>
        private async Task<bool> _AddNewSpecialization()
        {
            this.SpecializationID = await clsSpecialization.AddNewSpecialization(
                this.SpecializationName,
                this.Description,
                this.Category,
                this.Level,
                this.Duration,
                this.Price,
                this.Prerequisites,
                this.CertificateTemplate
            );

            return this.SpecializationID != -1;
        }
        /// <summary>
        /// Helper method to update an existing specialization in the database.
        /// </summary>
        /// <returns>
        /// boolean value indicating whether the update was successful.
        /// </returns>
        private async Task<bool> _UpdateSpecialization()
        {
            try
            {
                this.LastUpdatedAt = DateTime.Now;
                bool isUpdated = await clsSpecialization.UpdateSpecialization(
                    SpecializationID,
                    SpecializationName,
                    Description,
                    Category,
                    Level,
                    Duration,
                    Price,
                    Prerequisites,
                    CertificateTemplate,
                    IsActive
                );

                if (isUpdated)
                    EventLogger.LogInfo($"Specialization ID {this.SpecializationID} updated successfully.");
                else
                    EventLogger.LogWarning($"Update Specialization returned false for ID {this.SpecializationID}.");

                return isUpdated;
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"Error updating Specialization (ID {this.SpecializationID})", ex);
                return false;
            }
        }
        /// <summary>
        /// This method saves the current specialization instance.
        /// based on the mode (AddNew or Update), it either adds a new specialization
        /// </summary>
        /// <returns>
        /// a boolean value indicating whether the save operation was successful.
        /// </returns>
        public async Task<bool> Save()
        {
            switch (_Mode)
            {
                case enMode.AddNew:
                    if (await _AddNewSpecialization())
                    {
                        _Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return await _UpdateSpecialization();
            }

            return false;
        }
        public static async Task<Specialization> Find(int SpecializationID)
        {
            try {
                DataRow row = await clsSpecialization.GetSpecializationById(SpecializationID);
                if (row != null)
                {
                    EventLogger.LogInfo($"Successfully Find the Specialization With the ID {SpecializationID}.");
                    return new Specialization(row);
                }

                return null;
            }
            catch (Exception ex) {
                EventLogger.LogError($"Error finding Specialization (ID {SpecializationID})", ex);
                return null;
            }
        }
        public async Task<bool> Delete()
        {
            try
            {
                bool isDeactivated = await clsSpecialization.DeleteSpecialization(this.SpecializationID);
                if (isDeactivated)
                {
                    this.IsActive = false;
                    EventLogger.LogInfo($"Specialization ID {this.SpecializationID} deactivated successfully.");
                }
                else
                {
                    EventLogger.LogWarning($"Deactivate Specialization returned false for ID {this.SpecializationID}.");
                }
                return isDeactivated;
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"Error deactivating Specialization (ID {this.SpecializationID})", ex);
                return false;
            }
        }
    }
}