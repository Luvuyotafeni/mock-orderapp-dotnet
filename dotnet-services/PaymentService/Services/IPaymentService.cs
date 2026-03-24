using PaymentService.DTOs;

namespace PaymentService.Services;

public interface IPaymentService
{
    Task CreatePendingPaymentAsync(OrderEvent orderEvent);
    Task<PaymentResponse> PayAsync(long orderId, PaymentRequest request);
    Task<List<PaymentResponse>> GetAllPaymentsAsync();
    Task<PaymentResponse> GetPaymentByOrderIdAsync(long orderId);
}
