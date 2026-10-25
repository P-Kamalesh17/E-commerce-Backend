using DAL.DTO;
using DAL.Services;

namespace BLL
{
    public class OrderItemBLL : IOrderItemService
    {
        private readonly OrderItemDAO _orderItemDAO;

        public OrderItemBLL(OrderItemDAO orderItemDAO)
        {
            _orderItemDAO = orderItemDAO;
        }

        public async Task<List<OrderItemDetailVM>> GetOrderItemsByOrderId(int orderId)
        {
            return await _orderItemDAO.GetOrderItemsByOrderId(orderId);
        }

        public async Task<OrderItemDetailVM?> GetOrderItemById(int id)
        {
            return await _orderItemDAO.GetOrderItemById(id);
        }
    }
}
