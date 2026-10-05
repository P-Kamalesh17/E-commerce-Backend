using BLL;
using DAL.DTO;
using DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderBLL _orderBLL;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            OrderBLL orderBLL,
            ILogger<OrderController> logger)
        {
            _orderBLL = orderBLL;
            _logger = logger;
        }
        [HttpGet("get-all-orders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderStatus? orderStatus)
        {
            try
            {
                _logger.LogInformation("Fetching all orders with status filter {OrderStatus}.", orderStatus);

                var res = await _orderBLL.GetAllOrders(orderStatus);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching orders.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while filtering orders.",
                    error = ex.Message
                });
            }
        }
        [HttpGet("get-order-by-id/{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching order with ID {OrderId}.", id);

                var res = await _orderBLL.GetOrderById(id);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching order with ID {OrderId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while fetching order by id.",
                    error = ex.Message
                });
            }
        }
        [HttpGet("get-orders-by-userid/{userid}")]
        public async Task<IActionResult> GetOrdersByUserId(string userid)
        {
            try
            {
                _logger.LogInformation("Fetching orders for user {UserId}.", userid);

                var res = await _orderBLL.GetOrdersByUserId(userid);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching orders for user {UserId}.", userid);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while fetching orders by userid.",
                    error = ex.Message
                });
            }
        }
        [HttpGet("get-orders-by-status/{status}")]
        public async Task<IActionResult> GetOrdersByStatus([FromQuery] OrderStatus status)
        {
            try
            {
                _logger.LogInformation("Fetching orders with status {OrderStatus}.", status);

                var res = await _orderBLL.GetOrdersByStatus(status);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching orders with status {OrderStatus}.", status);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while fetching orders by status.",
                    error = ex.Message
                });
            }
        }
        [HttpGet("get-pending-orders")]
        public async Task<IActionResult> GetPendingOrders()
        {
            try
            {
                _logger.LogInformation("Fetching pending orders.");

                var res = await _orderBLL.GetOrdersByStatus(OrderStatus.Pending);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching pending orders.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while fetching pending orders.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderVM createOrderVM)
        {
            try
            {
                _logger.LogInformation("Creating a new order.");

                var res = await _orderBLL.CreateOrder(createOrderVM);
                return StatusCode(StatusCodes.Status201Created, res);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Invalid data while creating an order.");

                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid data while creating an order.");

                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Referenced entity not found while creating an order.");

                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating an order.");

                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating the order.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while creating the order.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("update-order/{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderVM updateOrderVM)
        {
            try
            {
                _logger.LogInformation("Updating order with ID {OrderId}.", id);

                var res = await _orderBLL.UpdateOrder(id, updateOrderVM);
                return Ok(res);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Invalid data while updating order with ID {OrderId}.", id);

                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid data while updating order with ID {OrderId}.", id);

                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order with ID {OrderId} not found for update.", id);

                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating order with ID {OrderId}.", id);

                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating order with ID {OrderId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while updating the order.",
                    error = ex.Message
                });
            }
        }

        [HttpPatch("update-order-status/{id}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusVM updateOrderStatusVM)
        {
            try
            {
                _logger.LogInformation("Updating status for order with ID {OrderId}.", id);

                var res = await _orderBLL.UpdateOrderStatus(id, updateOrderStatusVM);
                return Ok(res);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order with ID {OrderId} not found for status update.", id);

                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating status for order with ID {OrderId}.", id);

                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating status for order with ID {OrderId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while updating the order status.",
                    error = ex.Message
                });
            }
        }

        [HttpPatch("cancel-order/{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                _logger.LogInformation("Cancelling order with ID {OrderId}.", id);

                var res = await _orderBLL.CancelOrder(id);
                return Ok(res);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Order with ID {OrderId} not found for cancellation.", id);

                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while cancelling order with ID {OrderId}.", id);

                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cancelling order with ID {OrderId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while cancelling the order.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("delete-order/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                _logger.LogInformation("Deleting order with ID {OrderId}.", id);

                var isDeleted = await _orderBLL.DeleteOrder(id);

                if (!isDeleted)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found for deletion.", id);

                    return NotFound(new { message = "Order not found." });
                }

                return Ok(new { message = "Order deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while deleting order with ID {OrderId}.", id);

                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting order with ID {OrderId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while deleting the order.",
                    error = ex.Message
                });
            }
        }
    }
}
