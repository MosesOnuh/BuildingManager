using BuildingManager.Models.Dto;
using FluentValidation;

namespace BuildingManager.Validators
{
    public class ActivityRequestValidator: AbstractValidator<ActivityRequestDto>
    {
        public ActivityRequestValidator()
        {
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            //RuleFor(x => x.Description).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(500);
            //RuleFor(x => x.Description).Cascade(CascadeMode.Stop).MaximumLength(500);
            RuleFor(x => x.ProjectPhase).Cascade(CascadeMode.Stop).NotEmpty().GreaterThan(0).LessThanOrEqualTo(3);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
        }
    }

    public class ActivityPmRequestValidator : AbstractValidator<ActivityPmRequestDto>
    {
        public ActivityPmRequestValidator()
        {
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            //RuleFor(x => x.Description).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(500);
            //RuleFor(x => x.Description).Cascade(CascadeMode.Stop).MaximumLength(500);
            RuleFor(x => x.ProjectPhase).Cascade(CascadeMode.Stop).NotEmpty().GreaterThan(0).LessThanOrEqualTo(3);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
            RuleFor(x => x.AssignedTo).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
        }
    }

    public class ActivityStatusUpdateDtoValidator : AbstractValidator<ActivityStatusUpdateDto>
    {
        public ActivityStatusUpdateDtoValidator()
        {
            RuleFor(x => x.ActivityId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.StatusAction).Cascade(CascadeMode.Stop).NotEmpty().GreaterThan(0).LessThanOrEqualTo(5);
        }
    }

    public class ActivityStatusToDoneDtoValidator : AbstractValidator<ActivityStatusToDoneDto>
    {
        public ActivityStatusToDoneDtoValidator()
        {
            RuleFor(x => x.ActivityId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
        }
    }

    public class ActivityActualDatesDtoValidator : AbstractValidator<ActivityActualDatesDto>
    {
        public ActivityActualDatesDtoValidator()
        {
            RuleFor(x => x.ActivityId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ActualStartDate).NotEmpty();
            RuleFor(x => x.ActualEndDate).NotEmpty();
        }
    }


    public class UpdateActivityDetailsDtoValidator : AbstractValidator<UpdateActivityDetailsDto>
    {
        public UpdateActivityDetailsDtoValidator()
        {
            RuleFor(x => x.ActivityId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            //RuleFor(x => x.Description).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(500);
            //RuleFor(x => x.Description).Cascade(CascadeMode.Stop).MaximumLength(500);
            RuleFor(x => x.ProjectPhase).Cascade(CascadeMode.Stop).NotEmpty().GreaterThan(0).LessThanOrEqualTo(3);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
        }
    }

    public class UpdateActivityPmDetailsReqDtoValidator : AbstractValidator<UpdateActivityPmDetailsReqDto>
    {
        public UpdateActivityPmDetailsReqDtoValidator()
        {
            RuleFor(x => x.ActivityId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            //RuleFor(x => x.Description).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(500);
            //RuleFor(x => x.Description).Cascade(CascadeMode.Stop).MaximumLength(500);
            RuleFor(x => x.ProjectPhase).Cascade(CascadeMode.Stop).NotEmpty().GreaterThan(0).LessThanOrEqualTo(3);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
            RuleFor(x => x.AssignedTo).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
        }
    }

    public class AddActivityFileRequestDtoValidator : AbstractValidator<AddActivityFileRequestDto>
    {
        public AddActivityFileRequestDtoValidator()
        {          
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ActivityId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
        }
    }
}
