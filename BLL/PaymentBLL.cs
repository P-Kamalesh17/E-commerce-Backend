using DAL.DTO;
using DAL.Services;

namespace BLL
{
    public class PaymentBLL : IPaymentService
    {
        private readonly PaymentDAO _paymentDAO;

        public PaymentBLL(PaymentDAO paymentDAO)
        {
            _paymentDAO = paymentDAO;
        }

        public async Task<List<PaymentVM>> GetAllPayments()
        {
            return await _paymentDAO.GetAllPayments();
        }

        public async Task<PaymentDetailVM?> GetPaymentById(int id)
        {
            return await _paymentDAO.GetPaymentById(id);
        }

        public async Task<PaymentDetailVM?> GetPaymentByOrderId(int orderId)
        {
            return await _paymentDAO.GetPaymentByOrderId(orderId);
        }

        public async Task<List<PaymentVM>> GetPaymentsByUserId(string userId)
        {
            return await _paymentDAO.GetPaymentsByUserId(userId);
        }

        public async Task<PaymentVM> CreatePayment(CreatePaymentVM createPaymentVM)
        {
            return await _paymentDAO.CreatePayment(createPaymentVM);
        }

        public async Task<PaymentVM> ConfirmPayment(int id)
        {
            return await _paymentDAO.ConfirmPayment(id);
        }

        public async Task<PaymentVM> FailPayment(int id)
        {
            return await _paymentDAO.FailPayment(id);
        }

        public async Task<PaymentVM> RefundPayment(int id)
        {
            return await _paymentDAO.RefundPayment(id);
        }

        public async Task<PaymentVM> UpdatePaymentStatus(int id, UpdatePaymentStatusVM updatePaymentStatusVM)
        {
            return await _paymentDAO.UpdatePaymentStatus(id, updatePaymentStatusVM);
        }
    }
}
