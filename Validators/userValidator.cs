using BuildingManager.Models.Dto;
using System.Web;
using System.Net;
using BuildingManager.Helpers;
using FluentValidation;

namespace BuildingManager.Validators
{
    public class UserValidator : AbstractValidator<UserCreateDto> 
    { 
        public UserValidator()
        {
            RuleFor(x => x.FirstName).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Email).Cascade(CascadeMode.Stop).EmailAddress().NotEmpty().MaximumLength(254);
            RuleFor(x => x.PhoneNumber).Cascade(CascadeMode.Stop).MinimumLength(11).MaximumLength(15);
            RuleFor(x => x.Password).Cascade(CascadeMode.Stop).NotEmpty().MinimumLength(4).MaximumLength(30);
            RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.Stop).NotEmpty().MinimumLength(4).MaximumLength(30);
        }
    }

    public class LogInUserValidator : AbstractValidator<UserLoginReq>
    {  public LogInUserValidator()
        {
            RuleFor(x => x.Email).Cascade(CascadeMode.Stop).EmailAddress().NotEmpty().MaximumLength(254);
            RuleFor(x => x.Password).Cascade(CascadeMode.Stop).NotEmpty().MinimumLength(4).MaximumLength(30);
        } 
    }

    // Add universal validation
    public class TokenValidator : AbstractValidator<TokenReq>
    {
        public TokenValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(300);
        }
    }


    //public class UserValidator
    //{
    //public void ValidateUserCreateDto (UserCreateDto user) 
    //{
    //    user.FirstName = SanitizeInput(user.FirstName).ToLower();
    //    user.LastName = SanitizeInput(user.LastName).ToLower();
    //    user.Email = SanitizeInput(user.Email);
    //    user.PhoneNumber = SanitizeInput(user.PhoneNumber);
    //    user.Password = SanitizeInput(user.Password);
    //    user.ConfirmPassword = SanitizeInput(user.ConfirmPassword);

    //    if (string.IsNullOrWhiteSpace(user.FirstName))
    //    {
    //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid first name");
    //    }
    //    if (string.IsNullOrWhiteSpace(user.LastName))
    //    {
    //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid last name");
    //    }
    //    if (string.IsNullOrWhiteSpace(user.Email))
    //    {
    //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid email");
    //    }
    //    if (string.IsNullOrWhiteSpace(user.PhoneNumber))
    //    {
    //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid phone number");
    //    }
    //    if (string.IsNullOrWhiteSpace(user.Password))
    //    {
    //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid password");
    //    }
    //    if (string.IsNullOrWhiteSpace(user.ConfirmPassword))
    //    {
    //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid confirm password");
    //    }          
    //}

    //public void ValidateUserUserLoginReq(UserLoginReq model) 
    //{
    //    model.Email = SanitizeInput(model.Email);
    //    model.Password = SanitizeInput(model.Password);

    //    if (string.IsNullOrWhiteSpace(model.Email))
    //    {
    //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid email");
    //    }

    //    if (string.IsNullOrWhiteSpace(model.Password))
    //    {
    //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid password");
    //    }
    //}

    //private string SanitizeInput(string input)
    //{
    //    if (input == null) return "";

    //    // Remove potentially harmfull characters or code
    //    return HttpUtility.HtmlEncode(input.Trim());
    //}
    //}
}
