using DAL.Data;
using DAL.DTO;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Services
{
    public class ProductDAO
    {
        private readonly AppDbContext _context;

        public ProductDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateProduct(CreateProductVM createProductVM)
        {
            if (createProductVM == null)
            {
                throw new ArgumentNullException(
                    nameof(createProductVM),
                    "Product details are required."
                );
            }

            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == createProductVM.CategoryId);

            if (!categoryExists)
            {
                throw new KeyNotFoundException("Invalid category ID.");
            }

            var product = new Product
            {
                Name = createProductVM.Name,
                CategoryId = createProductVM.CategoryId,
                Description = createProductVM.Description,
                Price = createProductVM.Price,
                StockQuantity = createProductVM.StockQuantity,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                throw new KeyNotFoundException("Invalid product ID.");
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
        }

        public async Task<List<ProductVM>> GetAllProducts()
        {
            return await _context.Products
                .Select(p => new ProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    CategoryName = p.Category.CategoryName,
                    CategoryId = p.CategoryId
                })
                .ToListAsync();
        }

        public async Task<List<ProductVM>> GetProductByCategoryId(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Select(p => new ProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    CategoryName = p.Category.CategoryName,
                    CategoryId = p.CategoryId
                })
                .ToListAsync();
        }

        public async Task<ProductVM?> GetProductById(int id)
        {
            return await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new ProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    CategoryName = p.Category.CategoryName,
                    CategoryId = p.CategoryId
                })
                .FirstOrDefaultAsync();
        }

        public async Task UpdateProductById(int id, CreateProductVM createProductVM)
        {
            if (createProductVM == null)
            {
                throw new ArgumentNullException(
                    nameof(createProductVM),
                    "Product details are required."
                );
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                throw new KeyNotFoundException("Invalid product ID.");
            }

            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == createProductVM.CategoryId);

            if (!categoryExists)
            {
                throw new KeyNotFoundException("Invalid category ID.");
            }

            product.Name = createProductVM.Name;
            product.Description = createProductVM.Description;
            product.CategoryId = createProductVM.CategoryId;
            product.Price = createProductVM.Price;
            product.StockQuantity = createProductVM.StockQuantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        public async Task<List<ProductVM>> FilterProducts(
            decimal? minPrice,
            decimal? maxPrice,
            int? categoryId)
        {
            IQueryable<Product> query = _context.Products;

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            return await query
                .Select(p => new ProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.CategoryName
                })
                .ToListAsync();
        }
    }
}   