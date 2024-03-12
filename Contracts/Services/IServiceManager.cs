namespace BuildingManager.Contracts.Services
{
    public interface IServiceManager
    {
        ITokenService TokenService { get; }
        IAuthenticationService AuthenticationService { get; }
        IProjectService ProjectService { get; }
    }
}
