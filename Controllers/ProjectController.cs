using BuildingManager.Contracts.Services;
using BuildingManager.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BuildingManager.Models.Dto;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System;
using BuildingManager.Contracts.Repository;
using BuildingManager.Models.Entities;
using System.Reflection;
using BuildingManager.Validators;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using BuildingManager.Enums;

namespace BuildingManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IServiceManager _service;
        private readonly IRepositoryManager _repository;
        private readonly GeneralValidator _generalValidator;
        public ProjectController(IServiceManager service, IRepositoryManager repository)
        {
            _service = service;
            _repository = repository;
            _generalValidator = new GeneralValidator();
        }

        [Authorize]
        [HttpPost("user/CreateProject")]
        [ProducesResponseType(typeof(SuccessResponse<ProjectDto>), 200)]
        public async Task<IActionResult> CreateProject ([FromBody] ProjectRequestDto model) 
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;
            var response = await _service.ProjectService.CreateProject(model, userId);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("user/GetProjMemberDetails/{id}")]
        [ProducesResponseType(typeof(SuccessResponse<ProjectMemberDetails>), 200)]
        public async Task<IActionResult> GetProjMemberDetails(string id)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            _generalValidator.ValidateString(id, "Id", 50);

            var userId = HttpContext.Items["UserId"] as string;

            var members = await _repository.ProjectRepository.GetProjMemberDetails(id, userId); // where ID is project ID
            
            if (members.Count == 0) {
                var err = new ErrorResponse<object> { Message = "Project with Id provided does not exist or user is not a member of the project" };
                return StatusCode((int)HttpStatusCode.NotFound, err);
            }

            var response = new SuccessResponse<ProjectMemberDetails>
            {
                Message = "member successfully gotten",
                Data = members[0]
            };


            return Ok(response);
        }

        [Authorize]
        [HttpGet("user/ProjectMembers/{id}")]
        [ProducesResponseType(typeof(SuccessResponse<IList<member>>), 200)]
        public async Task<IActionResult> ProjectMembers(string id)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            _generalValidator.ValidateString(id, "Id", 50);

            var userId = HttpContext.Items["UserId"] as string;

            var members = await _repository.ProjectRepository.GetProjectMembers(id); // where ID is project ID

            if (members.Count == 0)
            {
                var err = new ErrorResponse<object> { Message = "Project with Id provided does not exist or user is not a member of the project" };
                return StatusCode((int)HttpStatusCode.NotFound, err);
            }

            var response = new SuccessResponse<IList<member>>
            {
                Message = "member successfully gotten",
                Data = members
            };


            return Ok(response);
        }

        [Authorize]
        [HttpGet("user/GetProject/{id}")]
        [ProducesResponseType(typeof(SuccessResponse<ProjectDto>), 200)]
        public async Task<IActionResult> GetProject(string id)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            _generalValidator.ValidateString(id, "Id", 50);

            var userId = HttpContext.Items["UserId"] as string;

            var (_, projectId) = await _service.ProjectService.GetUserProjectRole(id, userId); // where ID is project ID

            var response = await _service.ProjectService.GetProject(projectId);

            return Ok(response);
        }

        [Authorize]
        [HttpGet("user/GetProjects/{pageNumber}/{pageSize}")]
        [ProducesResponseType(typeof(SuccessResponse<ProjectDto>), 200)]
        public async Task<IActionResult> GetProjects (int pageNumber, int pageSize) 
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            _generalValidator.ValidateInteger(pageNumber, "pageNumber", int.MaxValue);
            _generalValidator.ValidateInteger(pageSize, "pageSize", int.MaxValue);

            var userId = HttpContext.Items["UserId"] as string;
            var response = await _service.ProjectService.GetProjectsPaged(userId, pageNumber, pageSize);
            return Ok(response);
        }


        //Only PM can update a project
        [Authorize]
        [HttpPut("PM/UpdateProject")]
        [ProducesResponseType(typeof(SuccessResponse<ProjectDto>), 200)]
        public async Task<IActionResult> UpdateProject([FromBody] ProjectDto model)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, projectId) = await _service.ProjectService.GetUserProjectRole(model.Id, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.PM)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to update a project. User is not a PM");
                var err = new ErrorResponse<object> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }
            var response = await _service.ProjectService.UpdateProject(model);
            return Ok(response);
        }


        //Only PM can add members to project
        //Put check to ensure that A pm can not send an invite to himself
        [Authorize]
        [HttpPost("PM/ProjectMemberInvite")]
        [ProducesResponseType(typeof(SuccessResponse<ProjectDto>), 200)]
        public async Task<IActionResult> InviteUserToProject([FromBody] InviteNotificationRequestDto model)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;
            var (userRole, _) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.PM  && userRole != Enums.UserRoles.Client)
            {
                //_logger.LogError($"Error, only a PM (Project Manager) is allowed to add members to a project. User is not a PM");
                var err = new ErrorResponse<object> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //check if user exist
            //if user doesn't exist send email invite else continue
            // create user notification for invited user to join project (use user email and project id and role)
            //var response = await _service.ProjectService.AddProjectMember(model);
            var response = await _service.ProjectService.CreateProjectMembershipNotification(model,  userId);
            return Ok(response);
        }
        //when creating a notification if it already exist update the invite and change to pending

        //invite acceptance endpoint i.e. to accept or reject an invite 
        //when accepted add user to project member table
        // create two procedures 1 to accept and 2 to reject 
        // accept procedure will add the user to projectmember table--transaction
        //delete procedure will change status to 3 --> make necessary validation in db that only 3 can be passed. create a rep function that recieves any value
        //response message is based on input passed into the request

        [Authorize]
        [HttpPatch("PM/UserAccess")]
        [ProducesResponseType(typeof(SuccessResponse<ProjectDto>), 200)]
        public async Task<IActionResult> ProjectAccess([FromBody] ProjectAccessDto model)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;



            var (userRole,ownership, projId) = await _service.ProjectService.GetUserProjectRoleAndOwnership(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.PM && userRole != Enums.UserRoles.Client)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                var err = new ErrorResponse<object> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

           // var response = await _service.ProjectService.ProjectAccess(model, userId, (ProjectOwner)ownership );
            var response = await _service.ProjectService.ProjectAccess(model, (ProjectOwner)ownership);

            return Ok(response);
        }

        [Authorize]
        [HttpPatch("user/InviteAcceptance")]
        [ProducesResponseType(typeof(SuccessResponse<ProjectDto>), 200)]
        public async Task<IActionResult> ProjectInviteAcceptance([FromBody] ProjectInviteStatusUpdateDto model)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var response = await _service.ProjectService.ProjectInviteAcceptance(model, userId);

            return Ok(response);
        }

        [Authorize]
        [HttpGet("user/GetReceivedProjectInvites")]
        public async Task<IActionResult> GetReceivedProjectInvites()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;
            var response = await _service.ProjectService.GetReceivedProjectInvites(userId);
            return Ok(response);
        }


        [Authorize]
        [HttpGet("user/GetSentProjectInvites")]
        public async Task<IActionResult> GetSentProjectInvitesPaged(
            [FromQuery(Name = "pageNumber")] int pageNumber,
            [FromQuery(Name = "pageSize")] int pageSize
            )
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            _generalValidator.ValidateInteger(pageNumber, "pageNumber", int.MaxValue);
            _generalValidator.ValidateInteger(pageSize, "pageSize", int.MaxValue);

            var userId = HttpContext.Items["UserId"] as string;

            var invites = new SentProjInvitesDtoPaged
            {
                PmId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
          
            var response = await _service.ProjectService.GetSentProjectInvites(invites);
            return Ok(response);
        }






        //[Authorize]
        //[HttpGet("user/GetProjectInvites/{pageNumber:int}/{pageSize:int}")]
        //public async Task<IActionResult> GetProjectInvitesPaged (
        //    [FromQuery(Name = "projectId")] string projectId,
        //    //[FromQuery(Name = "projectPhase")] int projectPhase,
        //    [FromQuery(Name = "pageNumber")] int pageNumber,
        //    [FromQuery(Name = "pageSize")] int pageSize
        //    )
        //{
        //    if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
        //    {
        //        var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
        //        return Unauthorized(err);
        //    }

        //    var invites = new ProjectInvitesDtoPaged
        //    {
        //        ProjectId = projectId,
        //        PageNumber = pageNumber,
        //        PageSize = pageSize
        //    };

        //    var userId = HttpContext.Items["UserId"] as string;
        //    var response = await _service.ProjectService.GetProjectInvitesPaged(invites, userId);
        //    return Ok(response);
        //}




        //pm will have page in projects for sent invites


        //authorize
        //per page
        ////Get user Invite Notifications by email  // check invite status and get only the ones pending
        //public async Task IActionResult GetUserInvites(string id)
        //{
        //    //return OK();
        //}

        //pm get sent invites for project

        //authorize
        ////Get a user Invite counts by Email // check status // count those with pending status
        //public async Task IActionResult CountUserInvites(string id)
        //{
        //    //return OK();
        //}


        //authorize
        ////Get a user Invite Notification by notivication ID
        //public async Task IActionResult GetUserInvites(string id)
        //{
        //    //return OK();
        //}




        //get all :using project Id, join userroles and users table  using user id 
        ////Get project members
        //public async Task IActionResult GetProjectMembers(string id)
        //{
        //    //return OK();
        //}


        //remove
        //this isn't needed here as you can get user by Id. This will be in authentication controller or user controller
        ////Get a project member by ID. use userId to get the user 
        //public async Task IActionResult GetProjectMember(string id)
        //{
        //    //return OK();
        //}



        //authorize
        ////Get user Invite Notifications by email  // check invite status and get only the ones pending
        //public async Task IActionResult GetUserInvites(string id)
        //{
        //    //return OK();
        //}

        //authorize
        ////Get a user Invite counts by Email // check status // count those with pending status
        //public async Task IActionResult CountUserInvites(string id)
        //{
        //    //return OK();
        //}


        //authorize
        ////Get a user Invite Notification by notivication ID
        //public async Task IActionResult GetUserInvites(string id)
        //{
        //    //return OK();
        //}

        //Accept or decline a user invite notificattion

    }
}


//Note
//When a user accept a project invite, add the user to userRole Table
//ensure user input is validated

//@todo
//Add description as a field in projects table, to describe the project
//When creating a project, add the creators details in Projectmember table


//important
//create procedure for CreateProjectMembership(ProjectMember memberShip) : proc_CreateProjectMembership