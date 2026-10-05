using DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text;

namespace DAL.DTO
{
    public class OrderVM
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string OrderStatus { get; set; }
        public string ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string PaymentStatus { get; set; }
    }
    public class OrderDetailVM
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }

        public string OrderStatus { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<OrderItemDetailVM> OrderItems { get; set; } = new();

        public PaymentDetailVM? Payment { get; set; }
    }

    public class CreateOrderVM
    {
        public string UserId { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;

    }

    public class UpdateOrderVM
    {
        public string ShippingAddress { get; set; } = string.Empty;
    }

    public class UpdateOrderStatusVM
    {
        public OrderStatus Status { get; set; }
    }
}
