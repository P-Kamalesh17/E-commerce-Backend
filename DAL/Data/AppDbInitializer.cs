using DAL.Data;
using DAL.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.Data
{
    public class AppDbInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<AppDbContext>();

                // Seed Users
                if (!context.Users.Any())
                {
                    context.Users.AddRange(
                        new User()
                        {
                            UserName = "admin@example.com",
                            Email = "admin@example.com",
                            FullName = "Admin User",
                            Address = "123 Admin Street",
                            CreatedAt = DateTime.UtcNow
                        },
                        new User()
                        {
                            UserName = "customer@example.com",
                            Email = "customer@example.com",
                            FullName = "Customer User",
                            Address = "456 Customer Avenue",
                            CreatedAt = DateTime.UtcNow
                        }
                    );
                    context.SaveChanges();
                }

                // Seed Categories
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category()
                        {
                            CategoryName = "Electronics"
                        },
                        new Category()
                        {
                            CategoryName = "Clothing"
                        }
                    );
                    context.SaveChanges();
                }

                // Seed Products
                if (!context.Products.Any())
                {
                    context.Products.AddRange(
                        new Product()
                        {
                            Name = "iPhone 15 Pro",
                            Description = "Latest Apple smartphone with advanced features",
                            Price = 999.99m,
                            StockQuantity = 50,
                            CategoryId = 1,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Product()
                        {
                            Name = "Cotton T-Shirt",
                            Description = "Comfortable and durable cotton t-shirt",
                            Price = 29.99m,
                            StockQuantity = 100,
                            CategoryId = 2,
                            CreatedAt = DateTime.UtcNow
                        }
                    );
                    context.SaveChanges();
                }

                // Seed Orders
                if (!context.Orders.Any())
                {
                    var user1 = context.Users.FirstOrDefault(u => u.Email == "admin@example.com");
                    var user2 = context.Users.FirstOrDefault(u => u.Email == "customer@example.com");

                    context.Orders.AddRange(
                        new Order()
                        {
                            UserId = user1.Id,
                            Status = OrderStatus.Pending,
                            ShippingAddress = "123 Admin Street, City, State 12345",
                            TotalAmount = 1029.98m,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Order()
                        {
                            UserId = user2.Id,
                            Status = OrderStatus.Paid,
                            ShippingAddress = "456 Customer Avenue, City, State 67890",
                            TotalAmount = 59.98m,
                            CreatedAt = DateTime.UtcNow.AddDays(-5)
                        }
                    );
                    context.SaveChanges();
                }

                // Seed Order Items
                if (!context.Order_Items.Any())
                {
                    context.Order_Items.AddRange(
                        new Order_Item()
                        {
                            OrderId = 1,
                            ProductId = 1,
                            Quantity = 1,
                            UnitPrice = 999.99m,
                            Subtotal = 999.99m
                        },
                        new Order_Item()
                        {
                            OrderId = 2,
                            ProductId = 2,
                            Quantity = 2,
                            UnitPrice = 29.99m,
                            Subtotal = 59.98m
                        }
                    );
                    context.SaveChanges();
                }

                // Seed Payments
                if (!context.Payments.Any())
                {
                    context.Payments.AddRange(
                        new Payment()
                        {
                            OrderId = 1,
                            Amount = 1029.98m,
                            Status = PaymentStatus.Pending,
                            TransactionId = "TXN-001",
                            CreatedAt = DateTime.UtcNow
                        },
                        new Payment()
                        {
                            OrderId = 2,
                            Amount = 59.98m,
                            Status = PaymentStatus.Success,
                            TransactionId = "TXN-002",
                            PaidAt = DateTime.UtcNow.AddDays(-5),
                            CreatedAt = DateTime.UtcNow.AddDays(-5)
                        }
                    );
                    context.SaveChanges();
                }
            }
        }
    }
}