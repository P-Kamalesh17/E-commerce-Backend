using DAL.Data;
using DAL.Models;
using DAL.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Test
{
    public class OrderItemDaoTest
    {
        private AppDbContext _context;
        private OrderItemDAO _orderItemDAO;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _orderItemDAO = new OrderItemDAO(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        private User SeedUser(string fullName = "John Doe", string email = "john@example.com")
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                FullName = fullName,
                Email = email,
                UserName = email,
                NormalizedUserName = email.ToUpperInvariant(),
                NormalizedEmail = email.ToUpperInvariant(),
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        private Category SeedCategory(string name = "Electronics")
        {
            var category = new Category { CategoryName = name };
            _context.Categories.Add(category);
            _context.SaveChanges();
            return category;
        }

        private Product SeedProduct(int categoryId, string name = "Phone", decimal price = 100m, int stock = 10)
        {
            var product = new Product
            {
                Name = name,
                CategoryId = categoryId,
                Description = "Sample description",
                Price = price,
                StockQuantity = stock,
                CreatedAt = DateTime.UtcNow
            };
            _context.Products.Add(product);
            _context.SaveChanges();
            return product;
        }

        private Order SeedOrder(string userId, OrderStatus status = OrderStatus.Pending)
        {
            var order = new Order
            {
                UserId = userId,
                Status = status,
                ShippingAddress = "123 Main St",
                TotalAmount = 100m,
                CreatedAt = DateTime.UtcNow
            };
            _context.Orders.Add(order);
            _context.SaveChanges();
            return order;
        }

        private Order_Item SeedOrderItem(int orderId, int productId, int quantity = 1, decimal unitPrice = 100m)
        {
            var item = new Order_Item
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Subtotal = unitPrice * quantity
            };
            _context.Order_Items.Add(item);
            _context.SaveChanges();
            return item;
        }

        // -------------------- GetOrderItemsByOrderId --------------------

        [Test]
        public async Task GetOrderItemsByOrderId_ValidOrderWithItems_ReturnsAllItems()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id, "Phone", price: 50m);
            var order = SeedOrder(user.Id);
            SeedOrderItem(order.Id, product.Id, 2, 50m);
            SeedOrderItem(order.Id, product.Id, 1, 50m);

            var result = await _orderItemDAO.GetOrderItemsByOrderId(order.Id);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(oi => oi.ProductName == "Phone"), Is.True);
        }

        [Test]
        public void GetOrderItemsByOrderId_NonExistentOrder_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _orderItemDAO.GetOrderItemsByOrderId(999));
        }

        [Test]
        public async Task GetOrderItemsByOrderId_OrderWithNoItems_ReturnsEmptyList()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id);

            var result = await _orderItemDAO.GetOrderItemsByOrderId(order.Id);

            Assert.That(result, Is.Empty);
        }

        // -------------------- GetOrderItemById --------------------

        [Test]
        public async Task GetOrderItemById_ExistingId_ReturnsCorrectItem()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id, "Headphones", price: 80m);
            var order = SeedOrder(user.Id);
            var item = SeedOrderItem(order.Id, product.Id, 3, 80m);

            var result = await _orderItemDAO.GetOrderItemById(item.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ProductName, Is.EqualTo("Headphones"));
            Assert.That(result.Quantity, Is.EqualTo(3));
            Assert.That(result.Subtotal, Is.EqualTo(240m));
        }

        [Test]
        public async Task GetOrderItemById_NonExistentId_ReturnsNull()
        {
            var result = await _orderItemDAO.GetOrderItemById(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetOrderItemById_MultipleItems_ReturnsOnlyRequestedOne()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id, "Charger", price: 20m);
            var order = SeedOrder(user.Id);
            SeedOrderItem(order.Id, product.Id, 1, 20m);
            var targetItem = SeedOrderItem(order.Id, product.Id, 2, 20m);

            var result = await _orderItemDAO.GetOrderItemById(targetItem.Id);

            Assert.That(result!.Id, Is.EqualTo(targetItem.Id));
            Assert.That(result.Quantity, Is.EqualTo(2));
        }
    }
}
