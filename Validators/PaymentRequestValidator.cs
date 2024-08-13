using BuildingManager.Models.Dto;
using FluentValidation;
using System.Collections.Generic;

namespace BuildingManager.Validators
{
    public class PaymentRequestValidator : AbstractValidator<PaymentRequestReqDto>
    {
        public PaymentRequestValidator()
        {
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Items).Must(ValidateItems);
            RuleFor(x => x.Description).Must(ValidateDescription);
            RuleFor(x => x.Type).Cascade(CascadeMode.Stop).NotEmpty().GreaterThan(0).LessThanOrEqualTo(2); //changed from 3 to 2
            RuleFor(x => x.SumTotalAmount).NotEmpty().GreaterThan(0);
        }

        private bool ValidateItems(IList<PaymentRequestItemReqDto> items)
        {
            if (items == null)
            {
                return false;
            }

            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    var validationResult = new PaymentRequestItemReqDtoValidator().Validate(item);
                    if (!validationResult.IsValid)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidateDescription(string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                if (description.Length > 500)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class PaymentRequestPmReqDtoValidator : AbstractValidator<PaymentRequestPmReqDto>
    {
        public PaymentRequestPmReqDtoValidator()
        {
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Items).Must(ValidateItems);
            RuleFor(x => x.Description).Must(ValidateDescription);
            RuleFor(x => x.Type).Cascade(CascadeMode.Stop).NotEmpty().GreaterThan(0).LessThanOrEqualTo(2); //changed from 3 to 2
            RuleFor(x => x.SumTotalAmount).NotEmpty().GreaterThan(0);
            RuleFor(x => x.AssignedTo).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
        }

        private bool ValidateItems(IList<PaymentRequestItemReqDto> items)
        {
            if (items == null)
            {
                return false;
            }

            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    var validationResult = new PaymentRequestItemReqDtoValidator().Validate(item);
                    if (!validationResult.IsValid)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidateDescription(string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                if (description.Length > 500)
                {
                    return false;
                }
            }
            return true;
        }
    }
    //Do not add this in the program.cs file
    public class PaymentRequestItemReqDtoValidator : AbstractValidator<PaymentRequestItemReqDto>
    {
        public PaymentRequestItemReqDtoValidator()
        {
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Price).NotEmpty().GreaterThan(0);
            RuleFor(x => x.Quantity).NotEmpty().GreaterThan(0);
            RuleFor(x => x.TotalAmount).NotEmpty().GreaterThan(0);
        }
    }

    

    public class PaymentRequestStatusUpdateDtoValidator : AbstractValidator<PaymentRequestStatusUpdateDto>
    {
        public PaymentRequestStatusUpdateDtoValidator()
        {
            RuleFor(x => x.PaymentRequestId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.StatusAction).Cascade(CascadeMode.Stop).NotEmpty().GreaterThan(0).LessThanOrEqualTo(5);
        }
    }

    public class UpdateSinglePaymentRequestDtoValidator : AbstractValidator<UpdateSinglePaymentRequestDto>
    {
        public UpdateSinglePaymentRequestDtoValidator()
        {
            RuleFor(x => x.Id).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).Must(ValidateDescription);
            RuleFor(x => x.SumTotalAmount).NotEmpty().GreaterThan(0);
        }

