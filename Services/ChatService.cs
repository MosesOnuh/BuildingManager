using BuildingManager.Contracts.Repository;
using BuildingManager.Models.Entities;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace BuildingManager.Services
{
    public class ChatService : Hub
    {
        //_chatRepository;
        private readonly IRepositoryManager _repository;

        public ChatService (
             IRepositoryManager repository
        ) { 
        _repository = repository;
        }

        public async Task CreateGroupChat(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
            await Clients.Caller.SendAsync("UserConnected");
        }

        //public async Task AddUserConnectionId(string userId)
        //{
        //    _chatService.AddUserConnectionId(userId, Context.ConnectionId);
        //    await DisplayOnlineUsers();
        //}

        public async Task ReceiveGroupChatMessage(ChatMessage message)
        {
            //message.Id = Guid.NewGuid().ToString();
            message.CreatedAt = DateTime.Now;
            //_db.ProdOwnerMessHistory.Add(message);

            //try { } catch { }
            //throw new Exception();
            await _repository.ChatRepository.CreateGroupChatMessage(message);
            await ReceiveGroupMessage(message);
        }

        public async Task ReceiveGroupMessage(ChatMessage message)
        {
            await Clients.Group(message.GroupId).SendAsync($"New{message.GroupId}Message", message);
        }
    }
}
