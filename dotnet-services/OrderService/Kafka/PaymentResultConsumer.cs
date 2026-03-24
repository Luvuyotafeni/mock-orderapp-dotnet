using System.Text.Json;
using Confluent.Kafka;
using OrderService.DTOs;
using OrderService.Services;

namespace OrderService.Kafka;

public class PaymentResultConsumer : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentResultConsumer> _logger;

    public PaymentResultConsumer(IConfiguration config, IServiceScopeFactory scopeFactory,
        ILogger<PaymentResultConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = "order-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _consumer.Subscribe("payment-result");
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
                    _logger.LogInformation("Received payment-result event: {Message}", result.Message.Value);

                    var paymentEvent = JsonSerializer.Deserialize<PaymentResultEvent>(result.Message.Value);
                    if (paymentEvent != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                        await orderService.UpdateOrderStatusAsync(paymentEvent.OrderId, paymentEvent.Status);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming payment-result event");
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
