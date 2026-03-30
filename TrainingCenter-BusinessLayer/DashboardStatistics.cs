using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer.Helpers;
using TrainningCenter_DataAccessLayer.Dashboard;
using TrainningCenter_Entities;

namespace TrainingCenter_BusinessLayer
{
    public class DashboardStatistics
    {
        public int TotalStudents { get; set; }
        public int ActiveGroups { get; set; }
        public int SessionsToday { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public decimal RevenueGrowthPercentage { get; set; }
        public decimal AttendanceRate { get; set; }
        public int NewStudentsThisMonth { get; set; }
        public decimal StudentGrowthPercentage { get; set; }

        public DashboardStatistics()
        {
            TotalStudents = 0;
            ActiveGroups = 0;
            SessionsToday = 0;
            RevenueThisMonth = 0;
            RevenueLastMonth = 0;
            RevenueGrowthPercentage = 0;
            AttendanceRate = 0;
            NewStudentsThisMonth = 0;
            StudentGrowthPercentage = 0;
        }

        public static async Task<DashboardStatistics> GetStatisticsAsync()
        {
            var statistics = new DashboardStatistics();

            try
            {
                // Get basic counts
                statistics.TotalStudents = await clsDashboardStatisticsData.GetTotalStudentsAsync();
                statistics.ActiveGroups = await clsDashboardStatisticsData.GetActiveGroupsAsync();
                statistics.SessionsToday = await clsDashboardStatisticsData.GetSessionsTodayAsync();
                statistics.AttendanceRate = await clsDashboardStatisticsData.GetAttendanceRateAsync();

                // Get revenue data
                statistics.RevenueThisMonth = await clsDashboardStatisticsData.GetRevenueThisMonthAsync();
                statistics.RevenueLastMonth = await clsDashboardStatisticsData.GetRevenueLastMonthAsync();

                if (statistics.RevenueLastMonth > 0)
                {
                    statistics.RevenueGrowthPercentage = Math.Round(
                        ((statistics.RevenueThisMonth - statistics.RevenueLastMonth) /
                            statistics.RevenueLastMonth) * 100,
                        1);
                }

                statistics.NewStudentsThisMonth = await clsDashboardStatisticsData.GetNewStudentsThisMonthAsync();

                int newStudentsLastMonth = await clsDashboardStatisticsData.GetNewStudentsLastMonthAsync();
                if (newStudentsLastMonth > 0)
                {
                    statistics.StudentGrowthPercentage = Math.Round(
                        ((statistics.NewStudentsThisMonth - newStudentsLastMonth) /
                            (decimal)newStudentsLastMonth) * 100,
                        1);
                }
            }
            catch(Exception ex)
            {
                EventLogger.LogError("An Error Occurred While Loading Today's Session Schedules", ex);
            }

            return statistics;
        }
        public static async Task<List<TodaySessionModelDTO>> GetScheduleStatistics()
        {
            List<TodaySessionModelDTO> dtos = new List<TodaySessionModelDTO>();

            try
            {
                dtos = await clsScheduleData.GetTodayScheduleAsync();
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"An Error Occured While load Todays Sessions Schedules", ex);
            }

            return dtos;
        }
        public static async Task<List<ActivityLogDTO>> GetActivityLogs()
        {
            var activityLogs = new List<ActivityLogDTO>();

            try
            {
                activityLogs = await clsDashboardStatisticsData.GetRecentActivities(4);
            }
            catch(Exception ex)
            {
                EventLogger.LogError($"An Error Occured While load Recent Activies", ex);
                throw;
            }

            return activityLogs;
        }
        public static async Task<List<UpcomingEventDTO>> GetUpcomingEvents()
        {
            var upcomingEvents = new List<UpcomingEventDTO>();

            try
            {
                upcomingEvents = await clsDashboardStatisticsData.GetUpcomingEvents();
            }
            catch(Exception ex)
            {
                EventLogger.LogError($"An Error Occured While load The Upcomming Events", ex);
                throw;
            }

            return upcomingEvents;
        }
    }
}
