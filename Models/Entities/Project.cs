using System;

namespace BuildingManager.Models.Entities
{
    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }



    //public class ProjectUserRoleDetails
    //{
    //    public string ProjectId { get; set; }
    //    public string UserId { get; set; }
    //    public string Role { get; set; }
    //}

    public class ProjectMember
    {
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        //Enum 1 for PM, 2 for Other-Pro, 3 for Client
        public int Role { get; set; }
        //public string Profession { get; set; }
        public int Profession { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }



}
