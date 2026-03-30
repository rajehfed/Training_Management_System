using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer.Helpers;
using TrainningCenter_DataAccessLayer;
using TrainningCenter_Entities;

namespace TrainingCenter_BusinessLayer
{
    public class Person
    {
        public enum enMode { AddNew = 0, Update = 1 }
        private enMode _Mode = enMode.AddNew;

        public enum enGender { Male = 'M', Female = 'F' }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalNo { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Nationality { get; set; }
        public enGender Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public bool IsActive { get; set; }

        public string FullName { get { return FirstName + " " + LastName; } }

        /// <summary>
        /// Add New Person Instance
        /// </summary>
        public Person()
        {
            this.Id = -1;
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.NationalNo = string.Empty;
            this.DateOfBirth = DateTime.MinValue;
            this.Nationality = string.Empty;
            this.Gender = enGender.Male;
            this.PhoneNumber = string.Empty;
            this.Email = string.Empty;
            this.Address = string.Empty;
            this.ImagePath = string.Empty;
            this.CreatedAt = DateTime.MinValue;
            this.LastUpdatedAt = DateTime.MinValue;
            this.IsActive = true;

            _Mode = enMode.AddNew;
        }

        /// <summary>
        /// Initialize Person from DataRow (for Update mode)
        /// </summary>
        private Person(DataRow personRow)
        {
            Id = (int)personRow["PersonID"];
            FirstName = personRow["FirstName"].ToString();
            LastName = personRow["LastName"].ToString();
            NationalNo = personRow["NationalNo"].ToString();
            DateOfBirth = (DateTime)personRow["DateOfBirth"];
            Nationality = personRow["Nationality"].ToString();
            Gender = (enGender)personRow["Gender"].ToString()[0]; // 'M' or 'F'
            PhoneNumber = personRow["PhoneNumber"].ToString();
            Email = personRow["Email"].ToString();
            Address = personRow["Address"].ToString();
            ImagePath = personRow["ImagePath"]?.ToString() ?? string.Empty;
            CreatedAt = (DateTime)personRow["CreatedAt"];
            LastUpdatedAt = (DateTime)personRow["UpdatedAt"];
            IsActive = (bool)personRow["IsActive"];

            _Mode = enMode.Update;
        }
        private Person(PersonDTO dto)
        {
            Id = dto.Id;
            FirstName = dto.FirstName;
            LastName = dto.LastName;
            NationalNo = dto.NationalNo;
            DateOfBirth = dto.DateOfBirth ?? DateTime.MinValue;
            Nationality = dto.Nationality;
            Gender = dto.Gender == "M" ? enGender.Male : enGender.Female;
            PhoneNumber = dto.Phone;
            Email = dto.Email;
            Address = dto.Address;
            ImagePath = dto.ImagePath ?? string.Empty;
            CreatedAt = dto.PersonCreatedAt;
            LastUpdatedAt = dto.PersonUpdatedAt;
            IsActive = dto.IsActivePerson;

            _Mode = enMode.Update;
        }
        public static Person CreateFromDTO(PersonDTO dto)
        {
            return new Person(dto);  // This should be made public/internal
        }

        /// <summary>
        /// Retrieves a person by their unique identifier.
        /// </summary>
        /// <param name="Id">The unique identifier of the person to retrieve.</param>
        /// <returns>A <see cref="Person"/> object representing the person with the specified identifier,  or <see
        /// langword="null"/> if no person with the given identifier is found.</returns>
        public static async Task<Person> Find(int Id)
        {
            DataRow personRow = await clsPerson.GetPersonByID(Id);

            if (personRow != null)
                return new Person(personRow);

            return null;
        }

