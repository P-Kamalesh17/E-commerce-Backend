using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class User : IdentityUser
    {
        [MaxLength(200)]
        public string? FullName { get; set; }

        [MaxLength(1000)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}