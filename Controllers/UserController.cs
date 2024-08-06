using Asp.Versioning;
using BuildingManager.Contracts.Services;
using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using BuildingManager.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BuildingManager.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IServiceManager _service;

        public UserController(IServiceManager service)
        {
            _service = service;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(SuccessResponse<TokenResponse>), 200)]
        public async Task<IActionResult> LoginUser(UserLoginReq model)
        {
            var response =  await _service.AuthenticationService.Login(model);

            return Ok(response);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutUser()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Request.Headers["Authorization"]))
            {
                var err = new ErrorResponse<ActivityDto> { Message = "No token provided in Authorization header" };
                return Unauthorized(err);
            }

            var userId = HttpContext.Items["UserId"] as string;
            var response = await _service.AuthenticationService.Logout(userId);

            return Ok(response);
        }

        [HttpPost("generateTokens")]
        [ProducesResponseType(typeof(SuccessResponse<TokenResponse>), 200)]
        public async Task<IActionResult> GenerateTokens(TokenReq model)
        {
            var response = await _service.AuthenticationService.GenerateTokens(model);

            return Ok(response);
        }

        [HttpPost("signup")]
        [ProducesResponseType(typeof(SuccessResponse<UserDto>), 200)]
        public async Task<IActionResult> SignUp([FromBody] UserCreateDto model)
        {
            var response = await _service.AuthenticationService.SignUp(model);
            return Ok(response);
        }
    }
}


//write procedure to check this
//bool phoneExist = await _repository.UserRepository.CheckPhoneExists(model.PhoneNumber):  proc_checkPhonelExists
//validate request
