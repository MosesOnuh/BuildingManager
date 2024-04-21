using BuildingManager.Models;
using BuildingManager.Models.Dto;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Services
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateTokens(User user, string prevToken);
        string ValidateToken(string token);
    }
}
