using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainningCenter_Entities
{
    public class TodaySessionModelDTO
    {
        public int SessionID { get; set; }
        public string Time { get; set; }
        public string Title { get; set; }
        public string GroupName { get; set; }
        public string Location { get; set; }
    }
}
