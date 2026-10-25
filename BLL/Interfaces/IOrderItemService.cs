using DAL.DTO;

namespace BLL
{
    public interface IOrderItemService
    {
        Task<List<OrderItemDetailVM>> GetOrderItemsByOrderId(int orderId);
        Task<OrderItemDetailVM?> GetOrderItemById(int id);
    }
}
