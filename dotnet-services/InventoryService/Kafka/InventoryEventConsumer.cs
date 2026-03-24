using System.Text.Json;
using Confluent.Kafka;
using InventoryService.DTOs;
using InventoryService.Services;

namespace InventoryService.Kafka;

public class InventoryEventConsumer : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InventoryEventConsumer> _logger;

    public InventoryEventConsumer(IConfiguration config, IServiceScopeFactory scopeFactory,
        ILogger<InventoryEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = "inventory-group",
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
                    _logger.LogInformation("Received order-created event: {Message}", result.Message.Value);

                    var orderEvent = JsonSerializer.Deserialize<OrderEvent>(result.Message.Value);
                    if (orderEvent != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
                        await inventoryService.DeductInventoryAsync(orderEvent.ProductId, orderEvent.Quantity);
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
