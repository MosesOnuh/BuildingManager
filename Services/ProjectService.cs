using BuildingManager.Contracts.Repository;
using BuildingManager.Contracts.Services;
using BuildingManager.Enums;
using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using BuildingManager.Utils.Logger;
using BuildingManager.Validators;
using System;
using System.Collections.Generic;
using System.Net;
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

        public async Task<SuccessResponse<ProjectDto>> CreateProject(ProjectRequestDto model)
        {
            _logger.LogInfo("Creating a new Project");
            var validate = new ProjectValidator();
            validate.ValidateProjectRequestDto(model);

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


        //public async Task<UserRoles> GetUserProjectRole(string projectId, string userId)
        public async Task<(UserRoles, string)> GetUserProjectRole(string projectId, string userId)
        {
            _logger.LogInfo("Getting User's Project role");
            IList<ProjectMember> roleDetails = await _repository.ProjectRepository.GetProjectMemberInfo(projectId);
            if (roleDetails.Count == 0)
            {
                throw new RestException(HttpStatusCode.NotFound, "Project with Id provided does not exist");
            }

            foreach (ProjectMember member in roleDetails)
            {
                if (member.UserId == userId)
                {
                    if (member.Role == 1) return (UserRoles.PM, member.ProjectId);
                    else if (member.Role == 2) return (UserRoles.OtherPro, member.ProjectId);
                    else if (member.Role == 3) return (UserRoles.Client, member.ProjectId);
                    else throw new Exception("Invalid user role");
                }
            }

            throw new RestException(HttpStatusCode.Forbidden, "User is not a member of this project");
        }

        public async Task<SuccessResponse<ProjectDto>> GetProject(string projectId)
        {
            var project = await _repository.ProjectRepository.GetProject(projectId);
            if (project == null)
            {
                throw new RestException(HttpStatusCode.NotFound, "Project does not exist.");
            };

            ProjectDto projectResponse = new()
            {
                Id = project.Id,
                Name = project.Name,
                Address = project.Address,
                State = project.State,
                Country = project.Country,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
            };

            return new SuccessResponse<ProjectDto>
            {
                Message = "Project successfully gotten",
                Data = projectResponse,
            };
        }

        public async Task<SuccessResponse<ProjectDto>> UpdateProject(ProjectDto model)
        {
            _logger.LogInfo("Updating a Project");
            var validate = new ProjectValidator();
            //validate.ValidateProjectRequestDto(model);

            await _repository.ProjectRepository.UpdateProject(model);
            return new SuccessResponse<ProjectDto>
            {
                Message = "Project successfully updated",
            };
        }

        public async Task<SuccessResponse<ProjectDto>> AddProjectMember(AddProjectMemberDto model)
        {
            //check if user exist
            //if user doesn't exist send email invite else continue
            // create user notification for invited user to join project (use user email and project id and role)
            _logger.LogInfo("Adding a member Project");

            bool emailExist = await _repository.UserRepository.CheckEmailExists(model.Email);
            if (!emailExist)
            {
                //Send mail using queing functiontionality telling the user to join building manager inorder to join the project he/she was invited to
            }

            InviteNotification newInvite = new()
            {
                Id = Guid.NewGuid().ToString(),
                Email = model.Email,
                ProjectId = model.ProjectId,
                UserRole = model.Role,
                CreatedAt = DateTime.Now,
            };

            await _repository.NotificationRepository.CreateInviteNotification(newInvite);
            return new SuccessResponse<ProjectDto>
            {
                Message = "Add member to project notification created",
            };
        }

        public async Task<PageResponse<IList<ProjectDto>>> GetProjectsPaged(string userId, int pageNumber, int pageSize)
        {
            var (totalCount, projects) = await _repository.ProjectRepository.GetProjectsPaged(userId, pageNumber, pageSize);

            //try-catch here
            int totalPages = (int)Math.Ceiling((double)totalCount / (double)pageSize);

            return new PageResponse<IList<ProjectDto>>()
            {
                Data = projects,
                Pagination = new Pagination()

                {
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    ActualDataSize = projects.Count,
                    TotalCount = totalCount
                }
            };
        }
    }
}



//public async Task<UserRoles> GetUserProjectRole(string projectId, string userId)
//{
//    _logger.LogInfo("Getting User's Project role");
//    IList<ProjectUserRoleDetails> roleDetails = await _repository.ProjectRepository.GetUserProjectRole(projectId);
//    if (roleDetails.Count == 0)
//    {
//        throw new RestException(HttpStatusCode.NotFound, "Project with Id provided does not exist");
//    }

//    UserRoles? actualRole = null;
//    int count = 0;
//    foreach (ProjectUserRoleDetails role in roleDetails)
//    {
//        if (role.UserId == userId)
//        {
//            count += 1;

//            if (role.Role == "1") actualRole = UserRoles.PM;
//            else if (role.Role == "2") actualRole = UserRoles.OtherPro;
//            else if (role.Role == "3") actualRole = UserRoles.Client;
//            else throw new Exception("Invalid user role");
//        }
//    }

//    if (count == 0)
//    {
//        throw new RestException(HttpStatusCode.Forbidden, "User is not a member of this project");
//    }
//    return actualRole;

//}