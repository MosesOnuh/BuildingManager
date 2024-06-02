using BuildingManager.Models;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Repository
{
    public interface IUserRepository
    {
        Task<bool> CheckEmailExists(string userEmail);
        Task<bool> CheckPhoneExists(string phoneNumber);
        Task SignUp(User user);
        Task<User?> GetUserByEmail(string userEmail);
        Task<User?> GetUserById(string userId);
    }
}
