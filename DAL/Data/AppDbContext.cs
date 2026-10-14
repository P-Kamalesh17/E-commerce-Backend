using DAL.Data;
using DAL.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Order_Item> Order_Items { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Category Configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.CategoryName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.HasIndex(x => x.CategoryName)
                    .IsUnique();
            });

            // Product Configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(x => x.Price)
                    .HasPrecision(18, 2);
                entity.HasOne(x => x.Category)
                    .WithMany(x => x.Products)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Order Configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TotalAmount)
                    .HasPrecision(18, 2);
                entity.HasOne(x => x.User)
                    .WithMany(x => x.Orders)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Order_Item Configuration
            modelBuilder.Entity<Order_Item>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UnitPrice)
                    .HasPrecision(18, 2);
                entity.Property(x => x.Subtotal)
                    .HasPrecision(18, 2);
                
                entity.HasOne(x => x.Product)
                    .WithMany(x => x.OrderItems)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(x => x.Order)
                    .WithMany(x => x.OrderItems)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Payment Configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Amount)
                    .HasPrecision(18, 2);

                entity.HasOne(x => x.Order)
                    .WithOne(x => x.Payment)
                    .HasForeignKey<Payment>(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cart Configuration
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UserId)
                    .IsRequired();
                entity.HasIndex(x => x.UserId)
                    .IsUnique();
            });

            // CartItem Configuration
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Cart)
                    .WithMany(x => x.CartItems)
                    .HasForeignKey(x => x.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // RefreshToken Configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}