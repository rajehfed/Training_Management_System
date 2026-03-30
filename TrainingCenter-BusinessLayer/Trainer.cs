using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer.Helpers;
using TrainningCenter_DataAccessLayer;
using TrainningCenter_Entities;

namespace TrainingCenter_BusinessLayer
{
    public class Trainer
    {
        public enum enMode { AddNew = 0, Update = 1 }
        private enMode _Mode = enMode.AddNew;

        #region Properties
        public int? TrainerID { get; set; }
        public int? PersonID { get; set; }
        public string TrainerName { get; set; }
        public int ExperienceYears { get; set; }
        public string Qualifications { get; set; }
        public string Specialization { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public decimal Rating { get; set; }
        public string Biography { get; set; }
        public int? CenterID { get; set; }
        public string CenterName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Person CurrentPerson { get; set; }
        public Center CurrentCenter { get; set; }
        #endregion

        #region Constructors
        public Trainer()
        {
            this.TrainerID = null;
            this.PersonID = null;
            this.TrainerName = string.Empty;
            this.ExperienceYears = 0;
            this.Qualifications = string.Empty;
            this.Specialization = string.Empty;
            this.HireDate = DateTime.MinValue;
            this.Salary = 0;
            this.Rating = 0;
            this.Biography = string.Empty;
            this.CenterID = null;
            this.CenterName = string.Empty;
            this.CreatedAt = DateTime.MinValue;
            this.IsActive = false;
            this.UpdatedAt = null;

            _Mode = enMode.AddNew;
        }
        private Trainer(TrainerDTO dto)
        {
            this.TrainerID = dto.TrainerID;
            this.PersonID = dto.PersonID;
            this.TrainerName = dto.TrainerName;
            this.ExperienceYears = dto.ExperienceYears;
            this.Qualifications = dto.Qualifications;
            this.Specialization = dto.Specialization;
            this.HireDate = dto.HireDate;
            this.Salary = dto.Salary;
            this.Rating = dto.Rating;
            this.Biography = dto.Biography;
            this.CenterID = dto.CenterID;
            this.CenterName = dto.Center;
            this.CreatedAt = dto.CreatedAt;
            this.IsActive = dto.IsActive;
            this.UpdatedAt = dto.UpdatedAt;

            _Mode = enMode.Update;
        }
        #endregion

        #region Factory Methods
        private static Trainer FromDTO(TrainerDTO dto)
        {
            Trainer trainer = new Trainer(dto);

            // Person data should be in DTO from JOIN
            if (dto.CurrentPerson != null)
            {
                trainer.CurrentPerson = Person.CreateFromDTO(dto.CurrentPerson);
            }

            // Center data should be in DTO from JOIN
            //if (dto.CurrentCenter != null)
            //{
            //    trainer.CurrentCenter = Center.CreateFromDTO(dto.CurrentCenter);
            //}

            return trainer;
        }
        private static async Task<Trainer> FromDTOAsync(TrainerDTO dto)
        {
            Trainer trainer = new Trainer(dto);
            if (trainer.PersonID.HasValue)
            {
                // ASSUMPTION: Person.Find(int id) is defined and returns Task<Person>
                trainer.CurrentPerson = Person.CreateFromDTO(dto.CurrentPerson);
            }

            // Un-comment and ensure Center.Find is defined.
            // if (trainer.CenterID.HasValue)
            // {
            //     trainer.CurrentCenter = await Center.Find(trainer.CenterID.Value);
            // }

            return trainer;
        }

        public static async Task<List<Trainer>> GetAllTrianers()
        {
            // This should return DTOs with Person AND Center data
            var dtos = await clsTrainer.GetAllTrainees();

            // Synchronous mapping - all data already loaded
            return dtos.Select(FromDTO).ToList();
        }

        public static async Task<Trainer> Find(int id)
        {
            // This should also JOIN Person and Center
            var dto = await clsTrainer.GetTrainerByID(id);

            if (dto != null)
            {
                return FromDTO(dto); // No async needed
            }

            return null;
        }
        #endregion

        #region CRUD Operations
        private async Task<bool> _AddNewTrainer()
        {
            if (!this.PersonID.HasValue)
                throw new InvalidOperationException("PersonID is required");
            try
            {
                this.TrainerID = await clsTrainer.AddNewTrainer(
                    this.PersonID.Value,
                    this.ExperienceYears,
                    this.Qualifications,
                    this.Specialization,
                    this.HireDate,
                    this.Salary,
                    this.Rating,
                    this.Biography,
                    this.CenterID
                );

                return this.TrainerID.HasValue;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error adding new trainer", ex);
            }
        }
        private async Task<bool> _UpdateTrainer()
        {
            try
            {
                return await clsTrainer.UpdateTrainer(
                    this.TrainerID.Value,
                    this.PersonID.Value,
                    this.ExperienceYears,
                    this.Qualifications,
                    this.Specialization,
                    this.HireDate,
                    this.Salary,
                    this.Rating,
                    this.Biography,
                    this.CenterID,
                    this.IsActive);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error updating trainer", ex);
            }
        }
        public async Task<bool> Save()
        {
            // Validation
            if (!PersonID.HasValue)
                throw new InvalidOperationException("PersonID is required");

            if (string.IsNullOrWhiteSpace(Qualifications))
                throw new ArgumentException("Qualifications cannot be empty");

            if (Rating < 0 || Rating > 5)
                throw new ArgumentException("Rating must be between 0 and 5");

            if (Salary < 0)
                throw new ArgumentException("Salary cannot be negative");

            if (ExperienceYears < 0)
                throw new ArgumentException("Experience years cannot be negative");

            if (HireDate > DateTime.Now)
                throw new ArgumentException("Hire date cannot be in the future");

            switch (_Mode)
            {
                case enMode.AddNew:
                    if (await this._AddNewTrainer())
                    {
                        _Mode = enMode.Update;
                        return true;
                    }
                    else
                        return false;

                case enMode.Update:
                    return await _UpdateTrainer();
            }

            return false;
        }
        public async Task<bool> Delete()
        {
            if (!this.TrainerID.HasValue)
                throw new InvalidOperationException("TrainerID is null. Cannot delete trainer.");

            try
            {
                if (this.TrainerID.HasValue)
                {
                    return await clsTrainer.DeleteTrainer(this.TrainerID.Value);
                }
                else
                {
                    throw new InvalidOperationException("TrainerID is null. Cannot delete trainer.");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error deleting trainer", ex);
            }
        }
        public static async Task<bool> IsTrainerExists(int PersonID)
        {
            try
            {
                return await clsTrainer.IsTrainerExists(PersonID);
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"Error While Try To Check the Existance of the Trainer of the person ID {PersonID}", ex);
                throw new Exception($"Error checking if Trainer exists for PersonID {PersonID}", ex);
            }
        }
        #endregion
    }
}
