using DAL.Data;
using DAL.DTO;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Services
{
    public class PaymentDAO
    {
        private readonly AppDbContext _context;

        public PaymentDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PaymentVM>> GetAllPayments()
        {
            var payments = await _context.Payments
                .Include(p => p.Order)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(p => MapToPaymentVM(p)).ToList();
        }

        public async Task<PaymentDetailVM?> GetPaymentById(int id)
        {
            return await _context.Payments
                .Where(p => p.Id == id)
                .Select(p => new PaymentDetailVM
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    Status = p.Status.ToString(),
                    TransactionId = p.TransactionId,
                    PaidAt = p.PaidAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentDetailVM?> GetPaymentByOrderId(int orderId)
        {
            return await _context.Payments
                .Where(p => p.OrderId == orderId)
                .Select(p => new PaymentDetailVM
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    Status = p.Status.ToString(),
                    TransactionId = p.TransactionId,
                    PaidAt = p.PaidAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<PaymentVM>> GetPaymentsByUserId(string userId)
        {
            var payments = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.Order.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(p => MapToPaymentVM(p)).ToList();
        }

        public async Task<PaymentVM> CreatePayment(CreatePaymentVM createPaymentVM)
        {
            if (createPaymentVM == null)
            {
                throw new ArgumentNullException(
                    nameof(createPaymentVM),
                    "Payment details are required."
                );
            }

            var order = await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == createPaymentVM.OrderId);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            if (order.Payment != null)
            {
                throw new InvalidOperationException("This order already has a payment.");
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot create a payment for a cancelled order.");
            }

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                Status = PaymentStatus.Pending,
                TransactionId = createPaymentVM.TransactionId
            };

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            return MapToPaymentVM(payment, order.UserId);
        }

        public async Task<PaymentVM> ConfirmPayment(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                throw new KeyNotFoundException("Payment not found.");
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                throw new InvalidOperationException("Only a pending payment can be confirmed.");
            }

            payment.Status = PaymentStatus.Success;
            payment.PaidAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            if (payment.Order.Status == OrderStatus.Pending)
            {
                payment.Order.Status = OrderStatus.Paid;
                payment.Order.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return MapToPaymentVM(payment, payment.Order.UserId);
        }

        public async Task<PaymentVM> FailPayment(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                throw new KeyNotFoundException("Payment not found.");
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                throw new InvalidOperationException("Only a pending payment can be marked as failed.");
            }

            payment.Status = PaymentStatus.Failed;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToPaymentVM(payment, payment.Order.UserId);
        }

        public async Task<PaymentVM> RefundPayment(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                throw new KeyNotFoundException("Payment not found.");
            }

            if (payment.Status != PaymentStatus.Success)
            {
                throw new InvalidOperationException("Only a successful payment can be refunded.");
            }

            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToPaymentVM(payment, payment.Order.UserId);
        }

        public async Task<PaymentVM> UpdatePaymentStatus(int id, UpdatePaymentStatusVM updatePaymentStatusVM)
        {
            if (updatePaymentStatusVM == null)
            {
                throw new ArgumentNullException(
                    nameof(updatePaymentStatusVM),
                    "Payment status details are required."
                );
            }

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                throw new KeyNotFoundException("Payment not found.");
            }

            payment.Status = updatePaymentStatusVM.Status;

            if (updatePaymentStatusVM.TransactionId != null)
            {
                payment.TransactionId = updatePaymentStatusVM.TransactionId;
            }

            if (updatePaymentStatusVM.PaidAt.HasValue)
            {
                payment.PaidAt = updatePaymentStatusVM.PaidAt;
            }

            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToPaymentVM(payment, payment.Order.UserId);
        }

        private static PaymentVM MapToPaymentVM(Payment payment, string? userId = null)
        {
            return new PaymentVM
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                UserId = userId ?? payment.Order.UserId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                TransactionId = payment.TransactionId,
                PaidAt = payment.PaidAt,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }
    }
}
