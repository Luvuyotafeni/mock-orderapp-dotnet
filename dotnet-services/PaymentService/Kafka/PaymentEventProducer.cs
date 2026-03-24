using System.Text.Json;
using Confluent.Kafka;
using PaymentService.DTOs;

namespace PaymentService.Kafka;

public class PaymentEventProducer
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<PaymentEventProducer> _logger;

    public PaymentEventProducer(IConfiguration config, ILogger<PaymentEventProducer> logger)
    {
        _logger = logger;
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"]
        };
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task SendPaymentResultEventAsync(PaymentResultEvent resultEvent)
    {
        var message = JsonSerializer.Serialize(resultEvent);
        _logger.LogInformation("Publishing payment-result event: {Message}", message);
        await _producer.ProduceAsync("payment-result", new Message<Null, string> { Value = message });
    }
}
