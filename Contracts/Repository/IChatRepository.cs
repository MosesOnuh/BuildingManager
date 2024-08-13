using BuildingManager.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Repository
{
    public interface IChatRepository
    {
        Task CreateGroupChatMessage(ChatMessage model);

        Task<IList<ChatMessage>> GetGroupChatMessages(string groupId);

    }
}
