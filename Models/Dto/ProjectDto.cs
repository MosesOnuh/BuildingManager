using System;

namespace BuildingManager.Models.Dto
{
    public class ProjectDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //public DateTime CreatedAt { get; set; }
    }

    public class ProjectRequestDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class AddProjectMemberDto
    {
        public string ProjectId { get; set; }
        public string Email { get; set; }
        public int Role { get; set; }
    }

}
