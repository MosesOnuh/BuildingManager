using BuildingManager.Models.Entities;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Repository
{
    public interface IProjectRepository
    {
        Task CreateProject(Project project);
    }
}
