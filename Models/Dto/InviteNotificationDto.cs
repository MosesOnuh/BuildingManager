using BuildingManager.Models.Entities;
using System;

namespace BuildingManager.Models.Dto
{
    //public class InviteNotificationDto : InviteNotification
    //{
    //    //string Title { get; set; }
    //    public string Message { get; set; }
    //}

    public class InviteNotificationRequestDto
    {
        public string ProjectId { get; set; }
        public string Email { get; set; }
        // public int Role { get; set; }
        public int Profession { get; set; }

    }

    public class ProjectInviteStatusUpdateDto
    {
        public string InviteNotificationId { get; set; }
        public string ProjectId { get; set; }
        public int StatusAction { get; set; }
    }

    public class ProjectInvitesDtoPaged
    {
        public string ProjectId { get; set; }
        //public string UserId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class InviteResponseDto
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string PmId { get; set; }
        public string PmFirstName { get; set; }
        public string PmLastName { get; set; }
        public string PmEmail { get; set; }
        public string PmPhoneNum { get; set; }
        public int PmProjectProfession { get; set; }
        public string UserEmail { get; set; }

        public int UserRole { get; set; }
        public int UserProfession { get; set; }

        // 1 for pending, 2 for accepted, 3 for rejected
        public int Status { get; set; }
        //string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}




