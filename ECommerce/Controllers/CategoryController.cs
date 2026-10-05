using BLL;
using DAL.DTO;
using DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryBLL _categoryBLL;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(
            CategoryBLL categoryBLL,
            ILogger<CategoryController> logger)
        {
            _categoryBLL = categoryBLL;
            _logger = logger;
        }
        [HttpGet("get-all-category")]
        public async Task<IActionResult> GetAllCategory()
        {
            try
            {
                _logger.LogInformation("Fetching all categories.");

                var res = await _categoryBLL.GetAllCategory();
                return Ok(res);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error while fetching categories.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching categories."
                });
            }
        }
        [HttpGet("get-category-by-id/{categoryid}")]
        public async Task<IActionResult> GetCategoryById(int categoryid)
        {
            try
            {
                _logger.LogInformation("Fetching category with ID {CategoryId}.", categoryid);

                var res = await _categoryBLL.GetCategoryById(categoryid);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching category with ID {CategoryId}.", categoryid);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching categories by id."
                });
            }
        }
        [HttpPost("create-category")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryVM categoryVM)
        {
            try
            {
                _logger.LogInformation("Creating a new category.");

                await _categoryBLL.CreateCategory(categoryVM);
                return Ok("Category created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating a category.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while creating categories."
                });
            }
        }
        [HttpPut("update-category/{id}")]
        public async Task<IActionResult> UpdateCategory(int id,[FromBody] UpdateCategoryVM categoryVM)
        {
            try
            {
                _logger.LogInformation("Updating category with ID {CategoryId}.", id);

                await _categoryBLL.UpdateCategory(id,categoryVM);
                return Ok("Category created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating category with ID {CategoryId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while creating category."
                });
            }
        }
        [HttpDelete("delete-category-by-id/{id}")]
        public async Task<IActionResult> DeleteCategoryById(int id)
        {
            try
            {
                _logger.LogInformation("Deleting category with ID {CategoryId}.", id);

                var isDeleted = await _categoryBLL.DeleteCategoryById(id);

                if (!isDeleted)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found for deletion.", id);

                    return NotFound(new
                    {
                        message = "Category not found."
                    });
                }

                return Ok(new
                {
                    message = "Category deleted successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                // For example: category still contains products.
                _logger.LogWarning(ex, "Cannot delete category with ID {CategoryId}.", id);

                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting category with ID {CategoryId}.", id);

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while deleting the category."
                });
            }
        }
    }
}