        private bool ValidateDescription(string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                if (description.Length > 500)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class UpdateSinglePaymentRequestPmDtoDtoValidator : AbstractValidator<UpdateSinglePaymentRequestPmDto>
    {
        public UpdateSinglePaymentRequestPmDtoDtoValidator()
        {
            RuleFor(x => x.Id).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).Must(ValidateDescription);
            RuleFor(x => x.SumTotalAmount).NotEmpty().GreaterThan(0);
            RuleFor(x => x.AssignedTo).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
        }

        private bool ValidateDescription(string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                if (description.Length > 500)
                {
                    return false;
                }
            }
            return true;
        }
    }

    //public class UpdatePaymentRequestDtoValidator : AbstractValidator<UpdatePaymentRequestDto>
    //{
    //    public UpdatePaymentRequestDtoValidator()
    //    {
    //        RuleFor(x => x.Id).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
    //        RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
    //        RuleFor(x => x.Items).Must(ValidateItems);
    //        RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
    //        RuleFor(x => x.Description).Must(ValidateDescription);
    //        RuleFor(x => x.SumTotalAmount).NotEmpty().GreaterThan(0);
    //    }

    //    private bool ValidateItems(IList<UpdatePaymentRequestItemDto> items)
    //    {

    //        if (items != null && items.Count > 0)
    //        {
    //            foreach (var item in items)
    //            {
    //                var validationResult = new UpdatePaymentRequestItemDtoValidator().Validate(item);
    //                if (!validationResult.IsValid)
    //                {
    //                    return false;
    //                }
    //            }
    //        }

    //        return true;
    //    }

    //    private bool ValidateDescription(string description)
    //    {
    //        if (!string.IsNullOrEmpty(description))
    //        {
    //            if (description.Length > 500)
    //            {
    //                return false;
    //            }
    //        }
    //        return true;
    //    }
    //}

    public class UpdateGroupPaymentRequestDtoValidator : AbstractValidator<UpdateGroupPaymentRequestDto>
    {
        public UpdateGroupPaymentRequestDtoValidator()
        {
            RuleFor(x => x.Id).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Items).Must(ValidateItems);
            RuleFor(x => x.DeletedItems).Must(ValidateItems);
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).Must(ValidateDescription);
            RuleFor(x => x.SumTotalAmount).NotEmpty().GreaterThan(0);
        }

        private bool ValidateItems(IList<UpdatePaymentRequestItemDto> items)
        {

            if (items != null && items.Count > 0)
            {
                foreach (var item in items)
                {
                    var validationResult = new UpdatePaymentRequestItemDtoValidator().Validate(item);
                    if (!validationResult.IsValid)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidateDescription(string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                if (description.Length > 500)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class UpdatePaymentRequestPmDtoValidator : AbstractValidator<UpdatePaymentRequestPmDto>
    {
        public UpdatePaymentRequestPmDtoValidator()
        {
            RuleFor(x => x.Id).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Items).Must(ValidateItems);
            RuleFor(x => x.DeletedItems).Must(ValidateItems);
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).Must(ValidateDescription);
            RuleFor(x => x.SumTotalAmount).NotEmpty().GreaterThan(0);
            RuleFor(x => x.AssignedTo).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
        }

        private bool ValidateItems(IList<UpdatePaymentRequestItemDto> items)
        {

            if (items != null && items.Count > 0)
            {
                foreach (var item in items)
                {
                    var validationResult = new UpdatePaymentRequestItemDtoValidator().Validate(item);
                    if (!validationResult.IsValid)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidateDescription(string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                if (description.Length > 500)
                {
                    return false;
                }
            }
            return true;
        }
    }


    public class UpdatePaymentRequestItemDtoValidator : AbstractValidator<UpdatePaymentRequestItemDto>
    {
        public UpdatePaymentRequestItemDtoValidator()
        {
            RuleFor(x => x.Id).Must(ValidateId);
            RuleFor(x => x.PaymentRequestId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Price).NotEmpty().GreaterThan(0);
            RuleFor(x => x.Quantity).NotEmpty().GreaterThan(0);
            RuleFor(x => x.TotalAmount).NotEmpty().GreaterThan(0);
        }

        private bool ValidateId(string Id)
        {
            if (!string.IsNullOrEmpty(Id))
            {
                if (Id.Length > 50)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class AddPaymentRequestFileReqDtoValidator : AbstractValidator<AddPaymentRequestFileReqDto>
    {
        public AddPaymentRequestFileReqDtoValidator()
        {
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(50);
        }
    }
}
    