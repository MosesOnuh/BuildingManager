using BuildingManager.Contracts.Repository;
using BuildingManager.Contracts.Services;
using BuildingManager.Utils.Logger;
using BuildingManager.Utils.StorageManager;
using Microsoft.Extensions.Configuration;
using System;

namespace BuildingManager.Services
{
    public class ServiceManager: IServiceManager
    {
        private readonly Lazy<IAuthenticationService> _authenticationService;
        private readonly Lazy<ITokenService> _tokenService;
        private readonly Lazy<IProjectService> _projectService;
        private readonly Lazy<IActivityService> _activityService;

        public ServiceManager(
            ILoggerManager logger,
            IConfiguration configuration,
            IRepositoryManager repositoryManager,
            IStorageManager storageManager
            )
        {
            _tokenService = new Lazy<ITokenService>(() => new TokenService(configuration, repositoryManager));
            _authenticationService = new Lazy<IAuthenticationService>(() => new AuthenticationService(logger,repositoryManager, this));
            _projectService = new Lazy<IProjectService>(() => new ProjectService(logger, repositoryManager));
            _activityService = new Lazy<IActivityService>(() => new ActivityService(logger, storageManager, repositoryManager));
        }

        public ITokenService TokenService => _tokenService.Value;
        public IAuthenticationService AuthenticationService => _authenticationService.Value;
        public IProjectService ProjectService => _projectService.Value;
        public IActivityService ActivityService => _activityService.Value;

 
    }
}
