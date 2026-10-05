using DAL.Data;
using DAL.DTO;
using DAL.Models;
using DAL.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Test
{
    public class OrderDaoTest
    {
        private AppDbContext _context;
        private OrderDAO _orderDAO;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _orderDAO = new OrderDAO(_context);
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

        private Order SeedOrder(string userId, OrderStatus status = OrderStatus.Pending, string shippingAddress = "123 Main St", decimal totalAmount = 100m)
        {
            var order = new Order
            {
                UserId = userId,
                Status = status,
                ShippingAddress = shippingAddress,
                TotalAmount = totalAmount,
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

        private Cart SeedCart(string userId)
        {
            var cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            _context.SaveChanges();
            return cart;
        }

        private CartItem SeedCartItem(int cartId, int productId, int quantity = 1)
        {
            var cartItem = new CartItem
            {
                CartId = cartId,
                ProductId = productId,
                Quantity = quantity
            };
            _context.CartItems.Add(cartItem);
            _context.SaveChanges();
            return cartItem;
        }

        private Payment SeedPayment(int orderId, decimal amount = 100m, PaymentStatus status = PaymentStatus.Pending)
        {
            var payment = new Payment
            {
                OrderId = orderId,
                Amount = amount,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };
            _context.Payments.Add(payment);
            _context.SaveChanges();
            return payment;
        }

        // -------------------- GetAllOrders --------------------

        [Test]
        public async Task GetAllOrders_NoFilter_ReturnsAllOrders()
        {
            var user = SeedUser();
            SeedOrder(user.Id, OrderStatus.Pending);
            SeedOrder(user.Id, OrderStatus.Paid);
            SeedOrder(user.Id, OrderStatus.Shipped);

            var result = await _orderDAO.GetAllOrders(null);

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task GetAllOrders_FilterByStatus_ReturnsOnlyMatchingOrders()
        {
            var user = SeedUser();
            SeedOrder(user.Id, OrderStatus.Pending);
            SeedOrder(user.Id, OrderStatus.Pending);
            SeedOrder(user.Id, OrderStatus.Paid);

            var result = await _orderDAO.GetAllOrders(OrderStatus.Pending);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(o => o.OrderStatus == OrderStatus.Pending.ToString()), Is.True);
        }

        [Test]
        public async Task GetAllOrders_OrderWithoutPayment_MapsPaymentStatusAsNotPaid()
        {
            var user = SeedUser();
            SeedOrder(user.Id);

            var result = await _orderDAO.GetAllOrders(null);

            Assert.That(result[0].PaymentStatus, Is.EqualTo("Not Paid"));
        }

        // -------------------- GetOrderById --------------------

        [Test]
        public async Task GetOrderById_ExistingId_ReturnsOrderWithUserAndItems()
        {
            var user = SeedUser("Jane Smith", "jane@example.com");
            var category = SeedCategory();
            var product = SeedProduct(category.Id);
            var order = SeedOrder(user.Id);
            SeedOrderItem(order.Id, product.Id, 2, 100m);
            SeedOrderItem(order.Id, product.Id, 1, 100m);

            var result = await _orderDAO.GetOrderById(order.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.UserName, Is.EqualTo(user.UserName));
            Assert.That(result.OrderItems.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetOrderById_NonExistentId_ReturnsNull()
        {
            var result = await _orderDAO.GetOrderById(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetOrderById_OrderWithPayment_IncludesPaymentDetails()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id, OrderStatus.Paid);
            SeedPayment(order.Id, 150m, PaymentStatus.Success);

            var result = await _orderDAO.GetOrderById(order.Id);

            Assert.That(result!.Payment, Is.Not.Null);
            Assert.That(result.Payment!.Amount, Is.EqualTo(150m));
            Assert.That(result.Payment.Status, Is.EqualTo(PaymentStatus.Success.ToString()));
        }

        // -------------------- GetOrdersByStatus --------------------

        [Test]
        public async Task GetOrdersByStatus_MatchingStatus_ReturnsOnlyThoseOrders()
        {
            var user = SeedUser();
            SeedOrder(user.Id, OrderStatus.Shipped);
            SeedOrder(user.Id, OrderStatus.Pending);

            var result = await _orderDAO.GetOrdersByStatus(OrderStatus.Shipped);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].OrderStatus, Is.EqualTo(OrderStatus.Shipped.ToString()));
        }

        [Test]
        public async Task GetOrdersByStatus_NoMatchingStatus_ReturnsEmptyList()
        {
            var user = SeedUser();
            SeedOrder(user.Id, OrderStatus.Pending);

            var result = await _orderDAO.GetOrdersByStatus(OrderStatus.Delivered);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetOrdersByStatus_MultipleOrdersSameStatus_ReturnsAll()
        {
            var user = SeedUser();
            SeedOrder(user.Id, OrderStatus.Paid);
            SeedOrder(user.Id, OrderStatus.Paid);
            SeedOrder(user.Id, OrderStatus.Paid);

            var result = await _orderDAO.GetOrdersByStatus(OrderStatus.Paid);

            Assert.That(result.Count, Is.EqualTo(3));
        }

        // -------------------- GetOrdersByUserId --------------------

        [Test]
        public async Task GetOrdersByUserId_ExistingUser_ReturnsUserOrders()
        {
            var user = SeedUser();
            SeedOrder(user.Id);
            SeedOrder(user.Id);

            var result = await _orderDAO.GetOrdersByUserId(user.Id);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetOrdersByUserId_UserWithNoOrders_ReturnsEmptyList()
        {
            var user = SeedUser();

            var result = await _orderDAO.GetOrdersByUserId(user.Id);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetOrdersByUserId_MultipleUsers_ReturnsOnlyRequestedUsersOrders()
        {
            var user1 = SeedUser("John Doe", "john@example.com");
            var user2 = SeedUser("Jane Smith", "jane@example.com");
            SeedOrder(user1.Id);
            SeedOrder(user2.Id);
            SeedOrder(user2.Id);

            var result = await _orderDAO.GetOrdersByUserId(user2.Id);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(o => o!.UserId == user2.Id), Is.True);
        }

        // -------------------- CreateOrder --------------------

        [Test]
        public async Task CreateOrder_ValidCart_CreatesOrderAndDecrementsStock()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id, "Phone", price: 50m, stock: 10);
            var cart = SeedCart(user.Id);
            SeedCartItem(cart.Id, product.Id, 2);
            var createOrderVM = new CreateOrderVM
            {
                UserId = user.Id,
                ShippingAddress = "123 Main St"
            };

            var result = await _orderDAO.CreateOrder(createOrderVM);

            Assert.That(result.TotalAmount, Is.EqualTo(100m));
            var updatedProduct = await _context.Products.FindAsync(product.Id);
            Assert.That(updatedProduct!.StockQuantity, Is.EqualTo(8));
            var remainingCartItems = await _context.CartItems.Where(ci => ci.CartId == cart.Id).ToListAsync();
            Assert.That(remainingCartItems, Is.Empty);
        }

        [Test]
        public void CreateOrder_NullOrder_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _orderDAO.CreateOrder(null!));
        }

        [Test]
        public async Task CreateOrder_InsufficientStock_ThrowsInvalidOperationException()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id, "Phone", price: 50m, stock: 1);
            var cart = SeedCart(user.Id);
            SeedCartItem(cart.Id, product.Id, 5);
            var createOrderVM = new CreateOrderVM
            {
                UserId = user.Id,
                ShippingAddress = "123 Main St"
            };

            Assert.ThrowsAsync<InvalidOperationException>(() => _orderDAO.CreateOrder(createOrderVM));
        }

        [Test]
        public void CreateOrder_EmptyCart_ThrowsInvalidOperationException()
        {
            var user = SeedUser();
            SeedCart(user.Id);
            var createOrderVM = new CreateOrderVM
            {
                UserId = user.Id,
                ShippingAddress = "123 Main St"
            };

            Assert.ThrowsAsync<InvalidOperationException>(() => _orderDAO.CreateOrder(createOrderVM));
        }

        [Test]
        public void CreateOrder_NoCart_ThrowsInvalidOperationException()
        {
            var user = SeedUser();
            var createOrderVM = new CreateOrderVM
            {
                UserId = user.Id,
                ShippingAddress = "123 Main St"
            };

            Assert.ThrowsAsync<InvalidOperationException>(() => _orderDAO.CreateOrder(createOrderVM));
        }

        // -------------------- UpdateOrder --------------------

        [Test]
        public async Task UpdateOrder_ValidData_UpdatesShippingAddress()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id, OrderStatus.Pending);
            var updateVM = new UpdateOrderVM { ShippingAddress = "New Address" };

            var result = await _orderDAO.UpdateOrder(order.Id, updateVM);

            Assert.That(result.ShippingAddress, Is.EqualTo("New Address"));
        }

        [Test]
        public void UpdateOrder_NonExistentOrder_ThrowsKeyNotFoundException()
        {
            var updateVM = new UpdateOrderVM { ShippingAddress = "New Address" };

            Assert.ThrowsAsync<KeyNotFoundException>(() => _orderDAO.UpdateOrder(999, updateVM));
        }

        [Test]
        public async Task UpdateOrder_ShippedOrder_ThrowsInvalidOperationException()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id, OrderStatus.Shipped);
            var updateVM = new UpdateOrderVM { ShippingAddress = "New Address" };

            Assert.ThrowsAsync<InvalidOperationException>(() => _orderDAO.UpdateOrder(order.Id, updateVM));
        }

        // -------------------- UpdateOrderStatus --------------------

        [Test]
        public async Task UpdateOrderStatus_ValidData_UpdatesStatus()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id, OrderStatus.Pending);
            var updateVM = new UpdateOrderStatusVM { Status = OrderStatus.Paid };

            var result = await _orderDAO.UpdateOrderStatus(order.Id, updateVM);

            Assert.That(result.OrderStatus, Is.EqualTo(OrderStatus.Paid.ToString()));
        }

        [Test]
        public void UpdateOrderStatus_NonExistentOrder_ThrowsKeyNotFoundException()
        {
            var updateVM = new UpdateOrderStatusVM { Status = OrderStatus.Paid };

            Assert.ThrowsAsync<KeyNotFoundException>(() => _orderDAO.UpdateOrderStatus(999, updateVM));
        }

        [Test]
        public async Task UpdateOrderStatus_DeliveredOrder_ThrowsInvalidOperationException()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id, OrderStatus.Delivered);
            var updateVM = new UpdateOrderStatusVM { Status = OrderStatus.Cancelled };

            Assert.ThrowsAsync<InvalidOperationException>(() => _orderDAO.UpdateOrderStatus(order.Id, updateVM));
        }

        // -------------------- CancelOrder --------------------

        [Test]
        public async Task CancelOrder_ValidOrder_CancelsAndRestoresStock()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id, "Phone", price: 50m, stock: 5);
            var order = SeedOrder(user.Id, OrderStatus.Pending);
            SeedOrderItem(order.Id, product.Id, 2, 50m);

            var result = await _orderDAO.CancelOrder(order.Id);

            Assert.That(result.OrderStatus, Is.EqualTo(OrderStatus.Cancelled.ToString()));
            var updatedProduct = await _context.Products.FindAsync(product.Id);
            Assert.That(updatedProduct!.StockQuantity, Is.EqualTo(7));
        }

        [Test]
        public void CancelOrder_NonExistentOrder_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _orderDAO.CancelOrder(999));
        }

        [Test]
        public async Task CancelOrder_AlreadyCancelledOrder_ThrowsInvalidOperationException()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id, OrderStatus.Cancelled);

            Assert.ThrowsAsync<InvalidOperationException>(() => _orderDAO.CancelOrder(order.Id));
        }

        // -------------------- DeleteOrder --------------------

        [Test]
        public async Task DeleteOrder_PendingOrder_DeletesAndReturnsTrue()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id, OrderStatus.Pending);

            var result = await _orderDAO.DeleteOrder(order.Id);

            Assert.That(result, Is.True);
            var orders = await _context.Orders.ToListAsync();
            Assert.That(orders, Is.Empty);
        }

        [Test]
        public async Task DeleteOrder_NonExistentOrder_ReturnsFalse()
        {
            var result = await _orderDAO.DeleteOrder(999);

            Assert.That(result, Is.False);
        }

        [Test]
        public void DeleteOrder_ShippedOrder_ThrowsInvalidOperationException()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id, OrderStatus.Shipped);

            Assert.ThrowsAsync<InvalidOperationException>(() => _orderDAO.DeleteOrder(order.Id));
        }
    }
}
