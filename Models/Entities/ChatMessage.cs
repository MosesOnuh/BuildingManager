using System.ComponentModel.DataAnnotations;
using System;
using BuildingManager.Enums;

namespace BuildingManager.Models.Entities
{
    public class ChatMessage
    {
        public string? Id { get; set; }
        [Required]
        public string GroupId { get; set; }
        public string UserId { get; set; }
        public string From { get; set; }  //Full Name of users
        public int Profession { get; set; }
        //public string Role { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
