using BLL;
using DAL.DTO;
using DAL.DTO.Authentication;
using ECommerce.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly ProductBLL _productBLL;
        private readonly ILogger<ProductController> _logger;
        private readonly IFileUploadService _fileUploadService;

        public ProductController(
            ProductBLL productBLL,
            ILogger<ProductController> logger,
            IFileUploadService fileUploadService
           )
        {
            _productBLL = productBLL;
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        [HttpGet("get-all-products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                _logger.LogInformation("Fetching all products.");
                var products = await _productBLL.GetAllProducts();

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching all products.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching products."
                });
            }
        }

        [HttpGet("get-product-by-id/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching product with ID {ProductId}.", id);

                var product = await _productBLL.GetProductById(id);

                if (product == null)
                {
                    return NotFound(new
                    {
                        message = "Product not found."
                    });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching product with ID {ProductId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching the product."
                });
            }
        }

        [HttpGet("get-by-category/{categoryId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsByCategoryId(int categoryId)
        {
            try
            {
                _logger.LogInformation("Fetching products for category ID {CategoryId}.", categoryId);

                var products = await _productBLL.GetProductsByCategoryId(categoryId);

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while fetching products for category ID {CategoryId}.",
                    categoryId
                );

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching products by category."
                });
            }
        }

        [HttpPost("create-product")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> CreateProduct(
            [FromBody] CreateProductVM createProductVM)
        {
            try
            {
                _logger.LogInformation("Creating a new product.");

                await _productBLL.CreateProduct(createProductVM);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "Product created successfully."
                });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Invalid data while creating a product.");

                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Referenced entity not found while creating a product.");

                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating a product.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while creating the product."
                });
            }
        }

        [HttpPut("update-product-by-id/{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> UpdateProductById(
            int id,
            [FromBody] CreateProductVM createProductVM)
        {
            try
            {
                _logger.LogInformation("Updating product with ID {ProductId}.", id);

                await _productBLL.UpdateProductById(id, createProductVM);

                return Ok(new
                {
                    message = "Product updated successfully."
                });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Invalid data while updating product with ID {ProductId}.", id);

                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product with ID {ProductId} not found for update.", id);

                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating product with ID {ProductId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while updating the product."
                });
            }
        }

        [HttpDelete("delete-product-by-id/{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> DeleteProductById(int id)
        {
            try
            {
                _logger.LogInformation("Deleting product with ID {ProductId}.", id);

                await _productBLL.DeleteProductById(id);

                return Ok(new
                {
                    message = "Product deleted successfully."
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product with ID {ProductId} not found for deletion.", id);

                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting product with ID {ProductId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while deleting the product."
                });
            }
        }
        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<IActionResult> FilterProducts(
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int? categoryId)
        {
            try
            {
                _logger.LogInformation(
                    "Filtering products with minPrice={MinPrice}, maxPrice={MaxPrice}, categoryId={CategoryId}.",
                    minPrice,
                    maxPrice,
                    categoryId
                );

                if (minPrice.HasValue && maxPrice.HasValue &&
                    minPrice.Value > maxPrice.Value)
                {
                    return BadRequest(new
                    {
                        message = "Minimum price cannot be greater than maximum price."
                    });
                }

                var products = await _productBLL.FilterProducts(
                    minPrice,
                    maxPrice,
                    categoryId
                );

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while filtering products.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while filtering products.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("upload-image")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No file provided." });
                }

                var imagePath = await _fileUploadService.UploadImageAsync(file);

                _logger.LogInformation("Image uploaded successfully: {ImagePath}.", imagePath);

                return Ok(new
                {
                    message = "Image uploaded successfully.",
                    imageUrl = imagePath
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid file upload.");

                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while uploading image.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while uploading the image."
                });
            }
        }
    }
}