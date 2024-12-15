using CustomerManagementDomain;
using CustomerManagementDomain.Entity;
using CustomerManagementDomain.Ports;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementApp.Service;

public class CustomerService(IUnitOfWork unitOfWork)
{
    public static async Task<IResult> GetNextCustomer(IUnitOfWork unitOfWork)
    {
        var service = new CustomerService(unitOfWork);
        var nextCustomers = await service.GetAndUpdateNextCustomers();
        return Results.Ok(nextCustomers);
    }

    private async Task<List<UserTicket>?> GetAndUpdateNextCustomers()
    {
       //  await unitOfWork.BeginTransactionAsync();

        try
        {
            var strategy = unitOfWork.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                List<UserTicket>? userTickets = null;

                await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var nextUsers = await unitOfWork.UserTickets.GetNextAsync();
                    userTickets = nextUsers.ToList();

                    foreach (var userTicket in userTickets)
                    {
                        userTicket.Status = StatusTicket.Called;
                        userTicket.UpdateStatus();
                        await unitOfWork.UserTickets.UpdateAsync(userTicket);
                    }
                });

                return userTickets;
            }); 
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync();
            return null;
        } 
    }
}