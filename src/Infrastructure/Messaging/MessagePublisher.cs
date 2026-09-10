using System.Text;
using System.Text.Json;
using MailForge.Application.Interfaces;
using MailForge.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MailForge.Infrastructure.Messaging;

public class MessagePublisher : IMessagePublisher, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<MessagePublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();

    public MessagePublisher(IOptions<RabbitMqSettings> settings, ILogger<MessagePublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    private IModel GetChannel()
    {
        if (_channel is { IsOpen: true }) return _channel;
        lock (_lock)
        {
            if (_channel is { IsOpen: true }) return _channel;
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                DispatchConsumersAsync = true
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            foreach (var queue in new[] { "finder", "verification", "enrichment", "bulk-import", "export", "cleanup" })
            {
                _channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueDeclare(queue: $"{queue}.dlq", durable: true, exclusive: false, autoDelete: false);
            }
            return _channel;
        }
    }

    public Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
    {
        var channel = GetChannel();
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        props.MessageId = Guid.NewGuid().ToString();
        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: props, body: body);
        _logger.LogDebug("Published message to {Queue}", queueName);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
