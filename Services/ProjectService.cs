using BuildingManager.Contracts.Repository;
using BuildingManager.Contracts.Services;
using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using BuildingManager.Utils.Logger;
using BuildingManager.Validators;
using System;
using System.Threading.Tasks;

namespace BuildingManager.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryManager _repository;
        public ProjectService(
            ILoggerManager logger,
            IRepositoryManager repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<SuccessResponse<ProjectDto>> CreateProject(ProjectCreateDto model)
        {
            _logger.LogInfo("Creating a new Project");
            var validate = new ProjectValidator();
            validate.ValidateProjectCreateDto(model);

            Project project = new Project
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                Address = model.Address,
                State = model.State,
                Country = model.Country,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            await _repository.ProjectRepository.CreateProject(project);
            return new SuccessResponse<ProjectDto>
            {
                Message = "Project created successfully",
            };
        }
    }
}
