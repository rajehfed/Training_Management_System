using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer.Helpers;
using TrainningCenter_DataAccessLayer;
using TrainningCenter_Entities;

namespace TrainingCenter_BusinessLayer
{
    public class Student
    {
        public enum enStatus { eDropped = 0, eTransferred = 1, eGraduated = 2, eSuspended = 3, eActive = 4 }
        private static readonly Dictionary<enStatus, string> _StatusMap = new Dictionary<enStatus, string> {
            { enStatus.eDropped, "Dropped"},
            { enStatus.eTransferred, "Transferred" },
            { enStatus.eGraduated, "Graduated" },
            { enStatus.eSuspended, "Suspended" },
            { enStatus.eActive, "Active" }
        };
        public enum enMode { eAddNew = 0, eUpdate = 1 }
        private enMode _Mode = enMode.eAddNew;
        public int? StudentID { get; set; }
        public int PersonID { get; set; }
        public string StudentName { get; set; }
        public string StudentNumber { get; set; }
        public DateTime AdmissionDate { get; set; }
        private enStatus _Status;
        public enStatus Status
        {
            get { return _Status; }
            set { _Status = value; }
        }
        public string StatusName
        {
            get
            {
                // Automatically returns the string based on the current Enum
                if (_StatusMap.ContainsKey(_Status))
                    return _StatusMap[_Status];

                return "Active";
            }
            set
            {
                // Automatically updates the Enum based on the string value
                // Find the key (Enum) where the value matches the input string
                var statusEntry = _StatusMap.FirstOrDefault(x => x.Value.Equals(value, StringComparison.OrdinalIgnoreCase));

                // If found (Key is not 0 or matches Dropped explicitly)
                if (!statusEntry.Equals(default(KeyValuePair<enStatus, string>)))
                {
                    _Status = statusEntry.Key;
                }
                else
                {
                    _Status = enStatus.eActive; // Default if string not found
                }
            }
        }
        public string EmergencyContact { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Person PersonInfo { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Student"/> class, setting the mode to <see cref="enMode.eAddNew"/>.
        /// </summary>
        public Student()
        {
            StudentID = null;
            PersonID = 0;
            StudentName = string.Empty;
            StudentNumber = string.Empty;
            AdmissionDate = DateTime.MinValue;
            Status = enStatus.eActive;
            EmergencyContact = string.Empty;
            IsActive = true;
            CreatedDate = DateTime.MinValue;
            UpdatedAt = null;

            _Mode = enMode.eAddNew;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Student"/> class using a Data Transfer Object (DTO), 
        /// setting the mode to <see cref="enMode.eUpdate"/>. This constructor is private.
        /// </summary>
        /// <param name="dto">The StudentDTO containing data retrieved from the Data Access Layer.</param>
        private Student(StudentDTO dto)
        {
            StudentID = dto.StudentID;
            PersonID = dto.PersonID;
            StudentName = dto.StudentName;
            StudentNumber = dto.StudentNo;
            AdmissionDate = dto.AdmissionDate;
            StatusName = dto.StudentStatus;
            EmergencyContact = dto.EmergencyContact;
            IsActive = dto.IsActive;
            CreatedDate = dto.CreatedAt;
            UpdatedAt = dto.LastUpdatedAt;

            _Mode = enMode.eUpdate;
        }
        /// <summary>
        /// Factory method to convert a Student Data Transfer Object (DTO) into a business <see cref="Student"/> object,
        /// asynchronously loading the associated <see cref="Person"/> information.
        /// </summary>
        /// <param name="dto">The StudentDTO to convert.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains the fully loaded <see cref="Student"/> object.</returns>
        public static async Task<Student> FromDTOToStudent(StudentDTO dto)
        {
            Student student = new Student(dto);

            student.PersonInfo = new Person
            {
                Id = dto.PersonID,
                FirstName = dto.CurrentPerson.FirstName,
                LastName = dto.CurrentPerson.LastName,
                NationalNo = dto.CurrentPerson.NationalNo,
                Nationality = dto.CurrentPerson.Nationality,
                DateOfBirth = dto.CurrentPerson.DateOfBirth ?? DateTime.MinValue,
                Gender = dto.CurrentPerson.Gender == "M" ? Person.enGender.Male : Person.enGender.Female,
                PhoneNumber = dto.CurrentPerson.Phone,
                Email = dto.CurrentPerson.Email,
                Address = dto.CurrentPerson.Address,
                ImagePath = dto.CurrentPerson.ImagePath,
                CreatedAt = dto.CurrentPerson.PersonCreatedAt,
                LastUpdatedAt = dto.CurrentPerson.PersonUpdatedAt,
                IsActive = dto.CurrentPerson.IsActivePerson
            };

            return student;
        }
        /// <summary>
        /// Asynchronously retrieves a single Student object by its unique ID.
        /// </summary>
        /// <param name="StudentID">The ID of the student to find.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result is the <see cref="Student"/> object, or null if not found.</returns>
        public static async Task<Student> Find(int StudentID)
        {
            StudentDTO dto = await clsStudent.GetStudentByID(StudentID);

            if (dto is null)
                return null;

            return await FromDTOToStudent(dto);
        }
        /// <summary>
        /// Asynchronously retrieves a list of all Student records, including their associated Person information.
        /// </summary>
        /// <returns>A Task that represents the asynchronous operation. The task result contains a list of <see cref="Student"/> objects.</returns>
        public static async Task<List<Student>> GetAllStudent()
        {
            List<StudentDTO> dtos = new List<StudentDTO>();

            try
            {
                dtos = await clsStudent.GetAllStudents();

                IEnumerable<Task<Student>> loadingTasks =
                    dtos.Select(dto => FromDTOToStudent(dto));

                return (await Task.WhenAll(loadingTasks)).ToList();
            }
            catch (Exception ex)
            {
                EventLogger.LogError("An Error was Occured!!", ex);
                throw new ApplicationException("An Error was Occured!!", ex);
            }
        }

        public static async Task<List<Student>> GetActiveStudents()
        {
            List<Student> allStudents = await GetAllStudent();
            return allStudents.Where(student => student.IsActive).ToList();
        }
        /// <summary>
        /// Private method to handle the business logic for adding a new student to the database.
        /// </summary>
        /// <returns>A Task that represents the asynchronous operation. The task result is true if the student was added successfully and received an ID; otherwise, false.</returns>
        private async Task<bool> _AddNewStudent()
        {
            try
            {
                this.StudentID = await clsStudent.AddNewStudent(
                this.PersonID,
                this.StudentNumber,
                this.AdmissionDate,
                this.StatusName,
                this.EmergencyContact);

                return !(this.StudentID is null);
            }
            catch (Exception ex)
            {
                EventLogger.LogWarning($"Failed To Add New Student!! \n{ex.Message}");
                throw new ApplicationException($"Failed To Add New Student!!", ex);
            }
        }
        /// <summary>
        /// Private method to handle the business logic for updating an existing student record in the database.
        /// </summary>
        /// <returns>A Task that represents the asynchronous operation. The task result is true if the update was successful; otherwise, false.</returns>
        private async Task<bool> _UpdateStudent()
        {
            try
            {
                this.UpdatedAt = DateTime.Now;
                bool isUpdated = await clsStudent.UpdateStudent(
                    this.StudentID.Value,
                    this.PersonID,
                    this.StudentNumber,
                    this.AdmissionDate,
                    this.StatusName,
                    this.EmergencyContact,
                    this.IsActive);

                if (isUpdated)
                    EventLogger.LogInfo($"Student ID {this.StudentID} updated Successfully!");
                else
                    EventLogger.LogWarning($"Updated User returned false for ID {this.StudentID}");

                return isUpdated;
            }
            catch (Exception ex)
            {
                EventLogger.LogWarning($"Failed To Update Student!! \n{ex.Message}");
                throw new ApplicationException($"Failed To Update Student!!", ex);
            }
        }
        /// <summary>
        /// Saves the current Student object's data to the database.
        /// Determines whether to perform an Add (if <see cref="StudentID"/> is null) or an Update based on the current <see cref="enMode"/>.
        /// </summary>
        /// <returns>A Task that represents the asynchronous operation. The task result is true if the save operation was successful; otherwise, false.</returns>
        public async Task<bool> Save()
        {
            switch (_Mode)
            {
                case enMode.eAddNew:
                    if (await _AddNewStudent())
                    {
                        _Mode = enMode.eUpdate;
                        return true;
                    }
                    else
                        return false;

                case enMode.eUpdate:
                    return await _UpdateStudent();
            }

            return false;
        }
        public async Task<bool> Delete()
        {
            try
            {
                bool isDeleted = await clsStudent.DeleteStudent(this.StudentID.Value, false);

                return isDeleted;
            }
            catch(Exception ex)
            {
                EventLogger.LogError($"Failed To Delete Student ID: {this.StudentID}", ex);
                throw new ApplicationException($"Failed To Delete Student ID: {this.StudentID}", ex);
            }
        }
    }
}
