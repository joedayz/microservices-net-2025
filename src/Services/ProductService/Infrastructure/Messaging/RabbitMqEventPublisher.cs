using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductService.Domain.Events;
using RabbitMQ.Client;

namespace ProductService.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqEventPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (_channel != null) return;
        var factory = new ConnectionFactory { Uri = new Uri(_options.ConnectionString) };
        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _channel.ExchangeDeclareAsync(
            _options.Exchange, _options.ExchangeType,
            durable: true, autoDelete: false,
            cancellationToken: cancellationToken);
    }

    public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default)
        where T : DomainEvent
    {
        await EnsureInitializedAsync(cancellationToken);

        var json = JsonSerializer.Serialize(domainEvent);
        var body = Encoding.UTF8.GetBytes(json);
        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            ContentType = "application/json",
            Type = domainEvent.EventType
        };

        await _channel!.BasicPublishAsync(
            exchange: _options.Exchange,
            routingKey: domainEvent.EventType,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Published event {EventType} ({EventId}) to RabbitMQ",
            domainEvent.EventType,
            domainEvent.EventId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null) { await _channel.CloseAsync(); _channel.Dispose(); }
        if (_connection != null) { await _connection.CloseAsync(); _connection.Dispose(); }
    }
}
