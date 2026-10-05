using DAL.Data;
using DAL.DTO;
using DAL.Models;
using DAL.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Ecommerce_Test
{
    public class UserDaoTest
    {
        private AppDbContext _context;
        private Mock<UserManager<User>> _userManagerMock;
        private UserDAO _userDAO;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            var storeMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                storeMock.Object, null, null, null, null, null, null, null, null);

            _userDAO = new UserDAO(_context, _userManagerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        private User SeedUser(string fullName = "John Doe", string email = "john@example.com", string? id = null)
        {
            var user = new User
            {
                Id = id ?? Guid.NewGuid().ToString(),
                FullName = fullName,
                Email = email,
                UserName = email,
                NormalizedUserName = email.ToUpperInvariant(),
                NormalizedEmail = email.ToUpperInvariant(),
                Address = "123 Street",
                PhoneNumber = "1234567890",
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

        // -------------------- CreateUserAsync --------------------

        [Test]
        public async Task CreateUserAsync_ValidData_CallsCreateAsyncWithMappedUser()
        {
            var model = new CreateUserVM
            {
                FullName = "Jane Doe",
                Email = "jane@example.com",
                Address = "456 Avenue",
                PhoneNumber = "9998887777",
                Password = "Password123!"
            };
            _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email))
                .ReturnsAsync((User)null!);
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            await _userDAO.CreateUserAsync(model);

            _userManagerMock.Verify(m => m.CreateAsync(
                It.Is<User>(u => u.Email == model.Email && u.FullName == model.FullName && u.UserName == model.Email),
                model.Password), Times.Once());
        }

        [Test]
        public void CreateUserAsync_NullModel_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _userDAO.CreateUserAsync(null!));
        }

        [Test]
        public void CreateUserAsync_ExistingEmail_ThrowsInvalidOperationException()
        {
            var model = new CreateUserVM
            {
                FullName = "Jane Doe",
                Email = "jane@example.com",
                Address = "456 Avenue",
                PhoneNumber = "9998887777",
                Password = "Password123!"
            };
            var existingUser = new User { Id = Guid.NewGuid().ToString(), Email = model.Email };
            _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email))
                .ReturnsAsync(existingUser);

            Assert.ThrowsAsync<InvalidOperationException>(() => _userDAO.CreateUserAsync(model));
        }

        // -------------------- GetAllUsersAsync --------------------

        [Test]
        public async Task GetAllUsersAsync_NoUsers_ReturnsEmptyList()
        {
            var result = await _userDAO.GetAllUsersAsync();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllUsersAsync_MultipleUsers_ReturnsAllUsers()
        {
            SeedUser("John Doe", "john@example.com");
            SeedUser("Jane Smith", "jane.smith@example.com");
            SeedUser("Bob Lee", "bob.lee@example.com");

            var result = await _userDAO.GetAllUsersAsync();

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task GetAllUsersAsync_MapsFieldsCorrectly()
        {
            SeedUser("John Doe", "john@example.com");
            SeedUser("Jane Smith", "jane.smith@example.com");

            var result = await _userDAO.GetAllUsersAsync();
            var john = result.First(u => u.Email == "john@example.com");

            Assert.That(john.FullName, Is.EqualTo("John Doe"));
            Assert.That(john.Address, Is.EqualTo("123 Street"));
            Assert.That(john.PhoneNumber, Is.EqualTo("1234567890"));
        }

        // -------------------- GetUserByIdAsync --------------------

        [Test]
        public async Task GetUserByIdAsync_ExistingId_ReturnsMappedUser()
        {
            var user = new User { Id = "user-1", FullName = "John Doe", Email = "john@example.com", Address = "123 Street", PhoneNumber = "1234567890" };
            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);

            var result = await _userDAO.GetUserByIdAsync("user-1");

            Assert.That(result.FullName, Is.EqualTo("John Doe"));
            Assert.That(result.Email, Is.EqualTo("john@example.com"));
        }

        [Test]
        public void GetUserByIdAsync_EmptyId_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _userDAO.GetUserByIdAsync(""));
        }

        [Test]
        public void GetUserByIdAsync_NonExistentId_ThrowsKeyNotFoundException()
        {
            _userManagerMock.Setup(m => m.FindByIdAsync("missing-id")).ReturnsAsync((User)null!);

            Assert.ThrowsAsync<KeyNotFoundException>(() => _userDAO.GetUserByIdAsync("missing-id"));
        }

        // -------------------- UpdateUserByIdAsync --------------------

        [Test]
        public async Task UpdateUserByIdAsync_ValidData_UpdatesUserFields()
        {
            var existingUser = new User { Id = "user-1", FullName = "John Doe", Email = "john@example.com", UserName = "john@example.com" };
            var model = new UserVM { FullName = "John Updated", Email = "john@example.com", Address = "New Address", PhoneNumber = "5551234567" };

            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(existingUser);
            _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync((User)null!);
            _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

            await _userDAO.UpdateUserByIdAsync("user-1", model);

            Assert.That(existingUser.FullName, Is.EqualTo("John Updated"));
            Assert.That(existingUser.Address, Is.EqualTo("New Address"));
            Assert.That(existingUser.PhoneNumber, Is.EqualTo("5551234567"));
        }

        [Test]
        public void UpdateUserByIdAsync_NonExistentUser_ThrowsKeyNotFoundException()
        {
            var model = new UserVM { FullName = "John Updated", Email = "john@example.com" };
            _userManagerMock.Setup(m => m.FindByIdAsync("missing-id")).ReturnsAsync((User)null!);

            Assert.ThrowsAsync<KeyNotFoundException>(() => _userDAO.UpdateUserByIdAsync("missing-id", model));
        }

        [Test]
        public void UpdateUserByIdAsync_EmailUsedByAnotherUser_ThrowsInvalidOperationException()
        {
            var existingUser = new User { Id = "user-1", FullName = "John Doe", Email = "john@example.com" };
            var otherUser = new User { Id = "user-2", FullName = "Jane Smith", Email = "jane@example.com" };
            var model = new UserVM { FullName = "John Updated", Email = "jane@example.com" };

            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(existingUser);
            _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync(otherUser);

            Assert.ThrowsAsync<InvalidOperationException>(() => _userDAO.UpdateUserByIdAsync("user-1", model));
        }

        // -------------------- DeleteUserByIdAsync --------------------

        [Test]
        public async Task DeleteUserByIdAsync_ValidId_CallsDeleteAsync()
        {
            var user = new User { Id = "user-1", Email = "john@example.com" };
            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            await _userDAO.DeleteUserByIdAsync("user-1");

            _userManagerMock.Verify(m => m.DeleteAsync(user), Times.Once());
        }

        [Test]
        public void DeleteUserByIdAsync_EmptyId_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _userDAO.DeleteUserByIdAsync(""));
        }

        [Test]
        public void DeleteUserByIdAsync_NonExistentId_ThrowsKeyNotFoundException()
        {
            _userManagerMock.Setup(m => m.FindByIdAsync("missing-id")).ReturnsAsync((User)null!);

            Assert.ThrowsAsync<KeyNotFoundException>(() => _userDAO.DeleteUserByIdAsync("missing-id"));
        }

        // -------------------- GetUserOrderById --------------------

        [Test]
        public async Task GetUserOrderById_UserWithOrders_ReturnsOrders()
        {
            var user = SeedUser();
            SeedOrder(user.Id, OrderStatus.Pending, 100m);
            SeedOrder(user.Id, OrderStatus.Paid, 250m);

            var result = await _userDAO.GetUserOrderById(user.Id);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(o => o.UserId == user.Id), Is.True);
        }

        [Test]
        public void GetUserOrderById_EmptyId_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _userDAO.GetUserOrderById(""));
        }

        [Test]
        public async Task GetUserOrderById_UserWithNoOrders_ReturnsEmptyList()
        {
            var user = SeedUser();
            var otherUser = SeedUser("Other User", "other@example.com");
            SeedOrder(otherUser.Id);

            var result = await _userDAO.GetUserOrderById(user.Id);

            Assert.That(result, Is.Empty);
        }
    }
}
