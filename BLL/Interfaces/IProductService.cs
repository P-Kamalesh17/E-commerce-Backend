using DAL.DTO;

namespace BLL
{
    public interface IProductService
    {
        Task CreateProduct(CreateProductVM createProductVM);
        Task DeleteProductById(int id);
        Task<List<ProductVM>> GetAllProducts();
        Task<List<ProductVM>> GetProductsByCategoryId(int categoryId);
        Task<ProductVM?> GetProductById(int id);
        Task UpdateProductById(int id, CreateProductVM createProductVM);
        Task<List<ProductVM>> FilterProducts(decimal? minPrice, decimal? maxPrice, int? categoryId);
    }
}
