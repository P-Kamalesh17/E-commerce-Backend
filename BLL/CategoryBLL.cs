using DAL.DTO;
using DAL.Models;
using DAL.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL
{
    public class CategoryBLL : ICategoryService
    {
        private readonly CategoryDAO _categoryDAO;
        public CategoryBLL(CategoryDAO categoryDAO)
        {
            _categoryDAO = categoryDAO;
        }

        public async Task CreateCategory(CategoryVM categoryVM)
        {
            await _categoryDAO.CreateCategory(categoryVM);
        }

        public async Task<bool> DeleteCategoryById(int id)
        {
            return await _categoryDAO.DeleteCategoryById(id);
        }

        public async Task<List<CategoryVM>> GetAllCategory()
        {
            return await _categoryDAO.GetAllCategory();
        }

        public async Task<CategoryVM?> GetCategoryById(int categoryid)
        {
            return await _categoryDAO.GetCategoryById(categoryid);
        }

        public async Task UpdateCategory(int id, UpdateCategoryVM categoryVM)
        {
            await _categoryDAO.UpateCategory(id, categoryVM);
        }
    }
}
