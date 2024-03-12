using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Services
{
    public interface IProjectService
    {
        Task<SuccessResponse<ProjectDto>> CreateProject(ProjectCreateDto model);
    }
}
