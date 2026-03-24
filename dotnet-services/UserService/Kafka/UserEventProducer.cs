using Confluent.Kafka;

namespace UserService.Kafka;

public class UserEventProducer
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<UserEventProducer> _logger;

    public UserEventProducer(IConfiguration config, ILogger<UserEventProducer> logger)
    {
        _logger = logger;
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"]
        };
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task SendUserRegisteredEventAsync(string email)
    {
        _logger.LogInformation("Publishing user-registered event for: {Email}", email);
        await _producer.ProduceAsync("user-registered", new Message<Null, string> { Value = email });
    }
}
