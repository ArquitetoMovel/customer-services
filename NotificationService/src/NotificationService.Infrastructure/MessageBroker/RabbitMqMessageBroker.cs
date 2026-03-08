using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Ports;
using NotificationService.Domain.Ports.TelemetryExtension;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace NotificationService.Infrastructure.MessageBroker;

public class RabbitMqMessageBroker(IConfiguration configuration) : IMessageBroker
{
    private readonly ConnectionFactory _factory = new()
    {
        HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
        UserName = configuration["RabbitMQ:UserName"] ?? "guest",
        Password = configuration["RabbitMQ:Password"] ?? "guest"
    };

    private readonly string _exchangeName = configuration["RabbitMQ:ExchangeName"] ?? "amq.direct";
    private readonly string _queueName = configuration["RabbitMQ:QueueName"] ?? "attendance_tickets";

    public async Task ConsumeTicketsAsync(Func<AttendanceTicket, Task> processTicket,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            IConnection? connection = null;
            IChannel? channel = null;

            try
            {
                connection = await _factory.CreateConnectionAsync(cancellationToken);
                channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
                await channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Fanout, durable: true,
                    cancellationToken: cancellationToken);
                await channel.QueueDeclareAsync(_queueName, durable: false, exclusive: false,
                    autoDelete: false, arguments: null, cancellationToken: cancellationToken);
                await channel.QueueBindAsync(_queueName, _exchangeName, string.Empty,
                    cancellationToken: cancellationToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var parentContext = Propagators.DefaultTextMapPropagator.Extract(
                            default, ea.BasicProperties.Headers,
                            (headers, key) => headers is not null &&
                                headers.TryGetValue(key, out var value) && value is byte[] bytes
                                    ? [Encoding.UTF8.GetString(bytes)]
                                    : []);

                        using var activity = TraceExtensions.StartActivity(
                            "Processar ticket", ActivityKind.Consumer, parentContext.ActivityContext);
                        activity?.SetTag("messaging.system", "rabbitmq");
                        activity?.SetTag("messaging.operation", "process");
                        activity?.SetTag("messaging.destination.name", _queueName);

                        var ticket = JsonSerializer.Deserialize<AttendanceTicket>(ea.Body.Span);
                        if (ticket is not null)
                        {
                            await processTicket(ticket);
                        }

                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false,
                            cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true,
                            cancellationToken: cancellationToken);
                    }
                };

                await channel.BasicConsumeAsync(_queueName, autoAck: false, consumer: consumer,
                    cancellationToken: cancellationToken);
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex) when (ex is BrokerUnreachableException or OperationInterruptedException)
            {
                Console.WriteLine($"Falha ao conectar ao RabbitMQ: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
            finally
            {
                if (channel is not null)
                {
                    await channel.DisposeAsync();
                }

                if (connection is not null)
                {
                    await connection.DisposeAsync();
                }
            }
        }
    }
}
