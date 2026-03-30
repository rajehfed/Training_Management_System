using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter_Entities
{
    public class TrainerDTO
    {
        public int TrainerID { get; set; }
        public int PersonID { get; set; }
        public string TrainerName { get; set; }
        public int ExperienceYears { get; set; }
        public string Qualifications { get; set; }
        public string Specialization { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public decimal Rating { get; set; }
        public string Biography { get; set; }
        public int CenterID { get; set; }
        public string Center { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PersonDTO CurrentPerson { get; set; }
    }
}
