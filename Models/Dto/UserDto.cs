using System;

namespace BuildingManager.Models.Dto
{
    public class UserLoginReq
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoggedinUserDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }


    public class TokenReq
    {
        public string RefreshToken { get; set; }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RefreshTokenDto
    {
        public string UserId { get; set; }
        public string RefreshToken { get; set; }
    }

 public class UserCreateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int EmailVerified { get; set; }
        //public DateTime CreatedAt { get; set; }
    }

}
