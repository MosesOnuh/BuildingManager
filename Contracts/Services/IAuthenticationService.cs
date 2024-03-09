using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Services
{
    public interface IAuthenticationService
    {
        Task<SuccessResponse<TokenResponse>> Login(UserLoginReq model);
        Task<SuccessResponse<UserDto>> SignUp(UserCreateDto model);

    }
}
