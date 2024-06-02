using BuildingManager.Contracts.Repository;
using BuildingManager.Contracts.Services;
using BuildingManager.Helpers;
using BuildingManager.Models;
using BuildingManager.Models.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BuildingManager.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IRepositoryManager _repository;
        public TokenService(IConfiguration configuration, IRepositoryManager repository)
        {
            _configuration = configuration;
            _repository = repository;

        }

        public async Task<TokenResponse> GenerateTokens(User user, string prevToken)

        {
            
            if (!string.IsNullOrEmpty(prevToken))
            {
                int check = await _repository.TokenRepository.CheckAndDeleteToken(user.Id, prevToken);
                if (check == -1) throw new RestException(HttpStatusCode.NotFound,  "invalid token provided");
            }

            var accessToken = GenerateAccessToken(user.Id);
            var refreshToken = GenrateRefreshToken(user.Id);

            await _repository.TokenRepository.SaveRefreshTokenDetails(user.Id, refreshToken);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private string GenerateAccessToken(string userId)
        {
            var claims = new List<Claim>()
            {
                new Claim("UserId", userId),
                new Claim("tokenType", "1"),
            };

            IConfiguration jwtSettings = _configuration.GetSection("JWT");
            var secretKey = Encoding.ASCII.GetBytes(jwtSettings.GetSection("Secret").Value);
            var accessTokenExpiration = jwtSettings.GetSection("AccessTokenExpiration").Value;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddSeconds(20),
             
                //Expires = DateTime.Now.AddMinutes(int.Parse(accessTokenExpiration)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenrateRefreshToken(string userId)
        {
            var claims = new List<Claim>()
            {
                new Claim("UserId", userId),
                new Claim("tokenType", "2"),
            };

            IConfiguration jwtSettings = _configuration.GetSection("JWT");
            byte[] secretKey = Encoding.ASCII.GetBytes(jwtSettings.GetSection("Secret").Value);
            string refreshTokenExpiration = jwtSettings.GetSection("RefreshTokenExpiration").Value;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(int.Parse(refreshTokenExpiration)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string ValidateToken(string token)
        {
            IConfiguration jwtSettings = _configuration.GetSection("JWT");
            byte[] secretKey = Encoding.ASCII.GetBytes(jwtSettings.GetSection("Secret").Value);

            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.GetSection("Issuer").Value,
                ValidAudience = jwtSettings.GetSection("Audience").Value,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetSection("Secret").Value))
            }, out SecurityToken validatedToken);
            var jwtToken =  (JwtSecurityToken)validatedToken;

            var tokenTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "tokenType");
            if (tokenTypeClaim == null) throw new Exception("Invalid token provided");

            if (tokenTypeClaim.Value != "1") throw new Exception("Invalid token provided");

            //if (DateTime.UtcNow > jwtToken.ValidTo) throw new Exception("token expired");
            if (DateTime.UtcNow > jwtToken.ValidTo) throw new RestException(HttpStatusCode.Unauthorized, "token expired");

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim != null)
            {
                return userIdClaim.Value;
            }

            throw new Exception("Invalid token provided");
        }


        public string ValidateRefreshToken(string token)
        {
            IConfiguration jwtSettings = _configuration.GetSection("JWT");
            byte[] secretKey = Encoding.ASCII.GetBytes(jwtSettings.GetSection("Secret").Value);

            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.GetSection("Issuer").Value,
                ValidAudience = jwtSettings.GetSection("Audience").Value,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetSection("Secret").Value))

            }, out SecurityToken validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;

            var tokenTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "tokenType");
            if (tokenTypeClaim == null) throw new Exception("Invalid token provided");

            if (tokenTypeClaim.Value != "2") throw new Exception("Invalid token provided");

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (DateTime.UtcNow > jwtToken.ValidTo) throw new Exception("token expired");
            if (userIdClaim != null)
            {
                return userIdClaim.Value;
            }

            throw new Exception("Invalid token provided");
        }
    }
}
