using BLL;
using DAL.DTO;
using DAL.DTO.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentBLL;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentBLL,
            ILogger<PaymentController> logger)
        {
            _paymentBLL = paymentBLL;
            _logger = logger;
        }

        [HttpGet("get-all-payments")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetAllPayments()
        {
            try
            {
                _logger.LogInformation("Fetching all payments.");

                var res = await _paymentBLL.GetAllPayments();
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching payments.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while fetching payments.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("get-payment/{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                _logger.LogInformation("Fetching payment with ID {PaymentId}.", id);

                var res = await _paymentBLL.GetPaymentById(id);

                if (res == null)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found.", id);

                    return NotFound(new { message = "Payment not found." });
                }

                // Admin and Shipment can view any payment
                // User can only view their own payments (need to check order ownership)
                if (userRole != UserRoles.Admin && userRole != UserRoles.Shipment)
                {
                    // For users, we need to verify they own the order this payment belongs to
                    // This is a simplified check - you may need to fetch the order to validate
                    _logger.LogWarning("User {CurrentUserId} attempted to access payment {PaymentId}.", currentUserId, id);
                    return Forbid();
                }

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching payment with ID {PaymentId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while fetching the payment.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("get-payment-by-order/{orderId}")]
        public async Task<IActionResult> GetPaymentByOrder(int orderId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                _logger.LogInformation("Fetching payment for order ID {OrderId}.", orderId);

                var res = await _paymentBLL.GetPaymentByOrderId(orderId);

                if (res == null)
                {
                    _logger.LogWarning("No payment found for order ID {OrderId}.", orderId);

                    return NotFound(new { message = "No payment found for this order." });
                }

                // Admin and Shipment can view any payment
                // User can only view payments for their own orders (validation needed)
                if (userRole != UserRoles.Admin && userRole != UserRoles.Shipment)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to access payment for order {OrderId}.", currentUserId, orderId);
                    return Forbid();
                }

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching payment for order ID {OrderId}.", orderId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while fetching the payment for the order.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("get-user-payments/{userId}")]
        public async Task<IActionResult> GetUserPayments(string userId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                // Admin and Shipment can view any user's payments
                // User can only view their own payments
                if (userRole != UserRoles.Admin && userRole != UserRoles.Shipment && currentUserId != userId)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to access payments for user {UserId}.", currentUserId, userId);
                    return Forbid();
                }

                _logger.LogInformation("Fetching payments for user {UserId}.", userId);

                var res = await _paymentBLL.GetPaymentsByUserId(userId);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching payments for user {UserId}.", userId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while fetching payments for the user.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("create-payment")]
        [Authorize(Roles = UserRoles.User + "," + UserRoles.Admin)]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentVM createPaymentVM)
        {
            try
            {
                _logger.LogInformation("Creating a new payment.");

                var res = await _paymentBLL.CreatePayment(createPaymentVM);
                return StatusCode(StatusCodes.Status201Created, res);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Invalid data while creating a payment.");

                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Referenced entity not found while creating a payment.");

                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating a payment.");

                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating a payment.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while creating the payment.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("confirm-payment/{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            try
            {
                _logger.LogInformation("Confirming payment with ID {PaymentId}.", id);

                var res = await _paymentBLL.ConfirmPayment(id);
                return Ok(res);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment with ID {PaymentId} not found for confirmation.", id);

                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while confirming payment with ID {PaymentId}.", id);

                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while confirming payment with ID {PaymentId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while confirming the payment.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("fail-payment/{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> FailPayment(int id)
        {
            try
            {
                _logger.LogInformation("Marking payment with ID {PaymentId} as failed.", id);

                var res = await _paymentBLL.FailPayment(id);
                return Ok(res);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment with ID {PaymentId} not found while marking as failed.", id);

                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while marking payment with ID {PaymentId} as failed.", id);

                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while marking payment with ID {PaymentId} as failed.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while marking the payment as failed.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("refund-payment/{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> RefundPayment(int id)
        {
            try
            {
                _logger.LogInformation("Refunding payment with ID {PaymentId}.", id);

                var res = await _paymentBLL.RefundPayment(id);
                return Ok(res);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment with ID {PaymentId} not found for refund.", id);

                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while refunding payment with ID {PaymentId}.", id);

                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while refunding payment with ID {PaymentId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while refunding the payment.",
                    error = ex.Message
                });
            }
        }

        [HttpPatch("update-payment-status/{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusVM updatePaymentStatusVM)
        {
            try
            {
                _logger.LogInformation("Updating status for payment with ID {PaymentId}.", id);

                var res = await _paymentBLL.UpdatePaymentStatus(id, updatePaymentStatusVM);
                return Ok(res);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Invalid data while updating status for payment with ID {PaymentId}.", id);

                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment with ID {PaymentId} not found for status update.", id);

                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating status for payment with ID {PaymentId}.", id);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error while updating the payment status.",
                    error = ex.Message
                });
            }
        }
    }
}
