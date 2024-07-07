using BuildingManager.Contracts.Services;
using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using BuildingManager.Utils.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace BuildingManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentRequestController : ControllerBase
    {
        private readonly IServiceManager _service;
        private readonly ILoggerManager _logger;

        public PaymentRequestController(IServiceManager service, ILoggerManager logger)
        {
            _service = service;
            _logger = logger;
        }

        //[Authorize]
        //[HttpPost("OtherPro")]
        //[ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 201)]
        ////public async Task<IActionResult> PaymentRequest([FromForm] PaymentRequestReqDto model)
        //     public async Task<IActionResult> PaymentRequest([FromForm] PaymentRequestReqDto model)
        //{
        //    if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
        //    {
        //        _logger.LogError($"Error, no token provided in Authorization header ");
        //        var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
        //        return Unauthorized(err);
        //    }

        //    var userId = HttpContext.Items["UserId"] as string;

        //    //@Todo
        //    //validate model request
        //    //validate input
        //    //ensure the name of the files are in lower case when saving it

        //    // Check file size (max 30MB)
        //    if (model.File != null)
        //    {
        //        if (model.File.Length > 30 * 1024 * 1024)
        //        {
        //            var err = new ErrorResponse<object>()
        //            {
        //                Message = "File size cannot exceed 30MB"
        //            };
        //            return BadRequest(err);
        //        }
        //    }


        //    var (userRole, _) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
        //    if (userRole != Enums.UserRoles.OtherPro)
        //    {
        //        var err = new ErrorResponse<object> { Message = "User does not have sufficient permission" };
        //        return StatusCode((int)HttpStatusCode.Forbidden, err);
        //    }

        //    //create payment request in db and store file in cloud
        //    var response = await _service.PaymentRequestService.CreatePaymentRequest(model, userId);
        //    return StatusCode((int)HttpStatusCode.Created, response);
        //    //return Ok(response);
        //}


        [Authorize]
        [HttpPost("OtherPro")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 201)]
        //public async Task<IActionResult> PaymentRequest([FromForm] PaymentRequestReqDto model)
        public async Task<IActionResult> PaymentRequest([FromBody] PaymentRequestReqDto model)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                _logger.LogError($"Error, no token provided in Authorization header ");
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            //@Todo
            //validate model request
            //validate input


            var (userRole, _) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                var err = new ErrorResponse<object> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //create payment request in db and store file in cloud
            var response = await _service.PaymentRequestService.CreatePaymentRequest(model, userId);

            return StatusCode((int)HttpStatusCode.Created, response);
            //return Ok(response);
        }

        [Authorize]
        [HttpGet("OtherPro")]
        [ProducesResponseType(typeof(PageResponse<IEnumerable<PaymentRequestDto>>), 200)]
        public async Task<IActionResult> GetPaymentRequestOtherPro
            (
            [FromQuery(Name = "projectId")] string projectId,
            [FromQuery(Name = "pageNumber")] int pageNumber,
            [FromQuery(Name = "pageSize")] int pageSize
            )
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                _logger.LogError($"Error, no token provided in Authorization header ");
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var model = new PaymentRequestDtoPaged
            {
                ProjectId = projectId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                _logger.LogError($"Error, only OtherPro have permission");
                var err = new ErrorResponse<object> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var response = await _service.PaymentRequestService.GetPaymentRequestsOtherPro(model, userId);
            return Ok(response);
        }


        [Authorize]
        [HttpGet("PM")]
        [ProducesResponseType(typeof(PageResponse<IEnumerable<PaymentRequestAndMemberDto>>), 200)]
        public async Task<IActionResult> GetPaymentRequestPM
           (
           [FromQuery(Name = "projectId")] string projectId,
           [FromQuery(Name = "pageNumber")] int pageNumber,
           [FromQuery(Name = "pageSize")] int pageSize
           )
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                _logger.LogError($"Error, no token provided in Authorization header ");
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var model = new PaymentRequestDtoPaged
            {
                ProjectId = projectId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.PM && userRole != Enums.UserRoles.Client)
            {
                _logger.LogError($"Error, only a  PM (Project Manager) and Client have permission");
                var err = new ErrorResponse<object> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var response = await _service.PaymentRequestService.GetPaymentRequestsPM(model);
            return Ok(response);
        }


        [Authorize]
        [HttpPatch("PM/Confirmation")]
        [ProducesResponseType(typeof(SuccessResponse<PaymentRequestDto>), 200)]
        public async Task<IActionResult> PaymentRequestConfirmation([FromBody] PaymentRequestStatusUpdateDto model)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                _logger.LogError($"Error, no token provided in Authorization header ");
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, _) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.PM)
            {
                _logger.LogError($"Error, only a  PM (Project Manager) is allowed to confirm or reject a Payment Request");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var response = await _service.PaymentRequestService.PaymentRequestConfirmation(model);
            return Ok(response);
        }

        [Authorize]
        [HttpPatch("OtherPro/SinglePendingPaymentRequest")]
        [ProducesResponseType(typeof(SuccessResponse<PaymentRequestDto>), 200)]
        public async Task<IActionResult> UpdateSinglePendingPaymentRequest([FromBody] UpdateSinglePaymentRequestDto model)
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
                _logger.LogError($"Error, only OtherPro have permission");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var newModel = new UpdatePaymentRequestDto
            {
                Id = model.Id,
                ProjectId = model.ProjectId,
                Name = model.Name,
                Description = model.Description,
                SumTotalAmount = model.SumTotalAmount,
            };

            //OtherPro can only update his own pending or rejected activity
            var response = await _service.PaymentRequestService.UpdatePendingPaymentRequest(newModel, userId);

            return Ok(response);
        }

        [Authorize]
        [HttpPatch("OtherPro/GroupPendingPaymentRequest")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> UpdateGroupPendingPaymentRequest([FromBody] UpdatePaymentRequestDto model)
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
                _logger.LogError($"Error, only OtherPro have permission");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //OtherPro can only update his own pending or rejected activity
            var response = await _service.PaymentRequestService.UpdatePendingPaymentRequest(model, userId);

            return Ok(response);
        }


        [Authorize]
        [HttpDelete("OtherPro/{projectId}/{paymentRequestId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeletePaymentRequest(string projectId, string paymentRequestId)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, _) = await _service.ProjectService.GetUserProjectRole(projectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                _logger.LogError($"Error, only OtherPro have permission");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //OtherPro can only update his own pending or rejected payment request
            await _service.PaymentRequestService.DeletePaymentRequest(projectId, paymentRequestId, userId);

            return NoContent();
        }

        //otherpro can add a file to a pending  activity
        //validate form request and ensure that a file is always passed to the request
        [Authorize]
        [HttpPatch("OtherPro/AddPaymentRequestFile")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> AddPendingPaymentRequestFile([FromForm] AddPaymentRequestFileReqDto model)
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
                _logger.LogError($"Error, only OtherPro have permission");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //check if a file exist for the payment request, if it doesn't then create a file for the user
            var response = await _service.PaymentRequestService.UpdatePendingPaymentRequestFile(model, userId);

            return Ok(response);
        }

        [Authorize]
        [HttpPatch("PM/AddConfirmationPaymentRequestFile")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> AddConfirmationPaymentRequestFile([FromForm] AddPaymentRequestFileReqDto model)
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
            if (userRole != Enums.UserRoles.PM)
            {
                _logger.LogError($"Error, only PM has permission");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //check if a file exist for the payment request, if it doesn't then create a file for the user
            var response = await _service.PaymentRequestService.UpdateConfirmationPaymentRequestFile(model);

            return Ok(response);
        }


        //Delete pending payment request file
        [Authorize]
        [HttpDelete("OtherPro/PendingPaymentRequestFile")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> DeletePendingPaymentRequestFile(
            [FromQuery(Name = "projectId")] string projectId,
            [FromQuery(Name = "paymentRequestId")] string paymentRequestId,
            [FromQuery(Name = "fileName")] string fileName
            )
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var model = new PaymentRequestFileDto
            {
                ProjectId = projectId,
                PaymentRequestId = paymentRequestId,
                FileName = fileName
            };

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                _logger.LogError($"Error, only OtherPro has permission");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //OtherPro can delete only pending payment request file
            var response = await _service.PaymentRequestService.DeletePaymentRequestFileOtherPro(model, userId);
            return Ok(response);
        }

        //Delete payment request file
        [Authorize]
        [HttpDelete("PM/ConfirmationPaymentRequestFile")]
        [ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> DeleteConfirmationPaymentRequestFile(
            [FromQuery(Name = "projectId")] string projectId,
            [FromQuery(Name = "paymentRequestId")] string paymentRequestId,
            [FromQuery(Name = "fileName")] string fileName
            )
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var model = new PaymentRequestFileDto
            {
                ProjectId = projectId,
                PaymentRequestId = paymentRequestId,
                FileName = fileName
            };

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.PM)
            {
                _logger.LogError($"Error, only PM has permission");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            //OtherPro can delete only pending payment request file
            var response = await _service.PaymentRequestService.DeleteConfirmationPaymentRequestFilePM(model);
            return Ok(response);
        }


        [Authorize]
        [HttpPatch("OtherPro/SendForConfirmation")]
        [ProducesResponseType(typeof(SuccessResponse<PaymentRequestDto>), 200)]
        public async Task<IActionResult> SendPaymentRequestForConfirmation([FromBody] PaymentRequestStatusUpdateDto model)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                _logger.LogError($"Error, no token provided in Authorization header ");
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var (userRole, _) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                _logger.LogError($"Error, only OtherPro has permission");
                var err = new ErrorResponse<ActivityDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var response = await _service.PaymentRequestService.SendPaymentRequestForConfirmation(model, userId);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("User/PayReqMonthlyData")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<PayReqMonthlyDataDto>>), 200)]
        public async Task<IActionResult> GetProfessionalsPayReqMonthlyData
           (
           [FromQuery(Name = "projectId")] string projectId,
           [FromQuery(Name = "year")] int year
           )
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                _logger.LogError($"Error, no token provided in Authorization header ");
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            await _service.ProjectService.GetUserProjectRole(projectId, userId); // where ID is project ID

            var response = await _service.PaymentRequestService.GetPayReqMonthlyData(projectId, year);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("User/PayReqWeeklyDataDto")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<PayReqMonthlyDataDto>>), 200)]
        public async Task<IActionResult> GetProfessionalsPayReqWeeklyData
           (
           [FromQuery(Name = "projectId")] string projectId,
           [FromQuery(Name = "year")] int year,
            [FromQuery(Name = "month")] int month
           )
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                _logger.LogError($"Error, no token provided in Authorization header ");
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            await _service.ProjectService.GetUserProjectRole(projectId, userId); // where ID is project ID

            var response = await _service.PaymentRequestService.GetPayReqWeeklyData(projectId, year, month);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("User/PayReqDailyDataDto")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<PayReqMonthlyDataDto>>), 200)]
        public async Task<IActionResult> GetProfessionalsPayReqDailyData
           (
           [FromQuery(Name = "projectId")] string projectId,
           [FromQuery(Name = "year")] int year,
           [FromQuery(Name = "month")] int month,
           [FromQuery(Name = "week")] int week
           )
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                _logger.LogError($"Error, no token provided in Authorization header ");
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            await _service.ProjectService.GetUserProjectRole(projectId, userId); // where ID is project ID

            var response = await _service.PaymentRequestService.GetPayReqDailyData(projectId, year, month, week);
            return Ok(response);
        }
    

        [Authorize]
        [HttpGet("OtherPro/DownloadPayReqFile")]
        public async Task<IActionResult> DownloadPayReqFileOtherPro(
                [FromQuery(Name = "projectId")] string projectId,
                [FromQuery(Name = "paymentRequestId")] string paymentRequestId,
                [FromQuery(Name = "FileName")] string fileName
                )
        {

            //ensure user is otherpro

            //validate input
            //ensure the name of the files are in lower case when saving it 


            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var model = new PaymentRequestFileDto
            {
                ProjectId = projectId,
                PaymentRequestId = paymentRequestId,
                FileName = fileName
            };

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                var err = new ErrorResponse<object> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var file = await _service.PaymentRequestService.DownloadPayReqFileOtherPro(model, userId);
            return File(file.ResponseStream, file.Headers.ContentType);
        }

        [Authorize]
        [HttpGet("PM/DownloadPayReqFile")]
        //[ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> DownloadActivityFilePM(
            [FromQuery(Name = "projectId")] string projectId,
            [FromQuery(Name = "paymentRequestId")] string paymentRequestId,
            [FromQuery(Name = "FileName")] string fileName
            )
        {

            //ensure that the user is a pm

            //validate input
            //ensure the name of the files are in lower case when saving it 


            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var model = new PaymentRequestFileDto
            {
                ProjectId = projectId,
                PaymentRequestId = paymentRequestId,
                FileName = fileName
            };

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.PM && userRole != Enums.UserRoles.Client)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                var err = new ErrorResponse<object> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var file = await _service.PaymentRequestService.DownloadPayReqFilePM(model);
            return File(file.ResponseStream, file.Headers.ContentType);
        }

        [Authorize]
        [HttpGet("OtherPro/DownloadPayReqConfirmationFileOtherPro")]
        public async Task<IActionResult> DownloadPayReqConfirmationFileOtherPro(
            [FromQuery(Name = "projectId")] string projectId,
            [FromQuery(Name = "paymentRequestId")] string paymentRequestId,
            [FromQuery(Name = "FileName")] string fileName
            )
        {

            //ensure user is otherpro

            //validate input
            //ensure the name of the files are in lower case when saving it 


            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var model = new PaymentRequestFileDto
            {
                ProjectId = projectId,
                PaymentRequestId = paymentRequestId,
                FileName = fileName
            };

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.OtherPro)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                var err = new ErrorResponse<PaymentRequestDto> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var file = await _service.PaymentRequestService.DownloadPayReqConfirmationFileOtherPro(model, userId);
            return File(file.ResponseStream, file.Headers.ContentType);
        }


        [Authorize]
        [HttpGet("PM/DownloadPayReqConfirmationFilePM")]
        //[ProducesResponseType(typeof(SuccessResponse<ActivityDto>), 200)]
        public async Task<IActionResult> DownloadPayReqConfirmationFilePM(
            [FromQuery(Name = "projectId")] string projectId,
            [FromQuery(Name = "paymentRequestId")] string paymentRequestId,
            [FromQuery(Name = "FileName")] string fileName
            )
        {

            //ensure that the user is a pm

            //validate input
            //ensure the name of the files are in lower case when saving it 


            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;

            var model = new PaymentRequestFileDto
            {
                ProjectId = projectId,
                PaymentRequestId = paymentRequestId,
                FileName = fileName
            };

            var (userRole, projId) = await _service.ProjectService.GetUserProjectRole(model.ProjectId, userId); // where ID is project ID
            if (userRole != Enums.UserRoles.PM && userRole != Enums.UserRoles.Client)
            {
                //_logger.LogError($"Error, only a  PM (Project Manager) is allowed to approve or reject a project. User is not a PM");
                var err = new ErrorResponse<object> { Message = "User does not have sufficient permission" };
                return StatusCode((int)HttpStatusCode.Forbidden, err);
            }

            var file = await _service.PaymentRequestService.DownloadPayReqConfirmationFilePM(model);
            return File(file.ResponseStream, file.Headers.ContentType);
        }
    }
}
