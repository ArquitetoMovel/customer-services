using UserManagement.Domain.Entities;
using UserManagement.Domain.Ports;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using UserManagement.Domain.Ports.TelemetryExtension;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using OpenTelemetry;

namespace UserManagement.Infrastructure.MessageBroker;

public class RabbitMqService(IConfiguration configuration) : IMessageBrokerService, IAsyncDisposable
{
    private readonly ConnectionFactory _factory = new()
    {
        HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
        UserName = configuration["RabbitMQ:UserName"] ?? "admin",
        Password = configuration["RabbitMQ:Password"] ?? "adminpassword",
    };
    private IConnection? _connection;
    private IChannel? _channel;
    private const string NotificationQueue = "attendance_tickets";
    private const string CustomerQueue = "attendance_customers";
    private const string CustomerExchange = "customer.exchange";
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private async Task EnsureConnectionAndChannelAsync()
    {
        if (_connection?.IsOpen != true)
        {
            _connection = await _factory.CreateConnectionAsync();
        }

        if (_channel?.IsOpen != true)
        {
            _channel = await _connection.CreateChannelAsync();
            await _channel.ExchangeDeclareAsync(CustomerExchange, ExchangeType.Fanout, true);
            await _channel.QueueDeclareAsync(
                queue: NotificationQueue, 
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            await _channel.QueueDeclareAsync(
                queue: CustomerQueue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            await _channel.QueueBindAsync(NotificationQueue, CustomerExchange, string.Empty);
            await _channel.QueueBindAsync(CustomerQueue, CustomerExchange, string.Empty);
        }
    }

    public async Task PublishTicketAsync(AttendanceTicket ticket)
        {
            await _semaphore.WaitAsync();
            try
            {
                await EnsureConnectionAndChannelAsync();

                var message = JsonSerializer.Serialize(ticket);
                var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message));

                if (_channel != null)
                {
                    using var tracePublish = TracesExtension.StartActivity("Publishing Ticket", ActivityKind.Producer);
                    tracePublish?.SetTag("messaging.system", "rabbitmq");

                    var properties = new BasicProperties
                    {
                        ContentType = "application/json",
                        DeliveryMode  = DeliveryModes.Persistent,
                        Headers = new Dictionary<string, object?>()
                    };

                    if (tracePublish != null)
                    {
                        Propagators.DefaultTextMapPropagator.Inject(
                            new PropagationContext(tracePublish.Context, Baggage.Current),
                            properties.Headers,
                            (carrier, key, value) => carrier[key] = value);

                    }

                    await _channel.BasicPublishAsync(
                        exchange: CustomerExchange,
                        routingKey: string.Empty,
                        mandatory: false,
                        basicProperties: properties,
                        body: body
                    );
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}
