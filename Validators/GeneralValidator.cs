using BuildingManager.Helpers;
using BuildingManager.Models.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Net;

namespace BuildingManager.Validators
{
    public class GeneralValidator
    {

        public void ValidateString(string input, string inputName, int maxCharLength)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new RestException(HttpStatusCode.UnprocessableEntity, $"{inputName} cannot be empty");
            }

            if (input.Length > maxCharLength)
            {
                throw new RestException(HttpStatusCode.UnprocessableEntity, $"{inputName} cannot greater than {maxCharLength}");
            }
        }

        public void ValidateInteger(int input, string inputName, int maxNumber)
        {
            if (input == 0 || input < 0)
            {
                throw new RestException(HttpStatusCode.UnprocessableEntity, $"{inputName} cannot be empty or zero");
            }

            if (input > maxNumber)
            {
                throw new RestException(HttpStatusCode.UnprocessableEntity, $"{inputName} cannot be greater than {maxNumber}");
            }
        }

        public void ValidateFile(IFormFile file)
        {
            var maxSize = 30 * 1024 * 1024; // 30 MB limit
            var _allowedExtensions = new string[] { ".jpg", ".jpeg", ".png", ".docx", ".doc", ".pdf" }; // Allowed extensions
            var allowableExtensions = string.Join (", ", _allowedExtensions.Select(x => x.ToUpperInvariant()));



            if (file.Length > maxSize)
            {
                throw new RestException(HttpStatusCode.UnprocessableEntity, $" File cannot be greater than {maxSize} MB");
            }

            if (!_allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower())){
                throw new RestException(HttpStatusCode.UnprocessableEntity, $"Invalid file type. valid file type include: {allowableExtensions}");
            }
        }
    }

    //public class FileValidator : AbstractValidator<IFormFile>
    //{
    //    private readonly long _maxSize;
    //    private readonly string[] _allowedExtensions;

    //    public FileValidator()
    //    {
    //        _maxSize = 7 * 1024 * 1024; // 7 MB limit
    //        _allowedExtensions = new string[] { ".jpg", ".jpeg", ".png", ".docx", ".doc", ".pdf" }; // Allowed extensions

    //        RuleFor(file => file)
    //          .Must(BeValidSize).WithMessage("File size exceeds the maximum allowed size of {0} bytes.")
    //          .Must(BeValidExtension).WithMessage("Invalid file type. Only {0} extensions are allowed.");
    //    }

    //    private bool BeValidSize(IFormFile file) => file.Length <= _maxSize;

    //    private bool BeValidExtension(IFormFile file)
    //    {
    //        var extension = Path.GetExtension(file.FileName).ToLower();
    //        return _allowedExtensions.Contains(extension);
    //    }
    //}




}
