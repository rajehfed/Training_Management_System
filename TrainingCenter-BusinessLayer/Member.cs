using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter_BusinessLayer.Helpers;
using TrainningCenter_DataAccessLayer;
using TrainningCenter_Entities;

namespace TrainingCenter_BusinessLayer
{
    public class Member
    {
        public enum enMode { AddNew = 0, Update = 1 }
        private enMode _Mode = enMode.AddNew;

        #region Properties
        public int StudentID { get; set; }
        public int? SubscriptionID { get; set; } // Primary Key
        public int GroupID { get; set; }

        public string MemberName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public DateTime DateOfSubscription { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } // Active, Dropped, Completed
        public DateTime? CompletionDate { get; set; }

        public decimal Amount { get; set; }
        public decimal? Grade { get; set; }
        #endregion

        #region Constructors
        public Member()
        {
            this.StudentID = 0;
            this.SubscriptionID = null;
            this.GroupID = 0;
            this.MemberName = string.Empty;
            this.Email = string.Empty;
            this.DateOfSubscription = DateTime.Now;
            this.IsActive = true;
            this.Status = "Active";
            this.CompletionDate = null;
            this.Amount = 0;
            this.PhoneNumber = string.Empty;
            this.Grade = null;

            _Mode = enMode.AddNew;
        }

        private Member(MemberDTO dto)
        {
            this.StudentID = dto.StudentID;
            this.SubscriptionID = dto.SubscriptionID;
            this.GroupID = dto.GroupID;
            this.MemberName = dto.FullName;
            this.Email = dto.Email;
            this.PhoneNumber = dto.PhoneNumber;
            this.DateOfSubscription = dto.DateOfSubscription;
            this.IsActive = dto.IsActive;
            this.Status = dto.Status;
            this.CompletionDate = dto.CompletionDate;
            this.Amount = dto.Amount;
            this.Grade = dto.Grade;

            _Mode = enMode.Update;
        }
        #endregion

        #region Factory Methods

        /// <summary>
        /// Retrieves a list of all members in a specific group and maps them to Member BOs.
        /// </summary>
        public static async Task<List<Member>> GetMembersList(int GroupID, bool OnlySubscribing = false)
        {
            var members = await clsMember.GetGroupMembers(GroupID, OnlySubscribing);

            // Filter in memory if needed, or pass 'OnlySubscribing' to DAL if supported
            if (OnlySubscribing)
            {
                members = members.Where(m => m.IsActive).ToList();
            }

            return members.Select(dto => new Member(dto)).ToList();
        }

        #endregion

        #region Actions (Save & Delete)

        /// <summary>
        /// Enrolls the student in the group.
        /// </summary>
        private async Task<(bool Success, string Message)> _AddMember(int CreatedByUserID = 0)
        {
            // Validation
            if (this.StudentID <= 0 || this.GroupID <= 0) return (false, "Invalid Student ID or Group ID.");

            try
            {
                // Assuming you have PaymentStatus logic (e.g. "Pending" or "Paid")
                string paymentStatus = this.Amount > 0 ? "Paid" : "Pending";

                var result = await clsSubscription.AddNewSubscription(
                    this.StudentID,
                    this.GroupID,
                    this.DateOfSubscription,
                    paymentStatus,
                    this.Status,
                    CreatedByUserID // CreatedByUserID (pass real ID if available)
                );

                if (result.IsSuccess)
                {
                    this.SubscriptionID = result.SubscriptionID;
                    return (true, result.Message ?? "Member added successfully.");
                }
                else
                {
                    // نتيجة فاشلة ولكن بدون استثناء
                    return (false, result.ErrorMessage ?? "Failed to add member.");
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error adding member (StudentID: {this.StudentID}, GroupID: {this.GroupID})";
                EventLogger.LogError(errorMessage, ex);

                // إرجاع رسالة ودية للمستخدم
                return (false, $"An error occurred while adding the member. Please try again. Details: {ex.Message}");
            }
        }

        private async Task<bool> _UpdateMember()
        {
            if (!this.SubscriptionID.HasValue)
            {
                throw new InvalidOperationException("Cannot update: Subscription ID is missing.");
            }

            try
            {
                string paymentStatus = this.Amount > 0 ? "Paid" : "Pending";

                // استخدام قيمة افتراضية إذا كان Grade null
                decimal? gradeToUpdate = this.Grade ?? 0;

                return await clsSubscription.UpdateSubscription(
                    this.SubscriptionID.Value,
                    paymentStatus,
                    this.Status,
                    gradeToUpdate.Value
                );
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"Error updating member (SubscriptionID: {this.SubscriptionID})", ex);
                throw new ApplicationException($"Failed to update member: {ex.Message}", ex);
            }
        }
        // Note: Updates usually happen via SP_UpdateSubscription (Grades/Payments)
        // You can implement _UpdateMember here if you want to edit them from the grid.

        public async Task<bool> Save(int CreatedByUserID = 0)
        {
            if (!Validate())
                return false;

            if (_Mode == enMode.AddNew)
            {
                var result = await _AddMember(CreatedByUserID);
                if (result.Success)
                {
                    _Mode = enMode.Update;
                    return true;
                }
                return false;
            }
            else
            {
                return await _UpdateMember();
            }
        }

        private bool Validate()
        {
            if (this.StudentID <= 0)
                return false;

            if (this.GroupID <= 0)
                return false;

            if (this.DateOfSubscription > DateTime.Now.AddMonths(1))
                return false;

            if (string.IsNullOrEmpty(this.Status))
                return false;

            return true;
        }
        /// <summary>
        /// Removes the student from the group (Un-enroll).
        /// </summary>
        public async Task<bool> Delete()
        {
            if (!this.SubscriptionID.HasValue)
                throw new InvalidOperationException("Cannot delete: Subscription ID is missing.");

            return await clsSubscription.RemoveMember(this.SubscriptionID.Value);
        }

        #endregion

        #region Checks Methods

        public static async Task<bool> IsEnrolled(int StudentID, int GroupID)
        {
            return await clsSubscription.IsStudentEnrolled(StudentID, GroupID);
        }

        #endregion
    }
}
