using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Ports;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
        var connection = await _factory.CreateConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        try
        {
            await channel.QueueBindAsync(queue: _queueName,
                exchange: _exchangeName,
                routingKey: string.Empty,
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var ticket = JsonSerializer.Deserialize<AttendanceTicket>(message);

                    if (ticket != null)
                    {
                        await processTicket(ticket);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error
                    Console.WriteLine(ex);
                }
            };
            await channel.BasicConsumeAsync(_queueName, autoAck: true, consumer: consumer,
                cancellationToken: cancellationToken);

            // Keep the consumer running until cancellation is requested
            var tcs = new TaskCompletionSource<bool>();
            await using (cancellationToken.Register(() => tcs.TrySetResult(true)))
            {
                await tcs.Task;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            // Log the error and possibly retry or handle the exception
        }
        finally
        {
            channel.Dispose();
            await connection.CloseAsync(cancellationToken: cancellationToken);
        }
    }
}