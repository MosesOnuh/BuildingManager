using System;

namespace BuildingManager.Models.Entities
{
    public class Activity
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        //Enum showing if the task is pending, accepted or done;
        //public string Status { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        //Enum 1 for pre-construction, 2 for construction, 3 for Post-construction
        //public string ProjectPhase { get; set; }
        public int ProjectPhase { get; set; }
        public string? FileName { get; set; }
        public string? StorageFileName { get; set; }
        public string? FileExtension { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualStartDate { get; set; } = null;
        public DateTime? ActualEndDate { get; set; } = null;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
