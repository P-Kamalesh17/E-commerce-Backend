using DAL.DTO;

namespace BLL
{
    public interface ICategoryService
    {
        Task CreateCategory(CategoryVM categoryVM);
        Task<bool> DeleteCategoryById(int id);
        Task<List<CategoryVM>> GetAllCategory();
        Task<CategoryVM?> GetCategoryById(int categoryid);
        Task UpdateCategory(int id, UpdateCategoryVM categoryVM);
    }
}
