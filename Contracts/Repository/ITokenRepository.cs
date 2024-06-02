using System.Threading.Tasks;

namespace BuildingManager.Contracts.Repository
{
    public interface ITokenRepository
    {
         Task SaveRefreshTokenDetails(string userId, string refreshToken);
        Task<int> CheckAndDeleteToken(string userId, string prevToken);
        Task DeleteRefreshTokens(string userId);
    }
}

