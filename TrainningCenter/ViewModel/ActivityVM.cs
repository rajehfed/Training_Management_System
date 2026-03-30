using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter.ViewModel
{
    public class ActivityVM
    {
        public int ActivityID { get; set; }
        public int GroupID { get; set; }
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public int CreatedByUserID { get; set; }
        public string Username { get; set; }
    }
}
