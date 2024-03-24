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
        public DateTime UpdatedAt { get; set; }
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
        public string Role { get; set; }
        public string Profession { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }


 //   CREATE TABLE ProjectMembers(
 //   ProjectId NVARCHAR(250)  NOT NULL,
 //   UserId NVARCHAR(250) NOT NULL,
 //   Role NVARCHAR(200) NULL,
	//Profession NVARCHAR(50) NOT NULL,
 //   CreatedAt DateTime NOT NULL,
	//UpdatedAt DateTime NOT NULL,

}
