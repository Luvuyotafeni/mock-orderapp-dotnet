using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.DTOs;
using PaymentService.Entities;
using PaymentService.Kafka;

namespace PaymentService.Services.Impl;

public class PaymentServiceImpl : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly PaymentEventProducer _producer;
    private readonly ILogger<PaymentServiceImpl> _logger;
    private readonly Random _random = new();

    public PaymentServiceImpl(AppDbContext db, PaymentEventProducer producer,
        ILogger<PaymentServiceImpl> logger)
    {
        _db = db;
        _producer = producer;
        _logger = logger;
    }

    public async Task CreatePendingPaymentAsync(OrderEvent orderEvent)
    {
        var payment = new Payment
        {
            OrderId = orderEvent.OrderId,
            CustomerId = orderEvent.CustomerId,
            ProductId = orderEvent.ProductId,
            Quantity = orderEvent.Quantity
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Pending payment created for order {OrderId}", orderEvent.OrderId);
    }

    public async Task<PaymentResponse> PayAsync(long orderId, PaymentRequest request)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId)
            ?? throw new Exception($"No pending payment found for order: {orderId}");

        if (payment.Status != "PENDING")
            throw new Exception($"Payment for order {orderId} already {payment.Status}");

        // Mock validation — 16 digit card and 3 digit CVV
        var cardNumber = request.CardNumber.Replace(" ", "");
        bool validCard = cardNumber.Length == 16 && request.Cvv.Length == 3;

        // 70% approval if card is valid, always declined if invalid
        string status = (validCard && _random.Next(10) < 7) ? "APPROVED" : "DECLINED";

        payment.CardHolderName = request.CardHolderName;
        payment.MaskedCardNumber = $"**** **** **** {cardNumber[^4..]}";
        payment.Status = status;
        payment.ProcessedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Payment for order {OrderId} processed — result: {Status}", orderId, status);

        var resultEvent = new PaymentResultEvent
        {
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            ProductId = payment.ProductId,
            Quantity = payment.Quantity,
            Status = status
        };

        await _producer.SendPaymentResultEventAsync(resultEvent);
        return ToResponse(payment);
    }

    public async Task<List<PaymentResponse>> GetAllPaymentsAsync()
    {
        var payments = await _db.Payments.ToListAsync();
        return payments.Select(ToResponse).ToList();
    }

    public async Task<PaymentResponse> GetPaymentByOrderIdAsync(long orderId)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId)
            ?? throw new Exception($"Payment not found for order: {orderId}");
        return ToResponse(payment);
    }

    private static PaymentResponse ToResponse(Payment p) => new()
    {
        Id = p.Id,
        OrderId = p.OrderId,
        CustomerId = p.CustomerId,
        MaskedCardNumber = p.MaskedCardNumber,
        Status = p.Status,
        CreatedAt = p.CreatedAt,
        ProcessedAt = p.ProcessedAt
    };
}
