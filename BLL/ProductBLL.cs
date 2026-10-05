using DAL.DTO;
using DAL.Services;

namespace BLL
{
    public class ProductBLL
    {
        private readonly ProductDAO _productDAO;

        public ProductBLL(ProductDAO productDAO)
        {
            _productDAO = productDAO;
        }

        public async Task CreateProduct(CreateProductVM createProductVM)
        {
            await _productDAO.CreateProduct(createProductVM);
        }

        public async Task DeleteProductById(int id)
        {
            await _productDAO.DeleteProductById(id);
        }

        public async Task<List<ProductVM>> GetAllProducts()
        {
            return await _productDAO.GetAllProducts();
        }

        public async Task<List<ProductVM>> GetProductsByCategoryId(int categoryId)
        {
            return await _productDAO.GetProductByCategoryId(categoryId);
        }

        public async Task<ProductVM?> GetProductById(int id)
        {
            return await _productDAO.GetProductById(id);
        }

        public async Task UpdateProductById(int id, CreateProductVM createProductVM)
        {
            await _productDAO.UpdateProductById(id, createProductVM);
        }
        public Task<List<ProductVM>> FilterProducts(
            decimal? minPrice,
            decimal? maxPrice,
            int? categoryId)
        {
            return _productDAO.FilterProducts(minPrice, maxPrice, categoryId);
        }
    }
}