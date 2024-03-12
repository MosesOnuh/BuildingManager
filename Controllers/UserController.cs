﻿using Asp.Versioning;
using BuildingManager.Contracts.Services;
using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
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

        [HttpPost("Signup")]
        [ProducesResponseType(typeof(SuccessResponse<UserDto>), 200)]
        public async Task<IActionResult> SignUp([FromBody] UserCreateDto model)
        {
            var response = await _service.AuthenticationService.SignUp(model);
            return Ok(response);
        }
    }
}