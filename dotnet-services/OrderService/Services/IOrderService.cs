using OrderService.DTOs;

namespace OrderService.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(OrderRequest request);
    Task<List<OrderResponse>> GetAllOrdersAsync();
    Task<OrderResponse> GetOrderByIdAsync(long id);
    Task UpdateOrderStatusAsync(long orderId, string paymentStatus);
}
