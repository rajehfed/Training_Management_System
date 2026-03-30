using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter_Entities
{
    public enum enStatus { stSuspended = 0, stCancelled = 1, stCompleted = 2, stActive = 3 }
    public class GroupDTO
    {
        // Core Table Fields (IDs)
        public int GroupID { get; set; }
        public int SpecializationID { get; set; }
        public int? CenterID { get; set; }
        public int? TrainerID { get; set; }

        public string GroupName { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxTrainees { get; set; }
        public int CurrentTrainees { get; set; }
        public string Room { get; set; }
        public string Schedule { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string SpecializationName { get; set; }
        public decimal BaseFee { get; set; }
        public string CenterName { get; set; }
        public string TrainerName { get; set; }
        public int ActiveMembers { get; set; }
        public string Description { get; set; }
    }

    public class GroupsPagedList
    {
        public List<GroupDTO> Groups { get; set; }
        public int TotalRecords { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}
