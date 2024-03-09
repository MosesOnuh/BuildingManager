﻿using BuildingManager.Contracts.Repository;
using BuildingManager.Utils.Logger;
using Microsoft.Extensions.Configuration;
using System;

namespace BuildingManager.Repository
{
    public class RepositoryManager: IRepositoryManager
    {
        private readonly Lazy<ITokenRepository> _tokenRepository;
        private readonly Lazy<IUserRepository> _userRepository;


        public RepositoryManager(
           ILoggerManager logger,
           IConfiguration configuration
           ) 
        {
            _tokenRepository = new Lazy<ITokenRepository>(() => new TokenRepository(configuration, logger));
            _userRepository = new Lazy<IUserRepository>(() => new UserRepository(configuration, logger));
        }

        public ITokenRepository TokenRepository => _tokenRepository.Value;
        public IUserRepository UserRepository => _userRepository.Value;
    }
}
