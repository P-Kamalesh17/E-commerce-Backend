using DAL.Data;
using DAL.DTO;
using DAL.Models;
using DAL.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Test
{
    public class CartDaoTest
    {
        private AppDbContext _context;
        private CartDAO _cartDAO;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _cartDAO = new CartDAO(_context);
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

        private Cart SeedCart(string userId)
        {
            var cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
            _context.Carts.Add(cart);
            _context.SaveChanges();
            return cart;
        }

        private CartItem SeedCartItem(int cartId, int productId, int quantity = 1)
        {
            var item = new CartItem
            {
                CartId = cartId,
                ProductId = productId,
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow
            };
            _context.CartItems.Add(item);
            _context.SaveChanges();
            return item;
        }

        // -------------------- GetCartByUserId --------------------

        [Test]
        public async Task GetCartByUserId_ExistingCartWithItems_ReturnsMappedCart()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id);
            var cart = SeedCart(user.Id);
            SeedCartItem(cart.Id, product.Id, 2);

            var result = await _cartDAO.GetCartByUserId(user.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetCartByUserId_NoCartForUser_ReturnsNull()
        {
            var result = await _cartDAO.GetCartByUserId("non-existent-user");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetCartByUserId_MapsItemFieldsCorrectly()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id, "Headphones", price: 80m);
            var cart = SeedCart(user.Id);
            SeedCartItem(cart.Id, product.Id, 3);

            var result = await _cartDAO.GetCartByUserId(user.Id);

            Assert.That(result!.Items[0].ProductName, Is.EqualTo("Headphones"));
            Assert.That(result.Items[0].UnitPrice, Is.EqualTo(80m));
            Assert.That(result.Items[0].Subtotal, Is.EqualTo(240m));
        }

        // -------------------- GetCartSummary --------------------

        [Test]
        public async Task GetCartSummary_ValidCart_ReturnsCorrectTotal()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id, "Phone", price: 100m);
            var cart = SeedCart(user.Id);
            SeedCartItem(cart.Id, product.Id, 2);

            var result = await _cartDAO.GetCartSummary(user.Id);

            Assert.That(result.Subtotal, Is.EqualTo(200m));
            Assert.That(result.TotalAmount, Is.EqualTo(200m));
        }

        [Test]
        public void GetCartSummary_NoCartForUser_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _cartDAO.GetCartSummary("non-existent-user"));
        }

        [Test]
        public async Task GetCartSummary_MultipleItems_SumsSubtotalCorrectly()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product1 = SeedProduct(category.Id, "Phone", price: 100m);
            var product2 = SeedProduct(category.Id, "Charger", price: 20m);
            var cart = SeedCart(user.Id);
            SeedCartItem(cart.Id, product1.Id, 1);
            SeedCartItem(cart.Id, product2.Id, 3);

            var result = await _cartDAO.GetCartSummary(user.Id);

            Assert.That(result.Subtotal, Is.EqualTo(160m));
        }

        // -------------------- AddItem --------------------

        [Test]
        public async Task AddItem_NewUser_CreatesCartAndAddsItem()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id);
            var addItemVM = new AddCartItemVM { UserId = user.Id, ProductId = product.Id, Quantity = 2 };

            await _cartDAO.AddItem(addItemVM);

            var cart = await _context.Carts.Include(c => c.CartItems).FirstAsync(c => c.UserId == user.Id);
            Assert.That(cart.CartItems.Count, Is.EqualTo(1));
            Assert.That(cart.CartItems[0].Quantity, Is.EqualTo(2));
        }

        [Test]
        public async Task AddItem_ExistingProductInCart_IncrementsQuantity()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id);
            var cart = SeedCart(user.Id);
            SeedCartItem(cart.Id, product.Id, 2);
            var addItemVM = new AddCartItemVM { UserId = user.Id, ProductId = product.Id, Quantity = 3 };

            await _cartDAO.AddItem(addItemVM);

            var updatedCart = await _context.Carts.Include(c => c.CartItems).FirstAsync(c => c.Id == cart.Id);
            Assert.That(updatedCart.CartItems.Count, Is.EqualTo(1));
            Assert.That(updatedCart.CartItems[0].Quantity, Is.EqualTo(5));
        }

        [Test]
        public void AddItem_InvalidProductId_ThrowsKeyNotFoundException()
        {
            var user = SeedUser();
            var addItemVM = new AddCartItemVM { UserId = user.Id, ProductId = 999, Quantity = 1 };

            Assert.ThrowsAsync<KeyNotFoundException>(() => _cartDAO.AddItem(addItemVM));
        }

        // -------------------- UpdateItemQuantity --------------------

        [Test]
        public async Task UpdateItemQuantity_ValidData_UpdatesQuantity()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id);
            var cart = SeedCart(user.Id);
            var cartItem = SeedCartItem(cart.Id, product.Id, 1);
            var updateVM = new UpdateCartItemVM { Quantity = 5 };

            await _cartDAO.UpdateItemQuantity(cartItem.Id, updateVM);

            var updatedItem = await _context.CartItems.FindAsync(cartItem.Id);
            Assert.That(updatedItem!.Quantity, Is.EqualTo(5));
        }

        [Test]
        public void UpdateItemQuantity_NonExistentCartItem_ThrowsKeyNotFoundException()
        {
            var updateVM = new UpdateCartItemVM { Quantity = 5 };

            Assert.ThrowsAsync<KeyNotFoundException>(() => _cartDAO.UpdateItemQuantity(999, updateVM));
        }

        [Test]
        public async Task UpdateItemQuantity_ZeroQuantity_ThrowsArgumentException()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id);
            var cart = SeedCart(user.Id);
            var cartItem = SeedCartItem(cart.Id, product.Id, 1);
            var updateVM = new UpdateCartItemVM { Quantity = 0 };

            Assert.ThrowsAsync<ArgumentException>(() => _cartDAO.UpdateItemQuantity(cartItem.Id, updateVM));
        }

        // -------------------- RemoveItem --------------------

        [Test]
        public async Task RemoveItem_ValidCartItem_RemovesItem()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id);
            var cart = SeedCart(user.Id);
            var cartItem = SeedCartItem(cart.Id, product.Id, 1);

            await _cartDAO.RemoveItem(cartItem.Id);

            var items = await _context.CartItems.ToListAsync();
            Assert.That(items, Is.Empty);
        }

        [Test]
        public void RemoveItem_NonExistentCartItem_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _cartDAO.RemoveItem(999));
        }

        [Test]
        public async Task RemoveItem_DoesNotAffectOtherItems()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id);
            var cart = SeedCart(user.Id);
            var itemToRemove = SeedCartItem(cart.Id, product.Id, 1);
            var itemToKeep = SeedCartItem(cart.Id, SeedProduct(category.Id, "Charger").Id, 2);

            await _cartDAO.RemoveItem(itemToRemove.Id);

            var items = await _context.CartItems.ToListAsync();
            Assert.That(items.Count, Is.EqualTo(1));
            Assert.That(items[0].Id, Is.EqualTo(itemToKeep.Id));
        }

        // -------------------- ClearCart --------------------

        [Test]
        public async Task ClearCart_ValidUser_RemovesAllItems()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id);
            var cart = SeedCart(user.Id);
            SeedCartItem(cart.Id, product.Id, 1);
            SeedCartItem(cart.Id, product.Id, 2);

            await _cartDAO.ClearCart(user.Id);

            var items = await _context.CartItems.ToListAsync();
            Assert.That(items, Is.Empty);
        }

        [Test]
        public void ClearCart_NonExistentUser_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _cartDAO.ClearCart("non-existent-user"));
        }

        [Test]
        public async Task ClearCart_CartItselfRemainsAfterClearing()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id);
            var cart = SeedCart(user.Id);
            SeedCartItem(cart.Id, product.Id, 1);

            await _cartDAO.ClearCart(user.Id);

            var remainingCart = await _context.Carts.FindAsync(cart.Id);
            Assert.That(remainingCart, Is.Not.Null);
        }

        // -------------------- Checkout --------------------

        [Test]
        public async Task Checkout_ValidCart_CreatesOrderAndClearsCart()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id, "Phone", price: 50m, stock: 10);
            var cart = SeedCart(user.Id);
            SeedCartItem(cart.Id, product.Id, 2);
            var checkoutVM = new CheckoutVM { UserId = user.Id, ShippingAddress = "123 Main St" };

            var result = await _cartDAO.Checkout(checkoutVM);

            Assert.That(result.TotalAmount, Is.EqualTo(100m));
            var remainingItems = await _context.CartItems.ToListAsync();
            Assert.That(remainingItems, Is.Empty);
            var updatedProduct = await _context.Products.FindAsync(product.Id);
            Assert.That(updatedProduct!.StockQuantity, Is.EqualTo(8));
        }

        [Test]
        public void Checkout_EmptyCart_ThrowsInvalidOperationException()
        {
            var user = SeedUser();
            SeedCart(user.Id);
            var checkoutVM = new CheckoutVM { UserId = user.Id, ShippingAddress = "123 Main St" };

            Assert.ThrowsAsync<InvalidOperationException>(() => _cartDAO.Checkout(checkoutVM));
        }

        [Test]
        public async Task Checkout_InsufficientStock_ThrowsInvalidOperationException()
        {
            var user = SeedUser();
            var category = SeedCategory();
            var product = SeedProduct(category.Id, "Phone", price: 50m, stock: 1);
            var cart = SeedCart(user.Id);
            SeedCartItem(cart.Id, product.Id, 5);
            var checkoutVM = new CheckoutVM { UserId = user.Id, ShippingAddress = "123 Main St" };

            Assert.ThrowsAsync<InvalidOperationException>(() => _cartDAO.Checkout(checkoutVM));
        }
    }
}
