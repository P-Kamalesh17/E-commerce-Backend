using BLL;
using DAL.DTO;
using DAL.DTO.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userBLL;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userBLL,
            ILogger<UserController> logger)
        {
            _userBLL = userBLL;
            _logger = logger;
        }

        [HttpGet("get-all-users")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                _logger.LogInformation("Fetching all users.");
                var res = await _userBLL.GetAllUsersAsync();
                return Ok(res);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching all users.");
                return BadRequest(new
                {
                    message = "Error in fetching users.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("get-user-profile")]
        [Authorize(Roles = UserRoles.User + "," + UserRoles.Admin + "," + UserRoles.Shipment)]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    return Unauthorized(new
                    {
                        message = "User ID was not found in the access token."
                    });
                }

                var user = await _userBLL.GetUserByIdAsync(currentUserId);

                if (user == null)
                {
                    return NotFound(new
                    {
                        message = "User not found."
                    });
                }

                _logger.LogInformation(
                    "User {UserId} fetched their own profile.",
                    currentUserId
                );

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching current user profile.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching the profile."
                });
            }
        }

        [HttpGet("get-user-by-id/{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                _logger.LogInformation("Admin fetching user with ID {UserId}.", id);
                var user = await _userBLL.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching user with ID {UserId}.", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update-user")]
        [Authorize(Roles = UserRoles.User + "," + UserRoles.Admin + "," + UserRoles.Shipment)]
        public async Task<IActionResult> UpdateUser([FromBody] UserVM user)
        {
            try
            {
                string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    return Unauthorized(new
                    {
                        message = "User ID was not found in the access token."
                    });
                }

                _logger.LogInformation(
                    "User {UserId} is updating their own profile.",
                    currentUserId
                );

                await _userBLL.UpdateUserByIdAsync(currentUserId, user);

                return Ok(new
                {
                    message = "User data updated successfully."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while updating profile for user {UserId}.",
                    User.FindFirstValue(ClaimTypes.NameIdentifier)
                );

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An unexpected error occurred while updating user data."
                });
            }
        }

        [HttpDelete("delete-user-by-id/{id}")]
        [Authorize(Roles = UserRoles.Admin)]
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
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.User)]
        public async Task<IActionResult> GetUserOrderById(string id)
        {
            try
            {
                string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                bool isAdmin = User.IsInRole(UserRoles.Admin);

                if (!isAdmin && currentUserId != id)
                {
                    _logger.LogWarning(
                        "User {CurrentUserId} attempted to view orders for user {RequestedUserId}.",
                        currentUserId,
                        id
                    );

                    return Forbid();
                }

                _logger.LogInformation("Fetching orders for user with ID {UserId}.", id);
                var res = await _userBLL.GetUserOrderById(id);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching orders for user with ID {UserId}.", id);

                return BadRequest(new
                {
                    message = "Error in fetching Orders",
                    error = ex.Message
                });
            }
        }

    }
}