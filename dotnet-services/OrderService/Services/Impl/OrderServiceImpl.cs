using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Entities;
using OrderService.Kafka;

namespace OrderService.Services.Impl;

public class OrderServiceImpl : IOrderService
{
    private readonly AppDbContext _db;
    private readonly OrderEventProducer _producer;
    private readonly ILogger<OrderServiceImpl> _logger;

    public OrderServiceImpl(AppDbContext db, OrderEventProducer producer, ILogger<OrderServiceImpl> logger)
    {
        _db = db;
        _producer = producer;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateOrderAsync(OrderRequest request)
    {
        var order = new Order
        {
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var orderEvent = new OrderEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity
        };

        await _producer.SendOrderCreatedEventAsync(orderEvent);
        return ToResponse(order);
    }

    public async Task<List<OrderResponse>> GetAllOrdersAsync()
    {
        var orders = await _db.Orders.ToListAsync();
        return orders.Select(ToResponse).ToList();
    }

    public async Task<OrderResponse> GetOrderByIdAsync(long id)
    {
        var order = await _db.Orders.FindAsync(id)
            ?? throw new Exception($"Order not found: {id}");
        return ToResponse(order);
    }

    public async Task UpdateOrderStatusAsync(long orderId, string paymentStatus)
    {
        var order = await _db.Orders.FindAsync(orderId)
            ?? throw new Exception($"Order not found: {orderId}");

        order.Status = paymentStatus == "APPROVED" ? "COMPLETED" : "PAYMENT_DECLINED";
        await _db.SaveChangesAsync();
        _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, order.Status);
    }

    private static OrderResponse ToResponse(Order order) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        ProductId = order.ProductId,
        Quantity = order.Quantity,
        Status = order.Status,
        CreatedAt = order.CreatedAt
    };
}
