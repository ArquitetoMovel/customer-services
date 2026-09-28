namespace CustomerManagementDomain.Ports;

public interface ICustomerIntegrationBus
{
    Task StartConsumingAsync(CancellationToken cancellationToken);
    Task StopConsumingAsync(CancellationToken cancellationToken);
}
