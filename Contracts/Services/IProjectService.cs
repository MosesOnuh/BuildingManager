using BuildingManager.Enums;
using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Services
{
    public interface IProjectService
    {
        Task<SuccessResponse<ProjectDto>> CreateProject(ProjectRequestDto model, string userId);
        Task<(UserRoles, string)> GetUserProjectRole(string projectId,string userId);
        Task<SuccessResponse<ProjectDto>> GetProject (string projectId);
        Task<SuccessResponse<ProjectDto>> UpdateProject(ProjectDto model);
        //Task<SuccessResponse<ProjectDto>> AddProjectMember(AddProjectMemberDto model);
        Task<SuccessResponse<ProjectDto>> CreateProjectMembershipNotification(InviteNotificationRequestDto model, string pmID);
        Task<PageResponse<IList<ProjectDto>>> GetProjectsPaged(string userId, int pageNumber, int pageSize);
        Task<SuccessResponse<ProjectDto>> ProjectInviteAcceptance(ProjectInviteStatusUpdateDto model, string userId);
    }
}

