using BLL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly OrderItemBLL _orderItemBLL;
        private readonly ILogger<OrderItemController> _logger;

        public OrderItemController(
            OrderItemBLL orderItemBLL,
            ILogger<OrderItemController> logger)
        {
            _orderItemBLL = orderItemBLL;
            _logger = logger;
        }

        [HttpGet("get-order-items/{orderId}")]
        public async Task<IActionResult> GetOrderItems(int orderId)
        {
            try
            {
                _logger.LogInformation("Fetching order items for order ID {OrderId}.", orderId);

                var res = await _orderItemBLL.GetOrderItemsByOrderId(orderId);
                return Ok(res);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order with ID {OrderId} not found while fetching order items.", orderId);

                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching order items for order ID {OrderId}.", orderId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while fetching order items.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("get-order-item/{id}")]
        public async Task<IActionResult> GetOrderItem(int id)
        {
            try
            {
                _logger.LogInformation("Fetching order item with ID {OrderItemId}.", id);

                var res = await _orderItemBLL.GetOrderItemById(id);

                if (res == null)
                {
                    _logger.LogWarning("Order item with ID {OrderItemId} not found.", id);

                    return NotFound(new
                    {
                        message = "Order item not found."
                    });
                }

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching order item with ID {OrderItemId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while fetching the order item.",
                    error = ex.Message
                });
            }
        }
    }
}
