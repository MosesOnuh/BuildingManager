using BuildingManager.Contracts.Repository;
using BuildingManager.Contracts.Services;
using BuildingManager.Enums;
using BuildingManager.Helpers;
using BuildingManager.Models;
using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using BuildingManager.Utils.Logger;
using BuildingManager.Validators;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
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

        public async Task<SuccessResponse<ProjectDto>> CreateProject(ProjectRequestDto model, string userId)
        {
            _logger.LogInfo("Creating a new Project");

            Project project = new Project
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                Address = model.Address,
                State = model.State,
                Country = model.Country,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CreatedAt = DateTime.Now,
            };

            await _repository.ProjectRepository.CreateProject(project);

            var memberShip = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = userId,
                Role = (int)UserRoles.PM,
                Profession = (int)UserProfession.ProjectManager,
                CreatedAt = DateTime.Now,
            };

            await _repository.ProjectRepository.CreateProjectMembership(memberShip);

            return new SuccessResponse<ProjectDto>
            {
                Message = "Project created successfully",
            };
        }

        public async Task<(UserRoles, string)> GetUserProjectRole(string projectId, string userId)
        {
            _logger.LogInfo("Getting User's Project role");

            IList<ProjectMember> roleDetails = await _repository.ProjectRepository.GetProjectMemberInfo(projectId);
            if (roleDetails.Count == 0)
            {
                throw new RestException(HttpStatusCode.NotFound, "Project with Id provided does not exist or user is not a member of the project");
            }

            foreach (ProjectMember member in roleDetails)
            {
                if (member.UserId == userId)
                {
                    //if ((ProjectUserAccess)member.UserAccess == ProjectUserAccess.Blocked)
                        if (member.UserAccess == (int)ProjectUserAccess.Blocked)
                        {
                        throw new RestException(HttpStatusCode.Forbidden, "User is blocked from having access to this project");
                    }

                    if (member.Role == 1) return (UserRoles.PM, member.ProjectId);
                    else if (member.Role == 2) return (UserRoles.OtherPro, member.ProjectId);
                    else if (member.Role == 3) return (UserRoles.Client, member.ProjectId);
                    else throw new Exception("Invalid user role");
                }
            }

            throw new RestException(HttpStatusCode.Forbidden, "User is not a member of this project");
        }

        public async Task<(UserRoles, int, string)> GetUserProjectRoleAndOwnership(string projectId, string userId)
        {
            _logger.LogInfo("Getting User's Project role");

            IList<ProjectMember> roleDetails = await _repository.ProjectRepository.GetProjectMemberInfo(projectId);
            if (roleDetails.Count == 0)
            {
                throw new RestException(HttpStatusCode.NotFound, "Project with Id provided does not exist or user is not a member of the project");
            }

            foreach (ProjectMember member in roleDetails)
            {
                if (member.UserId == userId)
                {
                    if (member.Role == 1) return (UserRoles.PM, member.ProjOwner, member.ProjectId);
                    else if (member.Role == 2) return (UserRoles.OtherPro, member.ProjOwner, member.ProjectId);
                    else if (member.Role == 3) return (UserRoles.Client, member.ProjOwner, member.ProjectId);
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

            await _repository.ProjectRepository.UpdateProject(model);
            return new SuccessResponse<ProjectDto>
            {
                Message = "Project successfully updated",
            };
        }

        //procedure should check if user already has an invite
        public async Task<SuccessResponse<ProjectDto>> CreateProjectMembershipNotification(InviteNotificationRequestDto model, string pmID)
        {
            //check if user exist
            //if user doesn't exist send email invite else continue
            // create user notification for invited user to join project (use user email and project id and role)
            _logger.LogInfo("Adding a member Project");

            //bool emailExist = await _repository.UserRepository.CheckEmailExists(model.Email);
            //if (!emailExist)
            //{
            //    //Send mail using queing functiontionality telling the user to join building manager inorder to join the project he/she was invited to
            //}

            if (model.Profession != (int)UserProfession.Client && 
                model.Profession != (int)UserProfession.ProjectManager &&
                model.Profession != (int)UserProfession.Architect &&
                model.Profession != (int)UserProfession.SiteEngineer &&
                model.Profession != (int)UserProfession.StructuralEngineer &&
                model.Profession != (int)UserProfession.MandE_Engineer &&
                model.Profession != (int)UserProfession.QuantitySurveyor &&
                model.Profession != (int)UserProfession.LandSurveyor &&
                model.Profession != (int)UserProfession.BrickLayer &&
                model.Profession != (int)UserProfession.IronBender &&
                model.Profession != (int)UserProfession.Technician &&
                model.Profession != (int)UserProfession.Labourer &&
                model.Profession != (int)UserProfession.Others
                )
            {
                _logger.LogError($"Error value for Profession is not accepted");
                throw new RestException(HttpStatusCode.BadRequest, "Error value for Profession is not accepted");
            }

            int userRole;
            if (model.Profession == (int)UserProfession.Client)
            {
                userRole = (int)UserRoles.Client;
            }else if (model.Profession == (int)UserProfession.ProjectManager)
            {
                userRole = (int)UserRoles.PM;
            }else
            {
                userRole = (int)UserRoles.OtherPro;
            }

            InviteNotification newInvite = new()
            {
                Id = Guid.NewGuid().ToString(),
                PmId = pmID,
                Email = model.Email,
                ProjectId = model.ProjectId,
                Profession = model.Profession,
                Role = userRole ,
                Status = (int)InviteNotificationStatus.Pending,
                CreatedAt = DateTime.Now,
            };

            var returnNum = await _repository.NotificationRepository.CreateInviteNotification(newInvite);
            if (returnNum == 0)
            {
                _logger.LogError($"Error user does Not have an Account");
                throw new RestException(HttpStatusCode.NotFound, "Error user does not have an account");
            }

            if(returnNum == 1)
            {
                _logger.LogError($"Error user is already a member of this project");
                throw new RestException(HttpStatusCode.Conflict, "Error user is already a member of this project");
            }

            if (returnNum == 2)
            {
                _logger.LogError($"Error user has already been invited to join this project and is yet to accept invitation");
                throw new RestException(HttpStatusCode.Conflict, "Error user has already been invited to join this project and is yet to accept invitation");
            }
            return new SuccessResponse<ProjectDto>
            {
                Message = "Project invite successfully sent to user",
            };
        }

        public async Task<PageResponse<IList<ProjectDto>>> GetProjectsPaged(string userId, int pageNumber, int pageSize)
        {
            var (totalCount, projects) = await _repository.ProjectRepository.GetProjectsPaged(userId, pageNumber, pageSize);

            try 
            {
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
            } catch(Exception ex)
            {
                _logger.LogError($"Error getting projects in the service layer {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting projects");
            }
            
        }

        //ProjectAccess(model, userId, userRole)
        public async Task<SuccessResponse<ProjectDto>> ProjectAccess(ProjectAccessDto model, Enums.ProjectOwner ownership)
        {      
            if (model.StatusAction != (int)ProjectUserAccess.Allowed && model.StatusAction != (int)ProjectUserAccess.Blocked)
            {
                throw new RestException(HttpStatusCode.BadRequest, "Error StatusAction can only be to Allow or Block a User to a Project");
            }

            if (ownership == ProjectOwner.NotOwner) { 
            
                var (rowsUpdated, returnNum) = await _repository.ProjectRepository.UpdateProjectUserAccessPm(model);
                if (rowsUpdated == 0 && returnNum == 0)
                {
                    _logger.LogError($"Error occurred when updating project user access status. The required project may not exist, check UserId and ProjectId provided");
                    throw new RestException(HttpStatusCode.NotFound, "Error failed to update project user access status");
                }

                if (rowsUpdated == 0 && returnNum == 1)
                {
                    _logger.LogError($"Error attempted to change user project access of an project manager or client; Insufficient Permission");
                    throw new Exception("Error attempted to change user project access of an project manager or client; Insufficient Permission");
                }

                if (rowsUpdated == 0)
                {
                    _logger.LogError($"Error failed to change project access");
                    throw new Exception("Error failed to change project access");
                }


                return new SuccessResponse<ProjectDto>
                {
                    Message = "User Project Access Updated successfully",
                };
            }

             var (rowsUpdated1, returnNum1) = await _repository.ProjectRepository.UpdateProjectUserAccessOwner(model);
            if (rowsUpdated1 == 0 && returnNum1 == 0)
            {
                _logger.LogError($"Error occurred when updating project user access status. The required project may not exist, check UserId and ProjectId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update project user access status");
            }

            if (rowsUpdated1 == 0)
            {
                _logger.LogError($"Error failed to change project access");
                throw new Exception("Error failed to change project access");
            }


            return new SuccessResponse<ProjectDto>
            {
                Message = "User Project Access Updated successfully",
            };
        }

        public async Task<SuccessResponse<ProjectDto>> ProjectInviteAcceptance(ProjectInviteStatusUpdateDto model, string userId)
        {
            //write procedure to check the status of the activity and if the status is one then 
            // the status can be updated to 2 or 3

            //validate the StatusAction and ensure that it is either 2 or 3
            if (model.StatusAction != (int)InviteNotificationStatus.Accepted && model.StatusAction != (int)InviteNotificationStatus.Rejected)
            {
                throw new RestException(HttpStatusCode.BadRequest, "Error StatusAction can only be to approve or reject an activity");
            }

            if (model.StatusAction == (int)InviteNotificationStatus.Accepted)
            {

                var (success, returnNum) = await _repository.NotificationRepository.AcceptProjectInvite(model, userId);
                if (success == 0 && returnNum == 1)
                {
                    _logger.LogError($"Error occurred when accepting project invite. The required invite notification may not exist, check parameters passed into the query");
                    throw new RestException(HttpStatusCode.NotFound, "Error failed to accept Project Invite. Notification Invite not found");
                }

                if (success == 0 && returnNum == 2)
                {
                    _logger.LogError($"Error the project invite has already been accepted or rejected");
                    throw new Exception("Error the project invite has already been accepted or rejected");
                }

                if (success != 1 && returnNum != 0)
                {
                    _logger.LogError($"Error failed to accept Project Invite");
                    throw new Exception("Error failed to accept Project Invite");
                }

                return new SuccessResponse<ProjectDto>
                {
                    Message = "Successfully accepted request to join project",
                };
            }

            var (rowsUpdated, returnedNum) = await _repository.NotificationRepository.RejectProjectInvite(model, userId);
            if (rowsUpdated == 0 && returnedNum == 0)
            {
                _logger.LogError($"Error occurred when rejecting project invite. The required invite notification may not exist, check parameters passed into the query");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to accept Project Invite. Notification Invite not found");
            }

            if (rowsUpdated == 0 && returnedNum == 1)
            {
                _logger.LogError($"Error the project invite has already been accepted or rejected");
                throw new Exception("Error the project invite has already been accepted or rejected");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to accept Project Invite");
                throw new Exception("Error failed to accept Project Invite");
            }

            return new SuccessResponse<ProjectDto>
            {
                Message = "Successfully rejected request to join project",
            };
        }

        public async Task<IList<ReceivedInviteRespDto>> GetReceivedProjectInvites( string userId)
        {
            var receivedInvites = await _repository.NotificationRepository.GetReceivedProjectInvites(userId);

            return receivedInvites;          
        }

        public async Task<PageResponse<IList<SentInviteRespDto>>> GetSentProjectInvites(SentProjInvitesDtoPaged model)
        {
            var (totalCount, invites) = await _repository.NotificationRepository.GetSentProjectInvites(model);
            try
            {

                int totalPages = (int)Math.Ceiling((double)totalCount / (double)model.PageSize);

                return new PageResponse<IList<SentInviteRespDto>>()
                {
                    Data = invites,
                    Pagination = new Pagination()

                    {
                        TotalPages = totalPages,
                        PageSize = model.PageSize,
                        ActualDataSize = invites.Count,
                        TotalCount = totalCount
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting sent invites in the service layer {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting sent project invites");
            }
        }



        //public async Task<PageResponse<IList<InviteResponseDto>>> GetProjectInvitesPaged(ProjectInvitesDtoPaged model, string userId)
        //{
        //    var (totalCount, invites) = await _repository.NotificationRepository.GetProjectInvites(model, userId);
        //    try 
        //    {

        //        int totalPages = (int)Math.Ceiling((double)totalCount / (double)model.PageSize);

        //        return new PageResponse<IList<InviteResponseDto>>()
        //        {
        //            Data = invites,
        //            Pagination = new Pagination()

        //            {
        //                TotalPages = totalPages,
        //                PageSize = model.PageSize,
        //                ActualDataSize = invites.Count,
        //                TotalCount = totalCount
        //            }
        //        };
        //    } catch (Exception ex) {
        //        _logger.LogError($"Error getting invites in the service layer {ex.StackTrace} {ex.Message}");
        //        throw new Exception("Error getting project invites");
        //    }  
        //}
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