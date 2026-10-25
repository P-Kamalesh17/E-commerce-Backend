using DAL.DTO;
using DAL.Models;

namespace BLL
{
    public interface IOrderService
    {
        Task<List<OrderVM>> GetAllOrders(OrderStatus? orderStatus);
        Task<OrderDetailVM?> GetOrderById(int id);
        Task<List<OrderVM>> GetOrdersByStatus(OrderStatus status);
        Task<List<OrderVM?>> GetOrdersByUserId(string userId);
        Task<OrderVM> CreateOrder(CreateOrderVM createOrderVM);
        Task<OrderVM> UpdateOrder(int id, UpdateOrderVM updateOrderVM);
        Task<OrderVM> UpdateOrderStatus(int id, UpdateOrderStatusVM updateOrderStatusVM);
        Task<OrderVM> CancelOrder(int id);
        Task<bool> DeleteOrder(int id);
    }
}
