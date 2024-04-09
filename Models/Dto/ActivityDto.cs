using Microsoft.AspNetCore.Http;
using System;

namespace BuildingManager.Models.Dto
{
    //public class ActivityRequestDto
    //{
    //    public string Name { get; set; }
    //    public string Description { get; set; }
    //    //Enum 1 for pre-construction, 2 for construction, 3 for Post-construction
    //    public string ProjectPhase { get; set; }
    //    public string? FileName { get; set; }
    //    public string? CloudFileName { get; set; }
    //    public string? FileType { get; set; }
    //    public DateTime StartDate { get; set; }
    //    public DateTime EndDate { get; set; }
    //    public DateTime? ActualStartDate { get; set; }
    //    public DateTime? ActualEndDate { get; set; }
    //}


    public class ActivityRequestDto
    {
        public string ProjectId { get; set; }
       // public string UserId { get; set; }
        public string Name { get; set; }
       // public int Status { get; set; }
        public string Description { get; set; }
        //Enum 1 for pre-construction, 2 for construction, 3 for Post-construction
        public int ProjectPhase { get; set; }
        public IFormFile? File { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //public DateTime? ActualStartDate { get; set; }
        //public DateTime? ActualEndDate { get; set; }
    }

    //might delete
    public class ActivityDto
    {

        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        //Enum 1 for pre-construction, 2 for construction, 3 for Post-construction
        public int ProjectPhase { get; set; }
        public string? FileName { get; set; }
        public string? StorageFileName { get; set; }
        //public string? FileType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        //public DateTime UpdatedAt { get; set; }
    }

    public class ActivityStatusUpdateDto
    {
        public string ActivityId { get; set; }
        public string ProjectId { get; set; }
       
        //Approval action changes the status of the project. When a project is created it has a default status of 1 which is pending
        //Pm can change it to 2 to approve or 3 to reject the request.
        //The OtherPro can then change it to 4 for done
        //other pro can delete this is only applicable if the project is still pending which is 1 or 3 i.e. pending or rejected
        public int StatusAction { get; set; }
    }

    public class ActivityActualDatesDto
    {
        public string ActivityId { get; set; }
        public string ProjectId { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
    }


    //Dto if OtherPro wants to update the details of a pending project
    public class UpdateActivityDetailsDto
    {
        public string ActivityId { get; set; }
        public string ProjectId { get; set; }
        // public string UserId { get; set; }
        public string Name { get; set; }
        //public int Status { get; set; }
        public string Description { get; set; }
        //Enum 1 for pre-construction, 2 for construction, 3 for Post-construction
        public int ProjectPhase { get; set; }
        // public IFormFile file { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //public DateTime? ActualStartDate { get; set; }
        //public DateTime? ActualEndDate { get; set; }
    }

    public class AddActivityFileRequestDto
    {
        public string ProjectId { get; set; }
        public string ActivityId { get; set; }
        public IFormFile File { get; set; }
    }

    public class AddActivityFileDto
    {
        public string ProjectId { get; set; }
        public string ActivityId { get; set; }
        public string? FileName { get; set; }
        public string? StorageFileName { get; set; }
        public string? FileExtension { get; set; }
       // public DateTime TimeStamp { get; set; }
    }

    public class ActivityFileDto
    {
        public string ProjectId { get; set; }
        public string ActivityId { get; set; }
        public string FileName { get; set; }
    }

    public class ActivitiesDtoPaged
    {
        public string ProjectId { get; set; }
        //public string UserId { get; set; }
        public int ProjectPhase { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    //  u.FirstName, u.LastName,u.Id, pm.ProjectId, pm.Role, pm.Profession,
    //a.Name AS ActivityName, a.Status, a.Description, a.ProjectPhase, a.FileName,
    //a.StorageFileName, a.FileExtension, a.StartDate, a.EndDate, a.ActualStartDate, a.ActualEndDate,
    //a.CreatedAt

    public class ActivityAndMemberDto
    {
        public string UserId { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Role { get; set; }
        public int Profession { get; set; }

        public string ProjectId { get; set; }
        public string ActivityId { get; set; }       
        //public string UserId { get; set; }
        public string ActivityName { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        //Enum 1 for pre-construction, 2 for construction, 3 for Post-construction
        public int ProjectPhase { get; set; }
        public string? FileName { get; set; }
        public string? StorageFileName { get; set; }
        //public string? FileType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        //public DateTime UpdatedAt { get; set; }
    }
}