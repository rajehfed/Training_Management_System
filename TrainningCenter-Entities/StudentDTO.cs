using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter_Entities
{
    public class StudentDTO
    {
        public int StudentID { get; set; }
        public int PersonID { get; set; }
        public string StudentName { get; set; }
        public string StudentNo { get; set; }
        public DateTime AdmissionDate { get; set; }
        public string StudentStatus { get; set; }
        public string EmergencyContact { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool IsActive { get; set; }

        // NEW: Person fields
        public PersonDTO CurrentPerson { get; set; }
    }
}
