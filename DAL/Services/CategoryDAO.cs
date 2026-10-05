using DAL.Data;
using DAL.DTO;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Services
{
    public class CategoryDAO
    {
        private readonly AppDbContext _context;

        public CategoryDAO(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<CategoryVM>> GetAllCategory()
        {
            return await _context.Categories.Select(c => new CategoryVM
            {
                CategoryId = c.Id,
                CategoryName = c.CategoryName
            }).ToListAsync();
        }

        public async Task<CategoryVM?> GetCategoryById(int categoryid)
        {
            return await _context.Categories
                .Where(c => c.Id == categoryid)
                .Select(c => new CategoryVM
                    {
                        CategoryId = c.Id,
                        CategoryName = c.CategoryName
                    }).FirstOrDefaultAsync();
        }

        public async Task CreateCategory(CategoryVM categoryVM)
        {
            if (categoryVM == null)
            {
                throw new ArgumentException("Category data is required.");
            }

            if (string.IsNullOrWhiteSpace(categoryVM.CategoryName))
            {
                throw new ArgumentException("Category name is required.");
            }

            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName == categoryVM.CategoryName);

            if (existingCategory != null)
            {
                throw new InvalidOperationException(
                    "A category with this name already exists.");
            }

            var category = new Category
            {
                CategoryName = categoryVM.CategoryName
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpateCategory(int id, UpdateCategoryVM categoryVM)
        {
            var _res = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if( _res != null )
            {
                _res.CategoryName = categoryVM.CategoryName;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteCategoryById(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return false;
            }

            if (category.Products.Any())
            {
                throw new InvalidOperationException(
                    "Cannot delete this category because products are assigned to it."
                );
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
