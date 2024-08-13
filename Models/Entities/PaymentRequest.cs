using BuildingManager.Models.Entities;
using System;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BuildingManager.Models.Entities
{
    public class PaymentRequest
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string CreatedBy { get; set; }
        public string Name { get; set; }
        //Enum showing if the task is pending, accepted or done;
        public int Status { get; set; }
        public int Type { get; set; }
        public string? Description { get; set; }
        public IList<PaymentRequestItem> Items { get; set; }
        public decimal SumTotalAmount { get; set; }
        public string? UserFileName { get; set; }
        public string? UserStorageFileName { get; set; }
        public string? PmFileName { get; set; }
        public string? PmStorageFileName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }  
        public DateTime? ConfirmedAt { get; set; }
    }


    public class PaymentRequestItem
    { 
        public string Id { get; set; }
        public string PaymentRequestId { get; set; }
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
