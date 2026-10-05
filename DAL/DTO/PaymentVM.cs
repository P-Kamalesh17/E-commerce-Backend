using DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTO
{
    public class PaymentDetailVM
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PaymentVM
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreatePaymentVM
    {
        public int OrderId { get; set; }
        public string? TransactionId { get; set; }
    }

    public class UpdatePaymentStatusVM
    {
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
