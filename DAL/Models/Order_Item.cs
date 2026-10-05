using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class Order_Item
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        // Foreign Keys
        public int ProductId { get; set; }

        public int OrderId { get; set; }

        // Navigation Properties
        public Product Product { get; set; }

        public Order Order { get; set; }
    }
}