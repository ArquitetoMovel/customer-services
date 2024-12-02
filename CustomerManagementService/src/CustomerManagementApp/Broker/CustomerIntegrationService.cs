using CustomerManagementDomain.Ports;

namespace CustomerManagementApp.Broker;

public class CustomerIntegrationService(ICustomerIntegrationBus customerIntegrationBus) : IHostedService
{ 
    public Task StartAsync(CancellationToken cancellationToken)
    {
        customerIntegrationBus.StartConsuming();
        Console.WriteLine("CustomerIntegrationService is starting.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        customerIntegrationBus.StopConsuming();
        Console.WriteLine("CustomerIntegrationService is stopping.");
        return Task.CompletedTask;
    }
}