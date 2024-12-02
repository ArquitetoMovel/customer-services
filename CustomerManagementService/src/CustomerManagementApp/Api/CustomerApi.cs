using CustomerManagementDomain;
using CustomerManagementDomain.Ports;

namespace CustomerManagementApp.Api;

public static class Customer
{
    public static IEndpointRouteBuilder MapCustomerApi(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/customers");

        groupBuilder.MapGet("next", async (IUserTicketRepository repository) =>
        {
            var nextUser = await repository.GetNextAsync();
            var userTickets = nextUser.ToList();
            foreach (var userTicket in userTickets)
            {
                userTicket.Status = StatusTicket.Called;
                userTicket.UpdateStatus();
                await repository.UpdateAsync(userTicket);
            }   

            return userTickets;
        });
        
        return groupBuilder;
    }
}