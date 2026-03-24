using System.Text.Json;
using Confluent.Kafka;
using PaymentService.DTOs;
using PaymentService.Services;

namespace PaymentService.Kafka;

public class PaymentEventConsumer : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentEventConsumer> _logger;

    public PaymentEventConsumer(IConfiguration config, IServiceScopeFactory scopeFactory,
        ILogger<PaymentEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = "payment-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _consumer.Subscribe("order-created");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    _logger.LogInformation("Payment service received order-created: {Message}", result.Message.Value);

                    var orderEvent = JsonSerializer.Deserialize<OrderEvent>(result.Message.Value);
                    if (orderEvent != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                        await paymentService.CreatePendingPaymentAsync(orderEvent);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming order-created event");
                }
            }
        }, stoppingToken);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}
