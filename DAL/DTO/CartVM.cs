using System;
using System.Collections.Generic;

namespace DAL.DTO
{
    public class CartVM
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CartItemVM> Items { get; set; } = new List<CartItemVM>();
    }

    public class CartItemVM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CartSummaryVM
    {
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class AddCartItemVM
    {
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemVM
    {
        public int Quantity { get; set; }
    }

    public class CheckoutVM
    {
        public string UserId { get; set; }
        public string? ShippingAddress { get; set; }
    }
}
