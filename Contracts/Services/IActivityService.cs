using Amazon.S3.Model;
using BuildingManager.Helpers;
using BuildingManager.Models;
using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Services
{
    public interface IActivityService
    {
        Task<SuccessResponse<ActivityDto>> CreateActivity(ActivityRequestDto activity, string userId);
        Task<SuccessResponse<ActivityDto>> ActivityApproval(ActivityStatusUpdateDto model);
        Task<SuccessResponse<ActivityDto>> UpdatePendingActivity(UpdateActivityDetailsDto model, string userId);
        Task<SuccessResponse<ActivityDto>>  UpdatePendingActivityFile(AddActivityFileRequestDto model, string userId);
        Task<SuccessResponse<ActivityDto>> UpdateActivityToDone(ActivityStatusUpdateDto model, string userId);
        // Task<SuccessResponse<ActivityDto>> ResendRejectedActivity(ActivityStatusUpdateDto model, string userId);
        Task<SuccessResponse<ActivityDto>> DeleteActivity(string projId, string activityId, string userId);
        Task<SuccessResponse<ActivityDto>> DeleteActivityFile(ActivityFileDto model, string userId);
        Task<GetObjectResponse> DownloadActivityFileOtherPro(ActivityFileDto model, string userId);
        Task<GetObjectResponse> DownloadActivityFilePM(ActivityFileDto model);
        Task<SuccessResponse<ActivityDto>> GetActivityOtherPro(string projectId, string activityId, string userId);
        Task<SuccessResponse<ActivityAndMemberDto>> GetActivityPM(string projectId, string activityId);
        Task<SuccessResponse<ActivityDto>> UpdateActivityActualDates(ActivityActualDatesDto model, string userId);
        Task<PageResponse<IList<ActivityDto>>> GetProjectPhaseActivitiesOtherPro(ActivitiesDtoPaged model, string UserId);
        Task<PageResponse<IList<ActivityAndMemberDto>>> GetProjectPhaseActivitiesPM(ActivitiesDtoPaged model);
        Task<SuccessResponse<IList<ActivityAndMemberDto>>> GetProjectActivities(string projectId);
    }
}
