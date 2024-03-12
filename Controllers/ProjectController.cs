using BuildingManager.Contracts.Services;
using BuildingManager.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BuildingManager.Models.Dto;
using System.Threading.Tasks;

namespace BuildingManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IServiceManager _service;
        public ProjectController(IServiceManager service)
        {
            _service = service;
        }

        [HttpPost("CreateProject")]
        [ProducesResponseType(typeof(SuccessResponse<ProjectDto>), 201)]
        public async Task<IActionResult> CreateProject ([FromBody] ProjectCreateDto model) 
        {
            var response = await _service.ProjectService.CreateProject(model);
            return Ok(response);
        }


    }
}
