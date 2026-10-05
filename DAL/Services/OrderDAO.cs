using DAL.Data;
using DAL.DTO;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Services
{
    public class OrderDAO
    {
        private readonly AppDbContext _context;

        public OrderDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderVM>> GetAllOrders(OrderStatus? orderStatus)
        {
            IQueryable<Order> query = _context.Orders;

            if (orderStatus.HasValue)
            {
                query = query.Where(o => o.Status == orderStatus.Value);
            }

            return await query
                .OrderByDescending(o => o.UpdatedAt)
                .ThenByDescending(o => o.CreatedAt)
                .Select(o => new OrderVM
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    OrderStatus = o.Status.ToString(),
                    ShippingAddress = o.ShippingAddress,
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,

                    PaymentStatus = o.Payment != null
                        ? o.Payment.Status.ToString()
                        : "Not Paid"
                })
                .ToListAsync();
        }
        public async Task<OrderDetailVM?> GetOrderById(int id)
        {
            return await _context.Orders
                .Where(o => o.Id == id)
                .Select(o => new OrderDetailVM
                {
                    Id = o.Id,

                    UserId = o.UserId,
                    UserName = o.User.UserName,
                    Email = o.User.Email,

                    OrderStatus = o.Status.ToString(),
                    ShippingAddress = o.ShippingAddress,
                    TotalAmount = o.TotalAmount,

                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,

                    OrderItems = o.OrderItems.Select(oi => new OrderItemDetailVM
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Subtotal = oi.Subtotal
                    }).ToList(),

                    Payment = o.Payment == null
                        ? null
                        : new PaymentDetailVM
                        {
                            Id = o.Payment.Id,
                            Amount = o.Payment.Amount,
                            Status = o.Payment.Status.ToString(),
                            TransactionId = o.Payment.TransactionId,
                            PaidAt = o.Payment.PaidAt,
                            CreatedAt = o.Payment.CreatedAt,
                            UpdatedAt = o.Payment.UpdatedAt
                        }
                })
                .FirstOrDefaultAsync();
        }

        private async Task<List<OrderVM>> GetOrdersByStatusFn(OrderStatus status)
        {
            return await _context.Orders
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.UpdatedAt ?? o.CreatedAt)
                .Select(o => new OrderVM
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    ShippingAddress = o.ShippingAddress,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.Status.ToString(),
                    PaymentStatus = o.Payment != null
                        ? o.Payment.Status.ToString()
                        : "Not Paid",
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<List<OrderVM>> GetOrdersByStatus(OrderStatus status)
        {
            return await GetOrdersByStatusFn(status);
        }

        public async Task<List<OrderVM?>> GetOrdersByUserId(string userid)
        {
            return await _context.Orders
                .Where(o => o.UserId == userid)
                .OrderByDescending(o => o.UpdatedAt ?? o.CreatedAt)
                .Select(o => new OrderVM
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    ShippingAddress = o.ShippingAddress,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.Status.ToString(),

                    PaymentStatus = o.Payment != null
                        ? o.Payment.Status.ToString()
                        : "Not Paid",

                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<OrderVM> CreateOrder(CreateOrderVM createOrderVM)
        {
            if (createOrderVM == null)
            {
                throw new ArgumentNullException(
                    nameof(createOrderVM),
                    "Order details are required."
                );
            }

            if (string.IsNullOrWhiteSpace(createOrderVM.UserId))
            {
                throw new ArgumentException("User ID is required.");
            }

            if (string.IsNullOrWhiteSpace(createOrderVM.ShippingAddress))
            {
                throw new ArgumentException("Shipping address is required.");
            }

            var order = new Order
            {
                UserId = createOrderVM.UserId,
                ShippingAddress = createOrderVM.ShippingAddress,
                Status = OrderStatus.Pending
            };
            var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == createOrderVM.UserId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("Cart is empty.");
            }

            foreach (var item in cart.CartItems)
            {
                 if (item.Product.StockQuantity < item.Quantity)
                 {
                     throw new InvalidOperationException(
                        $"Insufficient stock for product '{item.Product.Name}'."
                     );
                 }
            }

            foreach (var item in cart.CartItems)
                {
                order.OrderItems.Add(new Order_Item
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price,
                    Subtotal = item.Product.Price * item.Quantity
                });

                item.Product.StockQuantity -= item.Quantity;
            }

            _context.CartItems.RemoveRange(cart.CartItems);
            cart.UpdatedAt = DateTime.UtcNow;

            order.TotalAmount = order.OrderItems.Sum(oi => oi.Subtotal);

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            return MapToOrderVM(order);
        }

        public async Task<OrderVM> UpdateOrder(int id, UpdateOrderVM updateOrderVM)
        {
            if (updateOrderVM == null)
            {
                throw new ArgumentNullException(
                    nameof(updateOrderVM),
                    "Order details are required."
                );
            }

            if (string.IsNullOrWhiteSpace(updateOrderVM.ShippingAddress))
            {
                throw new ArgumentException("Shipping address is required.");
            }

            var order = await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            if (order.Status == OrderStatus.Shipped
                || order.Status == OrderStatus.Delivered
                || order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    "Order can only be updated before it is shipped."
                );
            }

            order.ShippingAddress = updateOrderVM.ShippingAddress;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToOrderVM(order);
        }

        public async Task<OrderVM> UpdateOrderStatus(int id, UpdateOrderStatusVM updateOrderStatusVM)
        {
            if (updateOrderStatusVM == null)
            {
                throw new ArgumentNullException(
                    nameof(updateOrderStatusVM),
                    "Order status details are required."
                );
            }

            var order = await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    "Cannot change the status of a delivered or cancelled order."
                );
            }

            order.Status = updateOrderStatusVM.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToOrderVM(order);
        }

        public async Task<OrderVM> CancelOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            if (order.Status == OrderStatus.Delivered)
            {
                throw new InvalidOperationException(
                    "An order cannot be cancelled after it has been delivered."
                );
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Order is already cancelled.");
            }

            foreach (var item in order.OrderItems)
            {
                item.Product.StockQuantity += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToOrderVM(order);
        }

        public async Task<bool> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return false;
            }

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    "Only pending or cancelled orders can be deleted. Cancel the order first."
                );
            }

            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            return true;
        }

        private static OrderVM MapToOrderVM(Order order)
        {
            return new OrderVM
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderStatus = order.Status.ToString(),
                ShippingAddress = order.ShippingAddress,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,

                UpdatedAt = order.UpdatedAt,
                PaymentStatus = order.Payment != null
                    ? order.Payment.Status.ToString()
                    : "Not Paid"
            };
        }
    }
}