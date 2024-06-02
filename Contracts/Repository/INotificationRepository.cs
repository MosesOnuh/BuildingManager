using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Repository
{
    public interface INotificationRepository
    {
        Task<int> CreateInviteNotification(InviteNotification model);
        Task<(int, int)> AcceptProjectInvite(ProjectInviteStatusUpdateDto model, string userId);
        Task<(int, int)> RejectProjectInvite(ProjectInviteStatusUpdateDto model, string userId);
        Task<IList<ReceivedInviteRespDto>> GetReceivedProjectInvites(string userId);
        Task<(int, IList<SentInviteRespDto>)> GetSentProjectInvites(SentProjInvitesDtoPaged model);
        //Task<(int, IList<InviteResponseDto>)> GetProjectInvites(ProjectInvitesDtoPaged model, string userId);
    }
}
