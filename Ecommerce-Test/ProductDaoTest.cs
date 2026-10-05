using DAL.Data;
using DAL.DTO;
using DAL.Models;
using DAL.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Test
{
    public class ProductDaoTest
    {
        private AppDbContext _context;
        private ProductDAO _productDAO;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _productDAO = new ProductDAO(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
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

        // -------------------- CreateProduct --------------------

        [Test]
        public async Task CreateProduct_ValidData_AddsProductToDatabase()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Headphones");
            SeedProduct(category.Id, "Charger");
            var createProductVM = new CreateProductVM
            {
                Name = "Laptop",
                CategoryId = category.Id,
                Description = "Gaming laptop",
                Price = 1500m,
                StockQuantity = 5
            };

            await _productDAO.CreateProduct(createProductVM);

            var products = await _context.Products.ToListAsync();
            Assert.That(products.Count, Is.EqualTo(3));
            Assert.That(products.Any(p => p.Name == "Laptop"), Is.True);
        }

        [Test]
        public void CreateProduct_NullProduct_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _productDAO.CreateProduct(null));
        }

        [Test]
        public void CreateProduct_InvalidCategoryId_ThrowsKeyNotFoundException()
        {
            var createProductVM = new CreateProductVM
            {
                Name = "Laptop",
                CategoryId = 999,
                Description = "Gaming laptop",
                Price = 1500m,
                StockQuantity = 5
            };

            Assert.ThrowsAsync<KeyNotFoundException>(() => _productDAO.CreateProduct(createProductVM));
        }

        [Test]
        public async Task CreateProduct_ValidData_SetsCreatedAtTimestamp()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Phone");
            SeedProduct(category.Id, "Headphones");
            var createProductVM = new CreateProductVM
            {
                Name = "Tablet",
                CategoryId = category.Id,
                Description = "Small tablet",
                Price = 300m,
                StockQuantity = 8
            };

            await _productDAO.CreateProduct(createProductVM);

            var product = await _context.Products.FirstAsync(p => p.Name == "Tablet");
            Assert.That(product.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        }

        // -------------------- DeleteProductById --------------------

        [Test]
        public async Task DeleteProductById_ValidId_RemovesProduct()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Phone");
            var productToDelete = SeedProduct(category.Id, "Headphones");
            SeedProduct(category.Id, "Charger");

            await _productDAO.DeleteProductById(productToDelete.Id);

            var products = await _context.Products.ToListAsync();
            Assert.That(products.Count, Is.EqualTo(2));
            Assert.That(products.Any(p => p.Id == productToDelete.Id), Is.False);
        }

        [Test]
        public void DeleteProductById_InvalidId_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _productDAO.DeleteProductById(999));
        }

        [Test]
        public async Task DeleteProductById_ValidId_DoesNotRemoveOtherProducts()
        {
            var category = SeedCategory();
            var productToDelete = SeedProduct(category.Id, "Phone");
            var productToKeep1 = SeedProduct(category.Id, "Headphones");
            var productToKeep2 = SeedProduct(category.Id, "Charger");

            await _productDAO.DeleteProductById(productToDelete.Id);

            var remaining = await _context.Products.ToListAsync();
            Assert.That(remaining.Count, Is.EqualTo(2));
            Assert.That(remaining.Select(p => p.Id), Is.EquivalentTo(new[] { productToKeep1.Id, productToKeep2.Id }));
        }

        [Test]
        public async Task DeleteProductById_DecreasesProductCountByOne()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Phone");
            SeedProduct(category.Id, "Headphones");
            var productToDelete = SeedProduct(category.Id, "Charger");
            var countBefore = await _context.Products.CountAsync();

            await _productDAO.DeleteProductById(productToDelete.Id);

            var countAfter = await _context.Products.CountAsync();
            Assert.That(countAfter, Is.EqualTo(countBefore - 1));
        }

        // -------------------- GetAllProducts --------------------

        [Test]
        public async Task GetAllProducts_NoProducts_ReturnsEmptyList()
        {
            var result = await _productDAO.GetAllProducts();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllProducts_MultipleProducts_ReturnsAllProducts()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Phone");
            SeedProduct(category.Id, "Headphones");
            SeedProduct(category.Id, "Charger");

            var result = await _productDAO.GetAllProducts();

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task GetAllProducts_ReturnsCorrectCategoryName()
        {
            var category = SeedCategory("Audio");
            SeedProduct(category.Id, "Speaker");
            SeedProduct(category.Id, "Headphones");

            var result = await _productDAO.GetAllProducts();

            Assert.That(result.All(p => p.CategoryName == "Audio"), Is.True);
        }

        [Test]
        public async Task GetAllProducts_MapsAllFieldsCorrectly()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Headphones", price: 80m, stock: 15);
            SeedProduct(category.Id, "Phone", price: 250m, stock: 7);
            SeedProduct(category.Id, "Charger", price: 20m, stock: 30);

            var result = await _productDAO.GetAllProducts();
            var phone = result.First(p => p.Name == "Phone");

            Assert.That(phone.Price, Is.EqualTo(250m));
            Assert.That(phone.StockQuantity, Is.EqualTo(7));
        }

        // -------------------- GetProductByCategoryId --------------------

        [Test]
        public async Task GetProductByCategoryId_MatchingProducts_ReturnsOnlyThoseProducts()
        {
            var category1 = SeedCategory("Electronics");
            var category2 = SeedCategory("Furniture");
            SeedProduct(category1.Id, "Phone");
            SeedProduct(category1.Id, "Headphones");
            SeedProduct(category2.Id, "Chair");

            var result = await _productDAO.GetProductByCategoryId(category1.Id);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Select(p => p.Name), Is.EquivalentTo(new[] { "Phone", "Headphones" }));
        }

        [Test]
        public async Task GetProductByCategoryId_NoMatchingProducts_ReturnsEmptyList()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Phone");
            SeedProduct(category.Id, "Headphones");

            var result = await _productDAO.GetProductByCategoryId(999);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetProductByCategoryId_MultipleProductsInCategory_ReturnsAll()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Phone");
            SeedProduct(category.Id, "Headphones");
            SeedProduct(category.Id, "Charger");

            var result = await _productDAO.GetProductByCategoryId(category.Id);

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task GetProductByCategoryId_ReturnsCorrectCategoryId()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Phone");
            SeedProduct(category.Id, "Headphones");

            var result = await _productDAO.GetProductByCategoryId(category.Id);

            Assert.That(result.All(p => p.CategoryId == category.Id), Is.True);
        }

        // -------------------- GetProductById --------------------

        [Test]
        public async Task GetProductById_ExistingId_ReturnsCorrectProduct()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Headphones");
            var product = SeedProduct(category.Id, "Phone");
            SeedProduct(category.Id, "Charger");

            var result = await _productDAO.GetProductById(product.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Phone"));
        }

        [Test]
        public async Task GetProductById_NonExistentId_ReturnsNull()
        {
            var result = await _productDAO.GetProductById(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetProductById_ReturnsCorrectCategoryName()
        {
            var category = SeedCategory("Books");
            SeedProduct(category.Id, "Comic");
            var product = SeedProduct(category.Id, "Novel");

            var result = await _productDAO.GetProductById(product.Id);

            Assert.That(result!.CategoryName, Is.EqualTo("Books"));
        }

        [Test]
        public async Task GetProductById_MultipleProducts_ReturnsOnlyRequestedOne()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Phone");
            var targetProduct = SeedProduct(category.Id, "Headphones");
            SeedProduct(category.Id, "Charger");

            var result = await _productDAO.GetProductById(targetProduct.Id);

            Assert.That(result!.Id, Is.EqualTo(targetProduct.Id));
        }

        // -------------------- UpdateProductById --------------------

        [Test]
        public async Task UpdateProductById_ValidData_UpdatesProductFields()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Headphones");
            var product = SeedProduct(category.Id, "Phone", price: 100m, stock: 10);
            SeedProduct(category.Id, "Charger");
            var updateVM = new CreateProductVM
            {
                Name = "Updated Phone",
                CategoryId = category.Id,
                Description = "Updated description",
                Price = 200m,
                StockQuantity = 20
            };

            await _productDAO.UpdateProductById(product.Id, updateVM);

            var updated = await _context.Products.FindAsync(product.Id);
            Assert.That(updated!.Name, Is.EqualTo("Updated Phone"));
            Assert.That(updated.Price, Is.EqualTo(200m));
            Assert.That(updated.StockQuantity, Is.EqualTo(20));
        }

        [Test]
        public void UpdateProductById_NullProduct_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _productDAO.UpdateProductById(1, null));
        }

        [Test]
        public async Task UpdateProductById_InvalidProductId_ThrowsKeyNotFoundException()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Phone");
            SeedProduct(category.Id, "Headphones");
            var updateVM = new CreateProductVM
            {
                Name = "Updated Phone",
                CategoryId = category.Id,
                Description = "Updated description",
                Price = 200m,
                StockQuantity = 20
            };

            Assert.ThrowsAsync<KeyNotFoundException>(() => _productDAO.UpdateProductById(999, updateVM));
        }

        [Test]
        public async Task UpdateProductById_InvalidCategoryId_ThrowsKeyNotFoundException()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Headphones");
            var product = SeedProduct(category.Id, "Phone");
            var updateVM = new CreateProductVM
            {
                Name = "Updated Phone",
                CategoryId = 999,
                Description = "Updated description",
                Price = 200m,
                StockQuantity = 20
            };

            Assert.ThrowsAsync<KeyNotFoundException>(() => _productDAO.UpdateProductById(product.Id, updateVM));
        }

        [Test]
        public async Task UpdateProductById_ValidData_SetsUpdatedAtTimestamp()
        {
            var category = SeedCategory();
            var product = SeedProduct(category.Id, "Phone");
            SeedProduct(category.Id, "Headphones");
            var updateVM = new CreateProductVM
            {
                Name = "Updated Phone",
                CategoryId = category.Id,
                Description = "Updated description",
                Price = 200m,
                StockQuantity = 20
            };

            await _productDAO.UpdateProductById(product.Id, updateVM);

            var updated = await _context.Products.FindAsync(product.Id);
            Assert.That(updated!.UpdatedAt, Is.Not.Null);
        }

        // -------------------- FilterProducts --------------------

        [Test]
        public async Task FilterProducts_ByMinPrice_ReturnsProductsAboveThreshold()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Cheap", price: 50m);
            SeedProduct(category.Id, "MidRange", price: 150m);
            SeedProduct(category.Id, "Expensive", price: 500m);

            var result = await _productDAO.FilterProducts(100m, null, null);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Select(p => p.Name), Is.EquivalentTo(new[] { "MidRange", "Expensive" }));
        }

        [Test]
        public async Task FilterProducts_ByMaxPrice_ReturnsProductsBelowThreshold()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Cheap", price: 50m);
            SeedProduct(category.Id, "MidRange", price: 150m);
            SeedProduct(category.Id, "Expensive", price: 500m);

            var result = await _productDAO.FilterProducts(null, 100m, null);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Cheap"));
        }

        [Test]
        public async Task FilterProducts_ByCategoryId_ReturnsOnlyMatchingCategory()
        {
            var category1 = SeedCategory("Electronics");
            var category2 = SeedCategory("Furniture");
            SeedProduct(category1.Id, "Phone");
            SeedProduct(category1.Id, "Headphones");
            SeedProduct(category2.Id, "Chair");

            var result = await _productDAO.FilterProducts(null, null, category1.Id);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Select(p => p.Name), Is.EquivalentTo(new[] { "Phone", "Headphones" }));
        }

        [Test]
        public async Task FilterProducts_NoFiltersProvided_ReturnsAllProducts()
        {
            var category = SeedCategory();
            SeedProduct(category.Id, "Phone");
            SeedProduct(category.Id, "Headphones");
            SeedProduct(category.Id, "Charger");

            var result = await _productDAO.FilterProducts(null, null, null);

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task FilterProducts_PriceRangeAndCategory_ReturnsIntersection()
        {
            var category1 = SeedCategory("Electronics");
            var category2 = SeedCategory("Furniture");
            SeedProduct(category1.Id, "Phone", price: 300m);
            SeedProduct(category1.Id, "Cable", price: 10m);
            SeedProduct(category2.Id, "Chair", price: 300m);

            var result = await _productDAO.FilterProducts(100m, 1000m, category1.Id);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Phone"));
        }
    }
}
