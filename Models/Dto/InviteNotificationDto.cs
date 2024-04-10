using BuildingManager.Models.Entities;

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
}




