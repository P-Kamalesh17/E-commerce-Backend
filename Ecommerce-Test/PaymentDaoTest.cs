using DAL.Data;
using DAL.DTO;
using DAL.Models;
using DAL.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Test
{
    public class PaymentDaoTest
    {
        private AppDbContext _context;
        private PaymentDAO _paymentDAO;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _paymentDAO = new PaymentDAO(_context);
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

        private Order SeedOrder(string userId, OrderStatus status = OrderStatus.Pending, decimal totalAmount = 100m)
        {
            var order = new Order
            {
                UserId = userId,
                Status = status,
                ShippingAddress = "123 Main St",
                TotalAmount = totalAmount,
                CreatedAt = DateTime.UtcNow
            };
            _context.Orders.Add(order);
            _context.SaveChanges();
            return order;
        }

        private Payment SeedPayment(int orderId, decimal amount = 100m, PaymentStatus status = PaymentStatus.Pending, string? transactionId = null)
        {
            var payment = new Payment
            {
                OrderId = orderId,
                Amount = amount,
                Status = status,
                TransactionId = transactionId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Payments.Add(payment);
            _context.SaveChanges();
            return payment;
        }

        // -------------------- GetAllPayments --------------------

        [Test]
        public async Task GetAllPayments_NoPayments_ReturnsEmptyList()
        {
            var result = await _paymentDAO.GetAllPayments();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllPayments_MultiplePayments_ReturnsAll()
        {
            var user = SeedUser();
            var order1 = SeedOrder(user.Id);
            var order2 = SeedOrder(user.Id);
            SeedPayment(order1.Id, 100m);
            SeedPayment(order2.Id, 200m);

            var result = await _paymentDAO.GetAllPayments();

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllPayments_ReturnsCorrectMappedFields()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id);
            SeedPayment(order.Id, 150m, PaymentStatus.Success, "TXN123");

            var result = await _paymentDAO.GetAllPayments();

            Assert.That(result[0].UserId, Is.EqualTo(user.Id));
            Assert.That(result[0].Amount, Is.EqualTo(150m));
            Assert.That(result[0].Status, Is.EqualTo(PaymentStatus.Success.ToString()));
        }

        // -------------------- GetPaymentById --------------------

        [Test]
        public async Task GetPaymentById_ExistingId_ReturnsCorrectPayment()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id);
            var payment = SeedPayment(order.Id, 120m, PaymentStatus.Pending, "TXN001");

            var result = await _paymentDAO.GetPaymentById(payment.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Amount, Is.EqualTo(120m));
            Assert.That(result.TransactionId, Is.EqualTo("TXN001"));
        }

        [Test]
        public async Task GetPaymentById_NonExistentId_ReturnsNull()
        {
            var result = await _paymentDAO.GetPaymentById(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetPaymentById_MultiplePayments_ReturnsOnlyRequestedOne()
        {
            var user = SeedUser();
            var order1 = SeedOrder(user.Id);
            var order2 = SeedOrder(user.Id);
            SeedPayment(order1.Id, 100m);
            var targetPayment = SeedPayment(order2.Id, 200m);

            var result = await _paymentDAO.GetPaymentById(targetPayment.Id);

            Assert.That(result!.Id, Is.EqualTo(targetPayment.Id));
            Assert.That(result.Amount, Is.EqualTo(200m));
        }

        // -------------------- GetPaymentByOrderId --------------------

        [Test]
        public async Task GetPaymentByOrderId_ExistingOrderId_ReturnsCorrectPayment()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id);
            SeedPayment(order.Id, 130m);

            var result = await _paymentDAO.GetPaymentByOrderId(order.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Amount, Is.EqualTo(130m));
        }

        [Test]
        public async Task GetPaymentByOrderId_NonExistentOrderId_ReturnsNull()
        {
            var result = await _paymentDAO.GetPaymentByOrderId(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetPaymentByOrderId_OrderWithoutPayment_ReturnsNull()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id);

            var result = await _paymentDAO.GetPaymentByOrderId(order.Id);

            Assert.That(result, Is.Null);
        }

        // -------------------- GetPaymentsByUserId --------------------

        [Test]
        public async Task GetPaymentsByUserId_ExistingUser_ReturnsUserPayments()
        {
            var user = SeedUser();
            var order1 = SeedOrder(user.Id);
            var order2 = SeedOrder(user.Id);
            SeedPayment(order1.Id, 100m);
            SeedPayment(order2.Id, 200m);

            var result = await _paymentDAO.GetPaymentsByUserId(user.Id);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetPaymentsByUserId_UserWithNoPayments_ReturnsEmptyList()
        {
            var user = SeedUser();

            var result = await _paymentDAO.GetPaymentsByUserId(user.Id);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetPaymentsByUserId_MultipleUsers_ReturnsOnlyRequestedUsersPayments()
        {
            var user1 = SeedUser("John Doe", "john@example.com");
            var user2 = SeedUser("Jane Smith", "jane@example.com");
            var order1 = SeedOrder(user1.Id);
            var order2 = SeedOrder(user2.Id);
            SeedPayment(order1.Id, 100m);
            SeedPayment(order2.Id, 200m);

            var result = await _paymentDAO.GetPaymentsByUserId(user2.Id);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].UserId, Is.EqualTo(user2.Id));
        }

        // -------------------- CreatePayment --------------------

        [Test]
        public async Task CreatePayment_ValidData_CreatesPaymentSuccessfully()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id, OrderStatus.Pending, 250m);
            var createVM = new CreatePaymentVM { OrderId = order.Id, TransactionId = "TXN999" };

            var result = await _paymentDAO.CreatePayment(createVM);

            Assert.That(result.Amount, Is.EqualTo(250m));
            Assert.That(result.Status, Is.EqualTo(PaymentStatus.Pending.ToString()));
        }

        [Test]
        public void CreatePayment_NullPayment_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _paymentDAO.CreatePayment(null!));
        }

        [Test]
        public async Task CreatePayment_OrderAlreadyHasPayment_ThrowsInvalidOperationException()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id);
            SeedPayment(order.Id);
            var createVM = new CreatePaymentVM { OrderId = order.Id };

            Assert.ThrowsAsync<InvalidOperationException>(() => _paymentDAO.CreatePayment(createVM));
        }

        // -------------------- ConfirmPayment --------------------

        [Test]
        public async Task ConfirmPayment_ValidPendingPayment_ConfirmsAndUpdatesOrderStatus()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id, OrderStatus.Pending);
            var payment = SeedPayment(order.Id, 100m, PaymentStatus.Pending);

            var result = await _paymentDAO.ConfirmPayment(payment.Id);

            Assert.That(result.Status, Is.EqualTo(PaymentStatus.Success.ToString()));
            var updatedOrder = await _context.Orders.FindAsync(order.Id);
            Assert.That(updatedOrder!.Status, Is.EqualTo(OrderStatus.Paid));
        }

        [Test]
        public void ConfirmPayment_NonExistentPayment_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _paymentDAO.ConfirmPayment(999));
        }

        [Test]
        public async Task ConfirmPayment_NonPendingPayment_ThrowsInvalidOperationException()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id);
            var payment = SeedPayment(order.Id, 100m, PaymentStatus.Success);

            Assert.ThrowsAsync<InvalidOperationException>(() => _paymentDAO.ConfirmPayment(payment.Id));
        }

        // -------------------- FailPayment --------------------

        [Test]
        public async Task FailPayment_ValidPendingPayment_MarksAsFailed()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id);
            var payment = SeedPayment(order.Id, 100m, PaymentStatus.Pending);

            var result = await _paymentDAO.FailPayment(payment.Id);

            Assert.That(result.Status, Is.EqualTo(PaymentStatus.Failed.ToString()));
        }

        [Test]
        public void FailPayment_NonExistentPayment_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _paymentDAO.FailPayment(999));
        }

        [Test]
        public async Task FailPayment_NonPendingPayment_ThrowsInvalidOperationException()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id);
            var payment = SeedPayment(order.Id, 100m, PaymentStatus.Success);

            Assert.ThrowsAsync<InvalidOperationException>(() => _paymentDAO.FailPayment(payment.Id));
        }

        // -------------------- RefundPayment --------------------

        [Test]
        public async Task RefundPayment_ValidSuccessfulPayment_RefundsPayment()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id);
            var payment = SeedPayment(order.Id, 100m, PaymentStatus.Success);

            var result = await _paymentDAO.RefundPayment(payment.Id);

            Assert.That(result.Status, Is.EqualTo(PaymentStatus.Refunded.ToString()));
        }

        [Test]
        public void RefundPayment_NonExistentPayment_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _paymentDAO.RefundPayment(999));
        }

        [Test]
        public async Task RefundPayment_NonSuccessfulPayment_ThrowsInvalidOperationException()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id);
            var payment = SeedPayment(order.Id, 100m, PaymentStatus.Pending);

            Assert.ThrowsAsync<InvalidOperationException>(() => _paymentDAO.RefundPayment(payment.Id));
        }

        // -------------------- UpdatePaymentStatus --------------------

        [Test]
        public async Task UpdatePaymentStatus_ValidData_UpdatesStatus()
        {
            var user = SeedUser();
            var order = SeedOrder(user.Id);
            var payment = SeedPayment(order.Id, 100m, PaymentStatus.Pending);
            var updateVM = new UpdatePaymentStatusVM { Status = PaymentStatus.Success, TransactionId = "TXN555" };

            var result = await _paymentDAO.UpdatePaymentStatus(payment.Id, updateVM);

            Assert.That(result.Status, Is.EqualTo(PaymentStatus.Success.ToString()));
            Assert.That(result.TransactionId, Is.EqualTo("TXN555"));
        }

        [Test]
        public void UpdatePaymentStatus_NullStatusVM_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _paymentDAO.UpdatePaymentStatus(1, null!));
        }

        [Test]
        public void UpdatePaymentStatus_NonExistentPayment_ThrowsKeyNotFoundException()
        {
            var updateVM = new UpdatePaymentStatusVM { Status = PaymentStatus.Success };

            Assert.ThrowsAsync<KeyNotFoundException>(() => _paymentDAO.UpdatePaymentStatus(999, updateVM));
        }
    }
}
