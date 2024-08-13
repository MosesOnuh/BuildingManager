using BuildingManager.Contracts.Repository;
using BuildingManager.Contracts.Services;
using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using BuildingManager.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildingManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IServiceManager _service;
        private readonly IRepositoryManager _repository;
        private readonly GeneralValidator _generalValidator;
        public ChatController (IServiceManager service, IRepositoryManager repository)
        {
            _service = service;
            _repository = repository;
            _generalValidator = new GeneralValidator();
        }

        [Authorize]
        [HttpGet("user/GroupChatMessages/{groupId}")]  //activity Id                                                
        public async Task<IActionResult> GetGroupChatMessages(string groupId)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<object> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }
            _generalValidator.ValidateString(groupId, "groupId", 50);

            var userId = HttpContext.Items["UserId"] as string;
            //var projectRole = _service.ProjectService.GetUserProjectRole(userId, id); // where ID is project ID

            await _service.ProjectService.GetUserProjectRole(groupId, userId); // where ID is project ID

            var chatData = await _repository.ChatRepository.GetGroupChatMessages(groupId);
            var response = new SuccessResponse<IList<ChatMessage>>
            {
                Message = "Chat Message successfully gotten",
                Data = chatData,
            };
            return Ok(response);
        }
    }    
}
