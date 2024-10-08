﻿
using BuildingManager.Helpers;
using BuildingManager.Models;
using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Repository
{
    public interface IActivityRepository
    {
        Task CreateActivity(Activity activity);
        Task<(int, int)> UpdateActivityApprovalStatus(ActivityStatusUpdateDto  model);
        Task<(int, int)> SendActivityForApproval(ActivityStatusUpdateDto model, string userId);
        Task<(int, int)> UpdateActivityToDone(ActivityStatusUpdateDto model, string userId);
        Task<(int, int)> UpdateActivityActualDates(ActivityActualDatesDto model, string userId);
        Task<(int, int)> UpdatePendingActivity(UpdateActivityDetailsDto model, string userId);
        Task<(int, int)> UpdateActivityPM(UpdateActivityPmDetailsDto model, string userId);
        Task<(int, int)> AddActivityFile(AddActivityFileDto model, string userId );
        Task<(int, int)> AddActivityFilePM(AddActivityFileDto model, string userId);
        Task<Activity> GetActivityOtherPro(string projId, string activityId, string userId);
        Task<ActivityAndMemberDto> GetActivityPM(string projId, string activityId);
        Task<(int, int)> DeleteActivity (string projId, string activityId, string userId);
        Task<(int, int)> DeleteActivityPM(string projId, string activityId, string userId);
        Task<(int, int)> RemoveActivityFileDetails(string projId, string activityId, string  userId);
        Task<(int, int)> RemoveActivityFileDetailsPM(string projId, string activityId, string userId);
        Task<(int, IList<ActivityDto>)> GetProjectPhaseActivitiesOtherPro (ProjectActivitiesReqDto model, string UserId);
        Task<(int, IList<ActivityAndMemberDto>)> GetProjectPhaseActivitiesPM(ProjectActivitiesReqDto model);
        Task<(int, IList<ActivityAndMemberDto>)> GetProjectActivities(ActivityDataReqDto model);
    }
}
