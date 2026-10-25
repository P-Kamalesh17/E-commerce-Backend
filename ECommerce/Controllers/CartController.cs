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
    [Authorize(Roles = UserRoles.User)]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartBLL;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartService cartBLL,
            ILogger<CartController> logger)
        {
            _cartBLL = cartBLL;
            _logger = logger;
        }

        [HttpGet("get-cart")]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if(string.IsNullOrWhiteSpace(currentUserId))
                {
                    return Unauthorized(new
                    {
                        message = "User ID was not found in the access token."
                    });
                }

                var cart = await _cartBLL.GetCartByUserId(currentUserId);

                if (cart == null)
                {
                    return NotFound(new
                    {
                        message = "Cart not found for this user."
                    });
                }

                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching cart for user {UserId}.", User.FindFirstValue(ClaimTypes.NameIdentifier));

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching the cart."
                });
            }
        }

        [HttpGet("get-cart-summary")]
        public async Task<IActionResult> GetCartSummary()
        {
            try
            {
                string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if(string.IsNullOrWhiteSpace(currentUserId))
                {
                    return Unauthorized(new
                    {
                        message = "User ID was not found in the access token."
                    });
                }

                var summary = await _cartBLL.GetCartSummary(currentUserId);

                return Ok(summary);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching cart summary for user {UserId}.", User.FindFirstValue(ClaimTypes.NameIdentifier));

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching the cart summary."
                });
            }
        }

        [HttpPost("add-item")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemVM addCartItemVM)
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

                addCartItemVM.UserId = currentUserId;

                await _cartBLL.AddItem(addCartItemVM);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "Item added to cart successfully."
                });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new
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
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding item to cart.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while adding the item to the cart."
                });
            }
        }

        [HttpPut("update-item/{cartItemId:int}")]
        public async Task<IActionResult> UpdateItem(
            int cartItemId,
            [FromBody] UpdateCartItemVM updateCartItemVM)
        {
            try
            {
                await _cartBLL.UpdateItemQuantity(cartItemId, updateCartItemVM);

                return Ok(new
                {
                    message = "Cart item updated successfully."
                });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new
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
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating cart item with ID {CartItemId}.", cartItemId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while updating the cart item."
                });
            }
        }

        [HttpDelete("remove-item/{cartItemId:int}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            try
            {
                await _cartBLL.RemoveItem(cartItemId);

                return Ok(new
                {
                    message = "Item removed from cart successfully."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while removing cart item with ID {CartItemId}.", cartItemId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while removing the item from the cart."
                });
            }
        }

        [HttpDelete("clear-cart")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if(string.IsNullOrWhiteSpace(currentUserId))
                {
                    return Unauthorized(new
                    {
                        message = "User ID was not found in the access token."
                    });
                }

                await _cartBLL.ClearCart(currentUserId);

                return Ok(new
                {
                    message = "Cart cleared successfully."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while clearing cart for user {UserId}.", User.FindFirstValue(ClaimTypes.NameIdentifier));

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while clearing the cart."
                });
            }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutVM checkoutVM)
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

                checkoutVM.UserId = currentUserId;

                var order = await _cartBLL.Checkout(checkoutVM);

                return StatusCode(StatusCodes.Status201Created, order);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking out the cart.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred during checkout."
                });
            }
        }
    }
}
