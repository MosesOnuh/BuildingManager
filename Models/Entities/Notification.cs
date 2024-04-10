using System;

namespace BuildingManager.Models.Entities
{
    public class InviteNotification
    {
        public string Id { get; set; }
        public string PmId { get; set; }
        public string Email { get; set; }
        public string ProjectId { get; set; }
        public int Role { get; set; }
        public int Profession { get; set; }

        // 1 for pending, 2 for accepted, 3 for rejected
        public int Status  {get; set;}
        //string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}


