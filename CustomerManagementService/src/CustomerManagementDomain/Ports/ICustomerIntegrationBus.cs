using RabbitMQ.Client;

namespace CustomerManagementDomain.Ports;

public interface ICustomerIntegrationBus
{
  void StartConsuming();   
  void StopConsuming();   
}