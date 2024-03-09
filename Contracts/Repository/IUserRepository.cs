using BuildingManager.Models;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Repository
{
    public interface IUserRepository
    {
        Task<bool> CheckEmailExists(string userEmail);
        Task SignUp(User user);
        Task<User?> GetUserByEmail(string userEmail);
    }
}
