using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Repository
{
    public interface INotificationRepository
    {
        Task<int> CreateInviteNotification(InviteNotification model);
    }
}
