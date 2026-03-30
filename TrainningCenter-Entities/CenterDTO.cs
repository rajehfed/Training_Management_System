using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter_Entities
{
    public class CenterDTO
    {
        public int CenterID { get; set; }
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
        public DateTime? UpdatedAt { get; set; }

        // ... existing properties ...
        public int TrainersCount { get; set; }
        public int GroupsCount { get; set; }
    }
}
