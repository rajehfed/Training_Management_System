using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrainningCenter_DataAccessLayer;
using TrainningCenter_Entities;

namespace TrainingCenter_BusinessLayer
{
    public class Activity
    {
        public enum enMode { AddNew = 0, Udpate = 1 }
        private enMode _Mode = enMode.AddNew;

        public int? ActivityID { get; set; }
        public int? GroupID { get; set; }
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public int? CreatedByUserID { get; set; }
        public string Username { get; set; }

        public User CreatedByUser;
        public Activity() {
            this.ActivityID = null;
            this.GroupID = null;
            this.ActivityType = string.Empty;
            this.Description = string.Empty;
            this.Timestamp = DateTime.MinValue;
            this.CreatedByUserID = null;
            this.Username = string.Empty;
            this.CreatedByUser = null;

            _Mode = enMode.AddNew;
        }

        private Activity(ActivityDTO dto)
        {
            this.ActivityID = dto.ActivityID;
            this.GroupID = dto.GroupID;
            this.ActivityType = dto.ActivityType;
            this.Description = dto.Description;
            this.Timestamp = dto.Timestamp;
            this.CreatedByUserID = dto.CreatedByUserID;
            this.Username = dto.Username;

            _Mode = enMode.Udpate;
        }

        private static async Task<Activity> FromDTOAsync(ActivityDTO dto)
        {
            Activity activity = new Activity(dto);
            activity.CreatedByUser = await User.Find(activity.CreatedByUserID.Value);

            return activity;
        }

        public static async Task<List<Activity>> GetGroupActivities(int groupID, int top = 20)
        {
            // 1. Get the list of DTOs asynchronously from the DAL.
            var activitiesFromDB = await clsActivity.GetGroupRecentActivities(groupID, top);

            // 2. Use LINQ Select to transform the list of DTOs into a list of Tasks.
            //    The tasks are started immediately by the Select call.

            var tasks = activitiesFromDB
                .Select(activityDTO => FromDTOAsync(activityDTO))
                .ToList();

            // 3. Use Task.WhenAll to concurrently wait for all the transformation tasks to complete.
            //    The result of WhenAll is an array or list of the results (BLL Activity objects).
            Activity[] activitiesArray = await Task.WhenAll(tasks);

            return activitiesArray.ToList();
        }
        public static async Task<Activity> GetLastActivity(int groupID, int top = 20)
        {
            List<Activity> acts = await GetGroupActivities(groupID, top);

            return acts.FirstOrDefault();
        }
    }
}
