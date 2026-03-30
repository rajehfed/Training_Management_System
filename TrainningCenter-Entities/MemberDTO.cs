using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter_Entities
{
    public class MemberDTO
    {
        public int StudentID { get; set; }
        public int GroupID { get; set; }

        // ✅ FIX: Keep only this one
        public int SubscriptionID { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public DateTime DateOfSubscription { get; set; }

        // ⚠️ NOTE: Make sure your SP selects this, or remove it if not needed
        public DateTime SubscriptionEndDate { get; set; }

        public bool IsActive { get; set; }
        public string Status { get; set; }

        public decimal Amount { get; set; }

        // Nullables are correct ✅
        public DateTime? CompletionDate { get; set; }
        public decimal? Grade { get; set; }
    }
}
