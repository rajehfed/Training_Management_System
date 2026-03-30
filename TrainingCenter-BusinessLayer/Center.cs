using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer.Helpers;
using TrainningCenter_DataAccessLayer;
using TrainningCenter_Entities;

namespace TrainingCenter_BusinessLayer
{
    public class Center
    {
        public enum enMode { AddNew, Update }
        private enMode _Mode = enMode.AddNew;
        public int? CenterID { get; set; }
        public string CenterName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Manager { get; set; }
        public int Capacity { get; set; }
        public string Facilities { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        // Properties
        public int TrainersCount { get; set; }
        public int GroupsCount { get; set; }
        public Center()
        {
            this.CenterID = null;
            this.CenterName = string.Empty;
            this.Address = string.Empty;
            this.City = string.Empty;
            this.Email = string.Empty;
            this.Phone = string.Empty;
            this.Manager = string.Empty;
            this.Capacity = 0;
            this.Facilities = string.Empty;
            this.CreatedAt = DateTime.MinValue;
            this.IsActive = false;

            _Mode = enMode.AddNew;
        }
        private Center(CenterDTO dto)
        {
            this.CenterID = dto.CenterID;
            this.CenterName = dto.CenterName;
            this.Address = dto.Address;
            this.City = dto.City;
            this.Email = dto.Email;
            this.Phone = dto.Phone;
            this.Manager = dto.Manager;
            this.Capacity = dto.Capacity;
            this.Facilities = dto.Facilities;
            this.CreatedAt = dto.CreatedAt;
            this.IsActive = dto.IsActive;
            this.TrainersCount = dto.TrainersCount;
            this.GroupsCount = dto.GroupsCount;

            _Mode = enMode.Update;
        }

        public static async Task<IReadOnlyList<Center>> GetAllCenters()
        {
            var centers = await clsCenters.LoadAllCenters();

            return centers
                .Select(c => new Center(c))
                .ToList()
                .AsReadOnly();
        }

        public static async Task<Center> Find(int centerID)
        {
            CenterDTO dto = await clsCenters.GetCenterByID(centerID);
            if (dto != null)
            {
                return new Center(dto);
            }
            return null;
        }
        private async Task<bool> _AddNewCenter()
        {
            int? newID = await clsCenters.AddNewCenter(
                this.CenterName,
                this.Address,
                this.City,
                this.Phone,
                this.Email,
                this.Manager,
                this.Capacity,
                this.Facilities,
                this.IsActive);

            if (newID.HasValue)
            {
                CenterID = newID.Value;
                _Mode = enMode.Update; // Set mode to Update after successful creation
                return true;
            }
            return false;
        }
        private async Task<bool> _UpdateCenter()
        {
            return await clsCenters.UpdateCenter(
                this.CenterID.Value,
                this.CenterName,
                this.Address,
                this.City,
                this.Phone,
                this.Email,
                this.Manager,
                this.Capacity,
                this.Facilities,
                this.IsActive);
        }

        /// <summary>
        /// Validates object state and saves to database (Insert or Update).
        /// </summary>
        public async Task<bool> Save(int userId)
        {
            // 1. Business Logic Validation
            if (string.IsNullOrWhiteSpace(CenterName))
                throw new ArgumentException("Center Name is required.");

            if (string.IsNullOrWhiteSpace(Address))
                throw new ArgumentException("Address is required.");

            if (Capacity < 0)
                throw new ArgumentException("Capacity cannot be negative.");


            if (_Mode == enMode.AddNew)
            {
                var result = await _AddNewCenter();
                if (result)
                {
                    await Task.Run(() => clsSystemLogData.LogAction(
                        userId,
                        "ADDNEW",
                        "Centers",
                        this.CenterID ?? 0,
                        "Added new Center record"));
                }

                return result;
            }
            else // enMode.Update
            {
                var result = await _UpdateCenter();
                if (result)
                {
                    await Task.Run(() => clsSystemLogData.LogAction(
                        userId,
                        "UPDATE",
                        "Centers",
                        this.CenterID ?? 0,
                        "Updated Center record"));
                }

                return result;
            }
        }

        public async Task<bool> Delete()
        {
            // FIX: Prevent crash if ID is null
            if (!this.CenterID.HasValue)
                throw new InvalidOperationException("Cannot delete: Center ID is not set.");

            return await clsCenters.DeleteCenter(this.CenterID.Value);
        }
    }
}
