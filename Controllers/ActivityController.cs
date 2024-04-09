using BuildingManager.Contracts.Services;
using BuildingManager.Enums;
using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace BuildingManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IServiceManager _service;
        public ActivityController(IServiceManager service)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost("OtherPro/CreateActivity")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> CreateActivity([FromForm] ActivityRequestDto model)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                //throw new RestException(HttpStatusCode.Unauthorized, "No token provided in Authorization header");
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            //@Todo
            //validate model request
            //validate input
            //ensure the name of the files are in lower case when saving it

            // Check file size (max 30MB)
            if (model.File != null)
            {
                if (model.File.Length > 30 * 1024 * 1024)
                {
                    var err = new ErrorResponse<ActivityDto>()
                    {
                        Message = "File size cannot exceed 30MB"
                    };
                    return BadRequest(err);
                }
            }
            

            var (userRole, _) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                //throw new RestException(HttpStatusCode.Forbidden, "PM an Client are not allowed to create an activity.");
                //throw new RestException(HttpStatusCode.Forbidden, "User does not have sufficient permission");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //create acctivity in db and store file in cloud
            var response = await _service.ActivityService.CreateActivity(model, userId);

            return Ok(response);
        }


        [Authorize]
        [HttpPost("PM/ApproveActivity")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> ActivityApproval([FromBody] ActivityStatusUpdateDto model)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, _) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.PM)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                //throw new RestException(HttpStatusCode.Forbidden, "Only PM is allowed to approve or reject an activity.");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //PM can approve or reject any activity in the project which he is a PM
            //approve or reject a project using activity Id and projectID. This is to be sure that the activity being approved is for the right project
            var response = await _service.ActivityService.ActivityApproval(model);

            return Ok(response);
        }

       
        [Authorize]
        [HttpPost("OtherPro/UpdateActivityToDone")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> UpdateActivityToDone([FromBody] ActivityStatusUpdateDto model)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, _) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                //throw new RestException(HttpStatusCode.Forbidden, "Only PM is allowed to approve or reject an activity.");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var response = await _service.ActivityService.UpdateActivityToDone(model, userId);

            return Ok(response);
        }

        //use this endpoint to add project startdate and enddate and to edit the start date and end date
        [Authorize]
        [HttpPost("OtherPro/UpdateActivityActualDates")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> UpdateActivityActualDates([FromBody] ActivityActualDatesDto model)
        {
            //ensure user is other pro
            //validate userinput

            //input user actual start date and actual enddate for acitivity that is approved or if it is done
            //Note: actual start date and actual done date cannot be greater than todays date i.e. date it is being inputted.

            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, _) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                //throw new RestException(HttpStatusCode.Forbidden, "Only PM is allowed to approve or reject an activity.");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }


            var response = await _service.ActivityService.UpdateActivityActualDates(model, userId);

            return Ok(response);
        }


        [Authorize]
        [HttpPost("OtherPro/UpdatePendingActivityDetails")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> UpdateActivity([FromBody] UpdateActivityDetailsDto model)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, projectId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                //throw new RestException(HttpStatusCode.Forbidden, "Only OtherPro can update a pending activity.");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //OtherPro can only update his own pending or rejected activity
            var response = await _service.ActivityService.UpdatePendingActivity(model, userId);

            return Ok(response);
        }

        //otherpro can add a file to a pending  activity
        [Authorize]
        [HttpPost("OtherPro/AddActivityFile")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> AddPendingActivityFile([FromForm] AddActivityFileRequestDto model)
        {
            
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            //@Todo
            //validate input
            //ensure the name of the files are in lower case when saving it

            var (userRole, projectId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                //throw new RestException(HttpStatusCode.Forbidden, "Only OtherPro can update the file of an pending activity.");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //check if a file exist for the activity, if it doesn't then create a file for the user
            var response = await _service.ActivityService.UpdatePendingActivityFile(model, userId);

            return Ok(response);
        }

        //[Authorize]
        //[HttpPost("PM/ResendActivity")]
        //[ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        //public async Task<IActionResult> ResendRejectedActivity([FromForm] ActivityStatusUpdateDto model)
        //{
        //    if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
        //    {
        //        throw new RestException(HttpStatusCode.Unauthorized, "No token provided in Authorization header");
        //    }

        //    var userId = HttpContext.Items["UserId"] as string;

        //    var (userRole, projectId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
        //    if (userRole != Enums.UserRoles.OtherPro)
        //    {
        //        //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
        //        throw new RestException(HttpStatusCode.Forbidden, "Only OtherPro can resend a rejected activity.");
        //    }

        //    //OtherPro can resend any of his activity that was rejected
        //    var response = await _service.ActivityService.ResendRejectedActivity(model, userId);

        //    return Ok(response);
        //}


        //Delete pending activity
        [Authorize]
        [HttpDelete("OtherPro/DeleteActivity/{projectId}/{activityId}")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> DeleteActivity(string projectId, string activityId)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(projectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                //throw new RestException(HttpStatusCode.Forbidden, "Only OtherPro can delete a pending or rejected activity.");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //OtherPro can delete only pending activity
            var response = await _service.ActivityService.DeleteActivity (projId, activityId, userId);

            //@Todo: Delete activity storage file function 

            return Ok(response);
        }



        ///////////////////////// Check this out ///////////////////
        /// <summary>
        /// /
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="activityId"></param>
        /// <returns></returns>
        /// <exception cref="RestException"></exception>



        //@todo procedure to remove FileName, StorageFileName, FileExtension after deleting a file in the database
        //Delete pending activity
        [Authorize]
        [HttpDelete("OtherPro/DeletePendingActivityFile/{projectId}/{activityId}")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> DeletePendingActivityFile(string projectId, string activityId)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(projectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                //throw new RestException(HttpStatusCode.Forbidden, "Only OtherPro can delete a pending or rejected activity.");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //OtherPro can delete only pending activity
            var response = await _service.ActivityService.DeleteActivityFile(projId, activityId, userId);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("OtherPro/DownloadActivityFile")]
        //[ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> DownloadActivityFileOtherPro ([FromBody] DownloadActivityFileDto model) 
        {
            //model
            //activity id
            //project id
            //userid
            //get file name
            //get storageFileName

            //ensure user is otherpro

            //validate input
            //ensure the name of the files are in lower case when saving it 

            //check if file name, userId, projectid, activity id ---exist in database
            //if not throw exception
            //go to cloud and download file


            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                //throw new RestException(HttpStatusCode.Forbidden, "Only OtherPro can delete a pending or rejected activity.");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var file = await _service.ActivityService.DownloadActivityFileOtherPro(model, userId);
            return File(file.ResponseStream, file.Headers.ContentType);
        }

        [Authorize]
        [HttpGet("PM/DownloadActivityFile")]
        //[ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> DownloadActivityFilePM([FromBody] DownloadActivityFileDto model)
        {
            //model
            //activity id
            //project id
            //userid
            //get file name
            //get storageFileName

            //ensure that the user is a pm

            //validate input
            //ensure the name of the files are in lower case when saving it 

            //check if file name, projectid, activity id ---exist in database
            //if not throw exception
            //go to cloud and download file


            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.PM)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                //throw new RestException(HttpStatusCode.Forbidden, "Only OtherPro can delete a pending or rejected activity.");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var file = await _service.ActivityService.DownloadActivityFilePM(model);
            return File(file.ResponseStream, file.Headers.ContentType);
        }


        [Authorize]
        [HttpGet("OtherPro/Getactivity/{projectId}/{activityId}")]  //activity Id
        [ProducesResponseType(typeof(SuccessResponse<ProjectDto>), 200)]
        public async Task<IActionResult> GetActivityOtherPro (string projectId, string activityId)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;
            //var projectRole = _service.ProjectService.GetUserProjectRole(userId, id); // where ID is project ID

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(projectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var response = await _service.ActivityService.GetActivityOtherPro(projId, activityId, userId); //where id = ActivityId

            return Ok(response);
        }


        //To be corrected, the response output
        [Authorize]
        [HttpGet("PM/Getactivity/{projectId}/{activityId}")]  //activity Id
        [ProducesResponseType(typeof(SuccessResponse<ProjectDto>), 200)]
        public async Task<IActionResult> GetActivityPM (string projectId, string activityId)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityAndMemberDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;
            //var projectRole = _service.ProjectService.GetUserProjectRole(userId, id); // where ID is project ID

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(projectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.PM)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                var err = new ErrorResponse<ActivityAndMemberDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var response = await _service.ActivityService.GetActivityPM(projId, activityId); //where id = ActivityId

            return Ok(response);
        }


        //carry out pagination
        [Authorize]
        [HttpGet("OtherPro/GetProjectPhaseActivities")]
        public async Task<IActionResult> GetProjectPhaseActivitiesOtherPro ([FromBody] ActivitiesDtoPaged model)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var response = await _service.ActivityService.GetProjectPhaseActivitiesOtherPro(model, userId);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("PM/GetProjectPhaseActivities")]
        public async Task<IActionResult> GetProjectPhaseActivitiesPM([FromBody] ActivitiesDtoPaged model)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityAndMemberDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.PM)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                var err = new ErrorResponse<ActivityAndMemberDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var response = await _service.ActivityService.GetProjectPhaseActivitiesPM(model);
            return Ok(response);
        }

        //create activity                        --- done
        //@Todo: update pending activity info   ----- done
        //@Todo: approve or reject a project --done
        //add activity file                 --done
        //@Todo: update/add pending activity file  ----done
        //@Todo: delete activity            --done --in prog
        //@Todo: delete activity file   --d
        //@Todo: download activity file     --d
        //@Todo: change project status to done other pro  --done
        //@Todo: Add actualstart and actual Finish date --d
        //actual start date and actual done date cannot be greater than todays date i.e. date it is being inputted.
        //@Todo: edit actualstart and actual Finish date --d
        //PM get activities
        //OtherPro get my activities
        //PM get one activity by Id ---D
        //Otherpro get one activity by Id ----D
        //@todo procedure to remove FileName, StorageFileName, FileExtension after deleting a file in the database

        // create enum for activity status
        // validate all request
        // ensure that the number of words inputted are not exceeded
        // change NVAR(250) TO NVAR(200)
        //validate the file type that can be uploaded, only picture, word and pdf can only be uploaded
        //Change all enums in db to in. drop table and create new table. change entities to Int also
        //project member role was changed to int... change all queries
        //change projectmember db i.e role from varchar to int
        // change invitation userrole from varchar to int
        // change user emailverified from varchar to int
        //update columns returned in the procedure that returns ActivityAndMember and ActivityAndMember--ActivityAndMemberDto
    }
}
