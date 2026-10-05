using BLL;
using DAL.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartBLL _cartBLL;
        private readonly ILogger<CartController> _logger;

        public CartController(
            CartBLL cartBLL,
            ILogger<CartController> logger)
        {
            _cartBLL = cartBLL;
            _logger = logger;
        }

        [HttpGet("get-cart/{userId}")]
        public async Task<IActionResult> GetCart(string userId)
        {
            try
            {
                var cart = await _cartBLL.GetCartByUserId(userId);

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
                _logger.LogError(ex, "Error while fetching cart for user {UserId}.", userId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while fetching the cart."
                });
            }
        }

        [HttpGet("get-cart-summary/{userId}")]
        public async Task<IActionResult> GetCartSummary(string userId)
        {
            try
            {
                var summary = await _cartBLL.GetCartSummary(userId);

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
                _logger.LogError(ex, "Error while fetching cart summary for user {UserId}.", userId);

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

        [HttpDelete("clear-cart/{userId}")]
        public async Task<IActionResult> ClearCart(string userId)
        {
            try
            {
                await _cartBLL.ClearCart(userId);

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
                _logger.LogError(ex, "Error while clearing cart for user {UserId}.", userId);

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
