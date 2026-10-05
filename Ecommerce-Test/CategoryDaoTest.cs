using DAL.Data;
using DAL.DTO;
using DAL.Models;
using DAL.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Test
{
    public class CategoryDaoTest
    {
        private AppDbContext _context;
        private CategoryDAO _categoryDAO;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _categoryDAO = new CategoryDAO(_context);
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

        // -------------------- GetAllCategory --------------------

        [Test]
        public async Task GetAllCategory_NoCategories_ReturnsEmptyList()
        {
            var result = await _categoryDAO.GetAllCategory();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllCategory_MultipleCategories_ReturnsAllCategories()
        {
            SeedCategory("Electronics");
            SeedCategory("Furniture");
            SeedCategory("Books");

            var result = await _categoryDAO.GetAllCategory();

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task GetAllCategory_ReturnsCorrectMappedFields()
        {
            var category = SeedCategory("Audio");
            SeedCategory("Furniture");

            var result = await _categoryDAO.GetAllCategory();
            var audio = result.First(c => c.CategoryId == category.Id);

            Assert.That(audio.CategoryName, Is.EqualTo("Audio"));
        }

        // -------------------- GetCategoryById --------------------

        [Test]
        public async Task GetCategoryById_ExistingId_ReturnsCorrectCategory()
        {
            var category = SeedCategory("Books");

            var result = await _categoryDAO.GetCategoryById(category.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.CategoryName, Is.EqualTo("Books"));
        }

        [Test]
        public async Task GetCategoryById_NonExistentId_ReturnsNull()
        {
            var result = await _categoryDAO.GetCategoryById(999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetCategoryById_MultipleCategories_ReturnsOnlyRequestedOne()
        {
            SeedCategory("Electronics");
            var target = SeedCategory("Furniture");
            SeedCategory("Books");

            var result = await _categoryDAO.GetCategoryById(target.Id);

            Assert.That(result!.CategoryId, Is.EqualTo(target.Id));
        }

        // -------------------- CreateCategory --------------------

        [Test]
        public async Task CreateCategory_ValidData_AddsCategoryToDatabase()
        {
            var categoryVM = new CategoryVM { CategoryName = "Toys" };

            await _categoryDAO.CreateCategory(categoryVM);

            var categories = await _context.Categories.ToListAsync();
            Assert.That(categories.Count, Is.EqualTo(1));
            Assert.That(categories[0].CategoryName, Is.EqualTo("Toys"));
        }

        [Test]
        public void CreateCategory_NullCategory_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _categoryDAO.CreateCategory(null!));
        }

        [Test]
        public async Task CreateCategory_DuplicateName_ThrowsInvalidOperationException()
        {
            SeedCategory("Toys");
            var categoryVM = new CategoryVM { CategoryName = "Toys" };

            Assert.ThrowsAsync<InvalidOperationException>(() => _categoryDAO.CreateCategory(categoryVM));
        }

        // -------------------- UpateCategory --------------------

        [Test]
        public async Task UpateCategory_ValidId_UpdatesCategoryName()
        {
            var category = SeedCategory("Electronics");
            var updateVM = new UpdateCategoryVM { CategoryName = "Updated Electronics" };

            await _categoryDAO.UpateCategory(category.Id, updateVM);

            var updated = await _context.Categories.FindAsync(category.Id);
            Assert.That(updated!.CategoryName, Is.EqualTo("Updated Electronics"));
        }

        [Test]
        public async Task UpateCategory_NonExistentId_DoesNotThrow()
        {
            var updateVM = new UpdateCategoryVM { CategoryName = "Doesn't matter" };

            Assert.DoesNotThrowAsync(() => _categoryDAO.UpateCategory(999, updateVM));
        }

        [Test]
        public async Task UpateCategory_MultipleCategories_OnlyUpdatesTarget()
        {
            var target = SeedCategory("Electronics");
            var other = SeedCategory("Furniture");
            var updateVM = new UpdateCategoryVM { CategoryName = "Gadgets" };

            await _categoryDAO.UpateCategory(target.Id, updateVM);

            var updatedTarget = await _context.Categories.FindAsync(target.Id);
            var untouchedOther = await _context.Categories.FindAsync(other.Id);
            Assert.That(updatedTarget!.CategoryName, Is.EqualTo("Gadgets"));
            Assert.That(untouchedOther!.CategoryName, Is.EqualTo("Furniture"));
        }

        // -------------------- DeleteCategoryById --------------------

        [Test]
        public async Task DeleteCategoryById_ValidEmptyCategory_DeletesAndReturnsTrue()
        {
            var category = SeedCategory("Electronics");

            var result = await _categoryDAO.DeleteCategoryById(category.Id);

            Assert.That(result, Is.True);
            var categories = await _context.Categories.ToListAsync();
            Assert.That(categories, Is.Empty);
        }

        [Test]
        public async Task DeleteCategoryById_NonExistentId_ReturnsFalse()
        {
            var result = await _categoryDAO.DeleteCategoryById(999);

            Assert.That(result, Is.False);
        }

        [Test]
        public void DeleteCategoryById_CategoryWithProducts_ThrowsInvalidOperationException()
        {
            var category = SeedCategory("Electronics");
            SeedProduct(category.Id, "Phone");
            SeedProduct(category.Id, "Headphones");

            Assert.ThrowsAsync<InvalidOperationException>(() => _categoryDAO.DeleteCategoryById(category.Id));
        }
    }
}
