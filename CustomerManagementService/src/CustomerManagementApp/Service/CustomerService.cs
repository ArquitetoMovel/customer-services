using CustomerManagementDomain;
using CustomerManagementDomain.Entity;
using CustomerManagementDomain.Ports;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementApp.Service;

public class CustomerService(IUnitOfWork unitOfWork, ICustomerIntegrationBus integrationBus)
{
    public static async Task<IResult> GetNextCustomer(IUnitOfWork unitOfWork, ICustomerIntegrationBus integrationBus)
    {
        var service = new CustomerService(unitOfWork, integrationBus);
        var nextCustomers = await service.GetAndUpdateNextCustomers();
        return Results.Ok(nextCustomers);
    }

    private async Task<List<UserTicket>> GetAndUpdateNextCustomers()
    {
        List<UserTicket> userTickets;
        try
        {
            var strategy = unitOfWork.CreateExecutionStrategy();
            userTickets = await strategy.ExecuteAsync(async () =>
            {
                List<UserTicket> tickets = [];

                await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var nextUsers = await unitOfWork.UserTickets.GetNextAsync();
                    tickets = [.. nextUsers];

                    foreach (var userTicket in tickets)
                    {
                        userTicket.Status = StatusTicket.Called;
                        userTicket.UpdateStatus();
                        await unitOfWork.UserTickets.UpdateAsync(userTicket);
                    }
                });

                return tickets;
            });
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync();
            throw;
        }

        foreach (var userTicket in userTickets)
        {
            await integrationBus.PublishAsync(userTicket);
        }

        return userTickets;
    }
}
