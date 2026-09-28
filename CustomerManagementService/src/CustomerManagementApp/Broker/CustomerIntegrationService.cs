using CustomerManagementDomain.Ports;

namespace CustomerManagementApp.Broker;

public class CustomerIntegrationService(ICustomerIntegrationBus customerIntegrationBus) : IHostedService
{ 
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await customerIntegrationBus.StartConsumingAsync(cancellationToken);
        Console.WriteLine("CustomerIntegrationService is starting.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await customerIntegrationBus.StopConsumingAsync(cancellationToken);
        Console.WriteLine("CustomerIntegrationService is stopping.");
    }
}
