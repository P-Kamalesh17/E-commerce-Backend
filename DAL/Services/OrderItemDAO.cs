using DAL.Data;
using DAL.DTO;
using Microsoft.EntityFrameworkCore;

namespace DAL.Services
{
    public class OrderItemDAO
    {
        private readonly AppDbContext _context;

        public OrderItemDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderItemDetailVM>> GetOrderItemsByOrderId(int orderId)
        {
            var orderExists = await _context.Orders
                .AnyAsync(o => o.Id == orderId);

            if (!orderExists)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            return await _context.Order_Items
                .Where(oi => oi.OrderId == orderId)
                .Select(oi => new OrderItemDetailVM
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.Subtotal
                })
                .ToListAsync();
        }

        public async Task<OrderItemDetailVM?> GetOrderItemById(int id)
        {
            return await _context.Order_Items
                .Where(oi => oi.Id == id)
                .Select(oi => new OrderItemDetailVM
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.Subtotal
                })
                .FirstOrDefaultAsync();
        }
    }
}
