using System.Text.Json;
using Confluent.Kafka;
using OrderService.DTOs;

namespace OrderService.Kafka;

public class OrderEventProducer
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<OrderEventProducer> _logger;

    public OrderEventProducer(IConfiguration config, ILogger<OrderEventProducer> logger)
    {
        _logger = logger;
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"]
        };
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task SendOrderCreatedEventAsync(OrderEvent orderEvent)
    {
        var message = JsonSerializer.Serialize(orderEvent);
        _logger.LogInformation("Publishing order-created event: {Message}", message);
        await _producer.ProduceAsync("order-created", new Message<Null, string> { Value = message });
    }
}
