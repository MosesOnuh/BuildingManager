using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using BuildingManager.Contracts.Services;
using System;
using System.Net;
using System.Threading.Tasks;
using BuildingManager.Models;
using BuildingManager.Enums;
using BuildingManager.Utils.Logger;
using BuildingManager.Contracts.Repository;
using BuildingManager.Validators;


namespace BuildingManager.Services
{
   
     public class AuthenticationService: IAuthenticationService
        {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryManager _repository;
        private readonly IServiceManager _service;
        

        public AuthenticationService(
                ILoggerManager logger,
                IRepositoryManager repository,
                IServiceManager service)
            {
            _service = service;
            _logger = logger;
            _repository = repository;
            }

        public async Task<SuccessResponse<TokenResponse>> Login(UserLoginReq model)
        {
            _logger.LogInfo("Logging in a user");
            var validate = new UserValidator();
            validate.ValidateUserUserLoginReq(model);

            var user = await _repository.UserRepository.GetUserByEmail(model.Email);
            if (user == null) {
                throw new RestException(HttpStatusCode.NotFound, "User does not exist.");
            };
            bool verified = VerifyPassword (model.Password, user.Password);

            if (!verified)
            {
                throw new RestException(HttpStatusCode.Unauthorized, "Wrong Email or Password");
            }

            var tokenDetails = await _service.TokenService.GenerateTokens(user, "");

            return new SuccessResponse<TokenResponse>
                {
                    Message = "Login successful",
                    Data = tokenDetails
                };
            }


        public async Task<SuccessResponse<TokenResponse>> Logout(string userId)
        {
            await _repository.TokenRepository.DeleteRefreshTokens(userId);
            return new SuccessResponse<TokenResponse>
            {
                Message = "Tokens successfully deleted",
                Data = null
            };
        }


        //validate request
        public async Task<SuccessResponse<TokenResponse>> GenerateTokens(TokenReq model)
        {
            _logger.LogInfo("Generating new tokens");
            //var validate = new UserValidator();
            //validate.ValidateUserUserLoginReq(model);

            
            //bool verified = VerifyPassword(model.Password, user.Password);

            //if (!verified)
            //{
            //    throw new RestException(HttpStatusCode.Unauthorized, "Wrong Email or Password");
            //}

            var userId = _service.TokenService.ValidateRefreshToken(model.RefreshToken);
            var user = await _repository.UserRepository.GetUserById(userId);
            if (user == null)
            {
                throw new RestException(HttpStatusCode.NotFound, "User does not exist.");
            };

            var tokenDetails = await _service.TokenService.GenerateTokens(user, model.RefreshToken);

            return new SuccessResponse<TokenResponse>
            {
                Message = "Successfully generated tokens",
                Data = tokenDetails
            };
        }

        public async Task<SuccessResponse<UserDto>> SignUp(UserCreateDto model)
        {
            _logger.LogInfo("Signing up a user");
            var  validate = new UserValidator();
            validate.ValidateUserCreateDto(model);

            bool emailExist = await _repository.UserRepository.CheckEmailExists(model.Email);
            if (emailExist)
            {
                throw new RestException(HttpStatusCode.Conflict, "Email address already exists.");
            }

            //bool phoneExist = await _repository.UserRepository.CheckPhoneExists(model.PhoneNumber);
            //if (phoneExist)
            //{
            //    throw new RestException(HttpStatusCode.Conflict, "Phone number already exists.");
            //}

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Password = model.Password,
                EmailVerified = (int)EUserVerification.NotVerified
            };


            int comparison = string.Compare(model.Password, model.ConfirmPassword, StringComparison.OrdinalIgnoreCase);
            if (comparison != 0)
            {
                throw new RestException(HttpStatusCode.BadRequest, "Passwords do not match");
            }
            user.Password = PasswordHasher(model.Password);
            await _repository.UserRepository.SignUp(user);

           return new SuccessResponse<UserDto>
            {
                Message = "SignUp successful",
            };
        }

        private string PasswordHasher(string password)
        {
            const int saltLength = 10;
            return BCrypt.Net.BCrypt.HashPassword(password, saltLength);
        }

        private bool VerifyPassword (string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

    }
    
}



//write procedure to check this
//bool phoneExist = await _repository.UserRepository.CheckPhoneExists(model.PhoneNumber):  proc_checkPhonelExists
//validate request