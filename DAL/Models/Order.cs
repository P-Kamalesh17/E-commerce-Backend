using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Foreign Key
        [Required]
        public string UserId { get; set; } = string.Empty;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        [MaxLength(1000)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public User User { get; set; }

        public List<Order_Item> OrderItems { get; set; } = new List<Order_Item>();

        public Payment? Payment { get; set; }
    }
}