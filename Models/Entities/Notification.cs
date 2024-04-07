using System;

namespace BuildingManager.Models.Entities
{
    public class InviteNotification
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string ProjectId { get; set; }
        public int UserRole { get; set; }

        // 1 for pending, 2 for accepted, 3 for rejected
        public string Status  {get; set;}
        //string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}