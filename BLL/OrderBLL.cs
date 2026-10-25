using DAL.DTO;
using DAL.Models;
using DAL.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL
{
    public class OrderBLL : IOrderService
    {
        private readonly OrderDAO _orderDAO;
        public OrderBLL(OrderDAO orderDAO)
        {
            _orderDAO = orderDAO;
        }
        public async Task<List<OrderVM>> GetAllOrders(OrderStatus? orderStatus) {
            return await _orderDAO.GetAllOrders(orderStatus);
        }
        public async Task<OrderDetailVM?> GetOrderById(int id)
        {
            return await _orderDAO.GetOrderById(id);
        }

        public async Task<List<OrderVM>> GetOrdersByStatus(OrderStatus status)
        {
            return await _orderDAO.GetOrdersByStatus(status);
        }

        public async Task<List<OrderVM?>> GetOrdersByUserId(string userId)
        {
            return await _orderDAO.GetOrdersByUserId(userId);
        }

        public async Task<OrderVM> CreateOrder(CreateOrderVM createOrderVM)
        {
            return await _orderDAO.CreateOrder(createOrderVM);
        }

        public async Task<OrderVM> UpdateOrder(int id, UpdateOrderVM updateOrderVM)
        {
            return await _orderDAO.UpdateOrder(id, updateOrderVM);
        }

        public async Task<OrderVM> UpdateOrderStatus(int id, UpdateOrderStatusVM updateOrderStatusVM)
        {
            return await _orderDAO.UpdateOrderStatus(id, updateOrderStatusVM);
        }

        public async Task<OrderVM> CancelOrder(int id)
        {
            return await _orderDAO.CancelOrder(id);
        }

        public async Task<bool> DeleteOrder(int id)
        {
            return await _orderDAO.DeleteOrder(id);
        }
    }
}
