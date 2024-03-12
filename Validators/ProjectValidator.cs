using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using System;
using System.Net;
using System.Web;

namespace BuildingManager.Validators
{
    public class ProjectValidator
    {
        public void ValidateProjectCreateDto(ProjectCreateDto project)
        {
            project.Name = SanitizeInput(project.Name).ToLower();
            project.Address = SanitizeInput(project.Address).ToLower();
            project.State = SanitizeInput(project.State).ToLower();
            project.Country = SanitizeInput(project.Country).ToLower();

            if (string.IsNullOrWhiteSpace(project.Name)) 
            {
                throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid name");
            }
            if (string.IsNullOrWhiteSpace(project.Address)) 
            {
                throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid address");
            }
            if (string.IsNullOrWhiteSpace(project.State)) 
            {
                throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid state");
            }
            if (string.IsNullOrWhiteSpace(project.Country)) 
            {
                throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid country");
            }

            if (project.StartDate == default) 
            {
                throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid Start Date");
            };

            if (project.EndDate == default)
            {
                throw new RestException(HttpStatusCode.UnprocessableEntity, "invalid End Date");
            };
        }

        private string SanitizeInput(string input)
        {
            if (input == null) return "";

            // Remove potentially harmfull characters or code
            return HttpUtility.HtmlEncode(input.Trim());
        }
    }
}