        /// <summary>
        /// Retrieves a <see cref="Person"/> object based on the provided national number.
        /// </summary>
        /// <remarks>This method performs an asynchronous operation to retrieve the person data.  Ensure
        /// that the provided national number is valid and unique within the data source.</remarks>
        /// <param name="NationalNo">The national number of the person to retrieve. This value cannot be <see langword="null"/> or empty.</param>
        /// <returns>A <see cref="Person"/> object representing the person with the specified national number,  or <see
        /// langword="null"/> if no matching person is found.</returns>
        public static async Task<Person> FindByNationalNo(string NationalNo)
        {
            DataRow personRow = await clsPerson.GetPersonByNationalNo(NationalNo);

            if (personRow != null)
                return new Person(personRow);

            return null;
        }

        private async Task<bool> _AddNewPerson()
        {
            try
            {
                this.CreatedAt = DateTime.Now;
                this.LastUpdatedAt = DateTime.Now;
                this.Id = await clsPerson.AddNewPerson(
                    FirstName,
                    LastName,
                    NationalNo,
                    Nationality,
                    DateOfBirth,
                    (char)Gender,
                    PhoneNumber,
                    Email,
                    Address,
                    ImagePath
                );

                if (this.Id == -1) {
                    // إذا كان -1 يعني فشل في DAL، يمكننا رمي استثناء هنا
                    EventLogger.LogError("Failed to add new person in DAL.");
                    throw new Exception("Could not add new person.");
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"Error adding new person: {ex.Message}");
                // إعادة رمي الاستثناء أو رمي استثناء مخصص
                throw new Exception("An error occurred while adding the person.", ex);
            }

            return this.Id != -1;
        }
        private async Task<bool> _UpdatePerson()
        {
            try
            {
                this.LastUpdatedAt = DateTime.Now;
                bool isUpdated = await clsPerson.UpdatePerson(
                    this.Id,
                    this.FirstName,
                    this.LastName,
                    this.NationalNo,
                    this.Nationality,
                    this.DateOfBirth,
                    (char)this.Gender,
                    this.PhoneNumber,
                    this.Email,
                    this.Address,
                    string.IsNullOrEmpty(this.ImagePath) ? null : this.ImagePath
                );

                if (isUpdated)
                    EventLogger.LogInfo($"Person ID {this.Id} updated successfully.");
                else
                    EventLogger.LogWarning($"UpdatePerson returned false for ID {this.Id}.");

                return isUpdated;
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"Error updating person (ID {this.Id}): {ex}");
                //Console.WriteLine(ex);
                //MessageBox.Show(ex.ToString(), "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Saves the current person object to the database.
        /// The operation depends on the current Mode (AddNew or Update).
        /// </summary>
        /// <returns>True if the save operation was successful; otherwise, false.</returns>
        public async Task<bool> Save()
        {
            switch (_Mode)
            {
                case enMode.AddNew:
                    if (await _AddNewPerson())
                    {
                        _Mode = enMode.Update;
                        return true;
                    }
                    else
                        return false;

                case enMode.Update:
                    return await _UpdatePerson();
            }

            return false;
        }

        /// <summary>
        /// Retrieve a Data Table with <see cref="Person"/> objects
        /// </summary>
        /// <returns>Data Table with <see cref="Person"/> objects</returns>
        public static async Task<DataTable> GetAllPeople()
        {
            try {
                return await clsPerson.GetAllPeople();
            }
            catch(Exception ex) {
                EventLogger.LogError($"An Error Was Occured When Try to Retriving the people Data", ex);
                return null;
            }
        }

        /// <summary>
        /// this function Retrieve data from DB and set it into a list
        /// </summary>
        /// <returns>List of <see cref="Person"/> objects</returns>
        public static async Task<List<Person>> GetPeopleList()
        {
            DataTable dt = await clsPerson.GetAllPeople();
            List<Person> people = new List<Person>();

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    people.Add(new Person(row));
                }
            }
            return people;
        }

        public async Task<bool> DeletePerson()
        {
            if(_Mode == enMode.AddNew || this.Id == -1) {
                EventLogger.LogWarning($"Attempted to delete a person not yet saved or with invalid ID: {this.Id}.");
                return false;
            }

            return await clsPerson.DeletePerson(this.Id);
        }
    }
}
