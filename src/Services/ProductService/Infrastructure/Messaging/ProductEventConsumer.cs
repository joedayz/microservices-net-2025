using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductService.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductService.Infrastructure.Messaging;

public class ProductEventConsumer : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<ProductEventConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public ProductEventConsumer(
        IOptions<RabbitMqOptions> options,
        ILogger<ProductEventConsumer> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var factory = new ConnectionFactory { Uri = new Uri(_options.ConnectionString) };
            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await _channel.ExchangeDeclareAsync(_options.Exchange, _options.ExchangeType, durable: true, autoDelete: false, cancellationToken: stoppingToken);

            var queueDeclareOk = await _channel.QueueDeclareAsync(
                "product-events-productservice", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(queueDeclareOk.QueueName, _options.Exchange, "product.*", cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                _logger.LogInformation("[ProductEventConsumer] Received event {RoutingKey}: {Payload}", ea.RoutingKey, json);
                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            };
            await _channel.BasicConsumeAsync(queueDeclareOk.QueueName, autoAck: false, consumer, stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ProductEventConsumer could not connect to RabbitMQ. Events will not be consumed.");
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
