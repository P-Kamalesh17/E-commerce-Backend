using DAL.DTO;

namespace BLL
{
    public interface IPaymentService
    {
        Task<List<PaymentVM>> GetAllPayments();
        Task<PaymentDetailVM?> GetPaymentById(int id);
        Task<PaymentDetailVM?> GetPaymentByOrderId(int orderId);
        Task<List<PaymentVM>> GetPaymentsByUserId(string userId);
        Task<PaymentVM> CreatePayment(CreatePaymentVM createPaymentVM);
        Task<PaymentVM> ConfirmPayment(int id);
        Task<PaymentVM> FailPayment(int id);
        Task<PaymentVM> RefundPayment(int id);
        Task<PaymentVM> UpdatePaymentStatus(int id, UpdatePaymentStatusVM updatePaymentStatusVM);
    }
}
