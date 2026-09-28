using System.Text;
using System.Text.Json;
using CustomerManagementDomain;
using CustomerManagementDomain.Entity;
using CustomerManagementDomain.Ports;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace CustomerManagementInfra.Broker;

public class CustomerIntegrationBus(IConnection connection, IServiceProvider serviceProvider) : ICustomerIntegrationBus
{
    private sealed record Ticket(int Number, int Type, DateTime CreatedAt, int Status, DateTime UpdatedAt);
    private const string ExchangeName = "customer.exchange";
    private const string QueueName = "attendance_customers";
    private IChannel? channel;

    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        try
        {
            channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Fanout, durable: true,
                cancellationToken: cancellationToken);
            await channel.QueueDeclareAsync(QueueName, durable: false, exclusive: false,
                autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            await channel.QueueBindAsync(QueueName, ExchangeName, string.Empty,
                cancellationToken: cancellationToken);
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                var ticket = JsonSerializer.Deserialize<Ticket>(message);
                var userTicket = new UserTicket
                {
                    Number = ticket!.Number,
                    Type = (UserType)ticket.Type,
                    Status = (StatusTicket)ticket.Status,
                    WaitTime = ticket.CreatedAt
                };
                using (var repositoryScope = serviceProvider.CreateScope())
                {
                    var repository = repositoryScope.ServiceProvider.GetService<IUserTicketRepository>();
                    if (repository != null)
                        await repository.AddAsync(userTicket);
                }

                Console.WriteLine($"Message received: {message}");
            };
            await channel.BasicConsumeAsync(queue: QueueName,
                    autoAck: true,
                    consumer: consumer,
                    cancellationToken: cancellationToken);
        }
        catch (Exception ex) 
            when (ex is BrokerUnreachableException or OperationInterruptedException) 
        {
            Console.WriteLine("Falha ao conectar ao RabbitMQ. Tentando novamente em 5 segundos...");
            Console.WriteLine(ex.Message);
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }
    }

    public async Task StopConsumingAsync(CancellationToken cancellationToken)
    {
        if (channel is not null)
        {
            await channel.CloseAsync(cancellationToken);
            await channel.DisposeAsync();
        }

        await connection.CloseAsync(cancellationToken);
    }
}
