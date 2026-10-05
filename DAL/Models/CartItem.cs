using System;

namespace DAL.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Foreign Keys
        public int CartId { get; set; }

        public int ProductId { get; set; }

        // Navigation Properties
        public Cart Cart { get; set; }

        public Product Product { get; set; }
    }
}
