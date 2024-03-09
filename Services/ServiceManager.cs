using BuildingManager.Contracts.Repository;
using BuildingManager.Contracts.Services;
using BuildingManager.Utils.Logger;
using Microsoft.Extensions.Configuration;
using System;

namespace BuildingManager.Services
{
    public class ServiceManager: IServiceManager
    {
        private readonly Lazy<IAuthenticationService> _authenticationService;
        private readonly Lazy<ITokenService> _tokenService;

        public ServiceManager(
            ILoggerManager logger,
            IConfiguration configuration,
            IRepositoryManager repositoryManager
            )
        {
            _tokenService = new Lazy<ITokenService>(() => new TokenService(configuration, repositoryManager));
            _authenticationService = new Lazy<IAuthenticationService>(() => new AuthenticationService(logger,repositoryManager, this));
        }

        public ITokenService TokenService => _tokenService.Value;
        public IAuthenticationService AuthenticationService => _authenticationService.Value;
 





    }
}
