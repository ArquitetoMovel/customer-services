using CustomerManagementDomain.Entity;
namespace CustomerManagementDomain.Ports;

public interface ICustomerIntegrationBus
{
    Task StartConsumingAsync(CancellationToken cancellationToken);
    Task StopConsumingAsync(CancellationToken cancellationToken);
    Task PublishAsync(UserTicket userTicket, CancellationToken cancellationToken = default);
}
