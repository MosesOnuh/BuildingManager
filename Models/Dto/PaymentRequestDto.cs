using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace BuildingManager.Models.Dto
{
    public class PaymentRequestReqDto
    {
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; } = null;
        public IList<PaymentRequestItemReqDto> Items { get; set; }
        public decimal SumTotalAmount { get; set; }
        public int Type { get; set; }
    }
    public class PaymentRequestPmReqDto
    {
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; } = null;
        public IList<PaymentRequestItemReqDto> Items { get; set; }
        public decimal SumTotalAmount { get; set; }
        public int Type { get; set; }
        public string AssignedTo { get; set; }
    }

    //public class PaymentRequestReqDto
    //{
    //    //public string Id { get; set; }
    //    public string ProjectId { get; set; }
    //    public string Name { get; set; }
    //    public string? Description { get; set; } = null;
    //    public IList<PaymentRequestItemReqDto> Items { get; set; }
    //    //Enum 1 for pre-construction, 2 for construction, 3 for Post-construction
    //    //public int ProjectPhase { get; set; }
    //    public decimal SumTotalAmount { get; set; }
    //    public IFormFile? File { get; set; }
    //    //public DateTime StartDate { get; set; }
    //    //public DateTime EndDate { get; set; }
    //}

    //public class PaymentRequestDtoPaged
    //{
    //    public string ProjectId { get; set; }
    //    public int PageNumber { get; set; }
    //    public int PageSize { get; set; }
    //}

    public class PaymentRequestDto
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        //Enum showing if the task is pending, accepted or done;
        public int Status { get; set; }
        public int Type { get; set; }
        public string? Description { get; set; } = null;
        public IList<PaymentRequestItemDto> Items { get; set; } = new List<PaymentRequestItemDto>();
        public decimal SumTotalAmount { get; set; }
        public string? UserFileName { get; set; }
        public string? UserStorageFileName { get; set; }
        public string? PmFileName { get; set; }
        public string? PmStorageFileName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
    }

    public class PaymentRequestItemReqDto
    {
        //public string PaymentRequestId { get; set; } = null;
        //public string? UserId { get; set; } = null;
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PaymentRequestItemDto
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string PaymentRequestId { get; set; }
        //public string? UserId { get; set; } = null;
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }


    public class PaymentRequestAndMemberDto
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Role { get; set; }
        public int Profession { get; set; }
        public string ProjectId { get; set; }
        public string PaymentRequestId { get; set; }
        public string PaymentRequestName { get; set; }
        //Enum showing if the task is pending, accepted or done;
        public int Status { get; set; }
        public int Type { get; set; }
        public string? Description { get; set; }
        public IList<PaymentRequestItemDto> Items { get; set; } = new List<PaymentRequestItemDto>();
        public decimal SumTotalAmount { get; set; }
        public string? UserFileName { get; set; }
        public string? UserStorageFileName { get; set; }
        public string? PmFileName { get; set; }
        public string? PmStorageFileName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
    }

    public class PaymentRequestStatusUpdateDto
    {
        public string PaymentRequestId { get; set; }
        public string ProjectId { get; set; }
        public int StatusAction { get; set; }
    }

    public class UpdatePaymentRequestItemDto
    {
        public string? Id { get; set; }
        public string PaymentRequestId { get; set; } = null;
        //public string? UserId { get; set; } = null;
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }

    //Dto if OtherPro wants to update the details of a pending project
    public class UpdatePaymentRequestDto
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        // public string UserId { get; set; }
        public string Name { get; set; }
        public int? Status { get; set; } = null;
        public string? Description { get; set; } = null;
        public decimal SumTotalAmount { get; set; }
        public IList<UpdatePaymentRequestItemDto>? Items { get; set; } = null;
        //public string? AssignedTo { get; set; } = null;
    }

    public class UpdateGroupPaymentRequestDto
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        // public string UserId { get; set; }
        public string Name { get; set; }
        public int? Status { get; set; } = null;
        public string? Description { get; set; } = null;
        public decimal SumTotalAmount { get; set; }
        public IList<UpdatePaymentRequestItemDto>? Items { get; set; } = null;
        public IList<UpdatePaymentRequestItemDto>? DeletedItems { get; set; } = null;
        public string? AssignedTo { get; set; } = null;
    }

    public class UpdateSinglePaymentRequestDto
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        // public string UserId { get; set; }
        public string Name { get; set; }
        //public int Status { get; set; }
        public string? Description { get; set; } = null;
        public decimal SumTotalAmount { get; set; }
       
    }

    public class UpdatePaymentRequestPmDto
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        // public string UserId { get; set; }
        public string Name { get; set; }
        //public int Status { get; set; }
        public string? Description { get; set; } = null;
        public decimal SumTotalAmount { get; set; }
        public IList<UpdatePaymentRequestItemDto>? Items { get; set; } = null;
        public IList<UpdatePaymentRequestItemDto>? DeletedItems { get; set; } = null;
        public string AssignedTo { get; set; }
    }

    public class UpdateGroupPaymentRequestPmDto
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        // public string UserId { get; set; }
        public string Name { get; set; }
        public int? Status { get; set; } = null;
        public string? Description { get; set; } = null;
        public decimal SumTotalAmount { get; set; }
        public IList<UpdatePaymentRequestItemDto> Items { get; set; } = null;
        public IList<UpdatePaymentRequestItemDto>? DeletedItems { get; set; } = null;
        public string AssignedTo { get; set; }
    }

    public class UpdateSinglePaymentRequestPmDto
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        // public string UserId { get; set; }
        public string Name { get; set; }
        //public int Status { get; set; }
        public string? Description { get; set; } = null;
        public decimal SumTotalAmount { get; set; }
        public string AssignedTo { get; set; }
    }


    public class AddPaymentRequestFileReqDto
    {
        public string ProjectId { get; set; }
        public string PaymentRequestId { get; set; }
        public IFormFile File { get; set; }
    }

    //public class AddPaymentRequestFileReqDto
    public class AddPaymentRequestFileDto
    {
        public string ProjectId { get; set; }
        public string PaymentRequestId { get; set; }
        public string? FileName { get; set; }
        public string? StorageFileName { get; set; }
        public string? FileExtension { get; set; }
        // public DateTime TimeStamp { get; set; }
    }

    public class PaymentRequestFileDto
    {
        public string ProjectId { get; set; }
        public string PaymentRequestId { get; set; }
        public string FileName { get; set; }
    }

    public class PayReqMonthlyDataDto
    {
        public int Profession { get; set; }
        public decimal Jan { get; set; } = 0.00m;
        public decimal Feb { get; set; } = 0.00m;
        public decimal Mar { get; set; } = 0.00m;
        public decimal Apr { get; set; } = 0.00m;
        public decimal May { get; set; } = 0.00m;
        public decimal Jun { get; set; } = 0.00m;
        public decimal Jul { get; set; } = 0.00m;
        public decimal Aug { get; set; } = 0.00m;
        public decimal Sep { get; set; } = 0.00m;
        public decimal Oct { get; set; } = 0.00m;
        public decimal Nov { get; set; } = 0.00m;
        public decimal Dec { get; set; } = 0.00m;
        public decimal Total { get; set; } = 0.00m;

        public void CalculateTotal()
        {
            Total = Jan + Feb + Mar + Apr + May + Jun + Jul + Aug + Sep + Oct + Nov + Dec;

        }
    }

    public class PayReqWeeklyDataDto
    {
        public int Profession { get; set; }
        public decimal Wk1 { get; set; } = 0.00m;
        public decimal Wk2 { get; set; } = 0.00m;
        public decimal Wk3 { get; set; } = 0.00m;
        public decimal Wk4 { get; set; } = 0.00m;
        public decimal Wk5 { get; set; } = 0.00m;
        public decimal Total { get; set; } = 0.00m;

        public void CalculateTotal()
        {
            Total = Wk1 + Wk2 + Wk3 + + Wk4 + Wk5;
        }
    }

    public class PayReqDailyDataDto
    {
        public int Profession { get; set; }
        public decimal Mon { get; set; } = 0.00m;
        public decimal Tue { get; set; } = 0.00m;
        public decimal Wed { get; set; } = 0.00m;
        public decimal Thu { get; set; } = 0.00m;  
        public decimal Fri { get; set; } = 0.00m;
        public decimal Sat { get; set; } = 0.00m;
        public decimal Sun { get; set; } = 0.00m;
        public decimal Total { get; set; } = 0.00m;

        public void CalculateTotal()
        {
            Total = Mon + Tue + Wed + Thu + Fri + Sat + Sun;
        }
    }

    public class PaymentRequestReqPagedDto {
        public string ProjectId { get; set; }
        public int? RequiredStatus { get; set; } = null;
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

}

