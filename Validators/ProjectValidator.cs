using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using FluentValidation;
using System;
using System.Net;
using System.Web;

namespace BuildingManager.Validators
{
    public class CreateProjectValidator : AbstractValidator<ProjectRequestDto>
    {
        public CreateProjectValidator()
        {
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Address).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            RuleFor(x => x.State).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Country).Cascade(CascadeMode.Stop).MaximumLength(70);
            RuleFor(x => x.StartDate).Cascade(CascadeMode.Stop).NotEmpty();
            RuleFor(x => x.EndDate).Cascade(CascadeMode.Stop).NotEmpty();
        }


        //public void ValidateProjectRequestDto(ProjectRequestDto project)
        //{
        //    project.Name = SanitizeInput(project.Name).ToLower();
        //    project.Address = SanitizeInput(project.Address).ToLower();
        //    project.State = SanitizeInput(project.State).ToLower();
        //    project.Country = SanitizeInput(project.Country).ToLower();

        //    if (string.IsNullOrWhiteSpace(project.Name)) 
        //    {
        //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid name");
        //    }
        //    if (string.IsNullOrWhiteSpace(project.Address)) 
        //    {
        //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid address");
        //    }
        //    if (string.IsNullOrWhiteSpace(project.State)) 
        //    {
        //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid state");
        //    }
        //    if (string.IsNullOrWhiteSpace(project.Country)) 
        //    {
        //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid country");
        //    }

        //    if (project.StartDate == default) 
        //    {
        //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid Start Date");
        //    };

        //    if (project.EndDate == default)
        //    {
        //        throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid End Date");
        //    };
        //}

        //private string SanitizeInput(string input)
        //{
        //    if (input == null) return "";

        //    // Remove potentially harmfull characters or code
        //    return HttpUtility.HtmlEncode(input.Trim());
        //}
    }

    public class ProjectValidator : AbstractValidator<ProjectDto>
    {
        public ProjectValidator()
        {
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Address).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            RuleFor(x => x.State).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Country).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(70);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
        }
    }

    public class InviteNotificationValidator : AbstractValidator<InviteNotificationRequestDto>
    {
        public InviteNotificationValidator()
        {
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Email).Cascade(CascadeMode.Stop).EmailAddress().NotEmpty().MaximumLength(254);
            RuleFor(x => x.Profession).Cascade(CascadeMode.Stop).NotEmpty().GreaterThan(0).LessThanOrEqualTo(13);
        }
    }
}
