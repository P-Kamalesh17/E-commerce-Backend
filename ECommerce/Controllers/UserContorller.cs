using BLL;
using DAL.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserBLL _userBLL;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserBLL userBLL,
            ILogger<UserController> logger)
        {
            _userBLL = userBLL;
            _logger = logger;
        }

        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                _logger.LogInformation("Fetching all users.");

                var users = await _userBLL.GetAllUsersAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching users.");

                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-user-by-id/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                _logger.LogInformation("Fetching user with ID {UserId}.", id);

                var user = await _userBLL.GetUserByIdAsync(id);

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching user with ID {UserId}.", id);

                return BadRequest(ex.Message);
            }
        }

        [HttpPut("create-user")]
        public async Task<IActionResult> CreateUser(
            [FromBody] CreateUserVM user)
        {
            try
            {
                _logger.LogInformation("Creating a new user.");

                await _userBLL.CreateUserAsync(user);

                return Ok("New User Created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating a user.");

                return BadRequest(new
                {
                    message = "Error in Creating user",
                    error = ex.Message
                });
            }
        }

        [HttpPut("update-user-by-id/{id}")]
        public async Task<IActionResult> UpdateUserById(
            string id,
            [FromBody] UserVM user)
        {
            try
            {
                _logger.LogInformation("Updating user with ID {UserId}.", id);

                await _userBLL.UpdateUserByIdAsync(id, user);

                return Ok("User Data Updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating user with ID {UserId}.", id);

                return BadRequest(new
                {
                    message = "Error in updating user.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("delete-user-by-id/{id}")]
        public async Task<IActionResult> DeleteUserById(string id)
        {
            try
            {
                _logger.LogInformation("Deleting user with ID {UserId}.", id);

                await _userBLL.DeleteUserByIdAsync(id);

                return Ok("User Deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting user with ID {UserId}.", id);

                return BadRequest(new
                {
                    message = "Error in Deleting user.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("get-user-order-by-id/{id}")]
        public async Task<IActionResult> GetUserOrderById(string id) {
            try
            {
                _logger.LogInformation("Fetching orders for user with ID {UserId}.", id);

                var res = await _userBLL.GetUserOrderById(id);
                return Ok(res);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error while fetching orders for user with ID {UserId}.", id);

                return BadRequest(new
                {
                    message = "Error in fetching Ordes",
                    error = ex.Message
                });
            }
        }

    }
}