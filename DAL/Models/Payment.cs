using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [MaxLength(200)]
        public string? TransactionId { get; set; }

        public DateTime? PaidAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public int OrderId { get; set; }

        public Order Order { get; set; }
    }
}