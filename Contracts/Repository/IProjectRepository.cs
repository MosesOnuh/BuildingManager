using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Repository
{
    public interface IProjectRepository
    {
        Task CreateProject(Project project);
        Task CreateProjectMembership(ProjectMember memberShip);
        Task<IList<ProjectMember>> GetProjectMemberInfo(string projectId);
        public Task<IList<member>> GetProjectMembers(string projectId);
        Task<Project?> GetProject(string projectId);
        Task UpdateProject(ProjectDto model);
        Task<(int, IList<ProjectDto>)> GetProjectsPaged(string userId, int pageNumber, int pageSize);       
        Task<IList<ProjectMemberDetails>> GetProjMemberDetails(string projectId, string userId);
        Task<(int, int)> UpdateProjectUserAccessPm(ProjectAccessDto model);
        Task<(int, int)> UpdateProjectUserAccessOwner(ProjectAccessDto model);
    }
}
