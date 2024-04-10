using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Repository
{
    public interface IProjectRepository
    {
        Task CreateProject(Project project);
        Task<int> CreateProjectMembership(ProjectMember memberShip);
        Task<IList<ProjectMember>> GetProjectMemberInfo(string projectId);
        Task<Project?> GetProject(string projectId);
        Task UpdateProject(ProjectDto model);
        Task<(int, IList<ProjectDto>)> GetProjectsPaged(string userId, int pageNumber, int pageSize);
        Task<(int, int)> AcceptProjectInvite(ProjectInviteStatusUpdateDto model, string userId);
        Task<(int, int)> RejectProjectInvite(ProjectInviteStatusUpdateDto model, string userId);
    }
}
