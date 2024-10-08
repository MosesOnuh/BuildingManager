﻿namespace BuildingManager.Contracts.Repository
{
    public interface IRepositoryManager
    {
        ITokenRepository TokenRepository { get; }
        IUserRepository UserRepository { get; }
        IProjectRepository ProjectRepository { get; }
        INotificationRepository NotificationRepository { get; }
        IActivityRepository ActivityRepository { get; }
        IPaymentRequestRepository PaymentRequestRepository { get; }
        IChatRepository ChatRepository { get; }
    }
}
