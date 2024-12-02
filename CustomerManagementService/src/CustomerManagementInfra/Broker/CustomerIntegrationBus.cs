using System.Text;
using System.Text.Json;
using CustomerManagementDomain;
using CustomerManagementDomain.Entity;
using CustomerManagementDomain.Ports;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CustomerManagementInfra.Broker;

public class CustomerIntegrationBus(IConnection connection, IServiceProvider serviceProvider) : ICustomerIntegrationBus
{
    private record Ticket(int Number, int Type, DateTime CreatedAt, int Status, DateTime UpdatedAt);
    public void StartConsuming()
    {
        var channel = connection.CreateModel();
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
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
                if(repository != null)
                    await repository.AddAsync(userTicket);
            }
            Console.WriteLine($"Message received: {message}");
        };
        channel.BasicConsume(queue: "attendance_customers",
            autoAck: true,
            consumer: consumer);
    }

    public void StopConsuming()
    {
        connection.Close();
    }
}