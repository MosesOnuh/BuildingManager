namespace BuildingManager.Contracts.Repository
{
    public interface IRepositoryManager
    {
        ITokenRepository TokenRepository { get; }
        IUserRepository UserRepository { get; }
    }
}
