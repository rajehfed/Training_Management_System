using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter_Entities
{
    public class ActivityLogDTO
    {
        public int LogId { get; set; }
        public string ActionType { get; set; }
        public string TableName { get; set; }
        public DateTime ActionDate { get; set; }
        public string Details { get; set; }
        public string PerformedBy { get; set; }
    }
}
