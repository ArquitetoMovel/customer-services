using Microsoft.EntityFrameworkCore.Metadata;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Ports;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

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
                await _channel.BasicPublishAsync(
                    exchange: CustomerExchange,
                    routingKey: string.Empty,
                    body: body
                );
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

