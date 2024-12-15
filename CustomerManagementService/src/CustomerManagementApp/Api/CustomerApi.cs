using CustomerManagementApp.Service;

namespace CustomerManagementApp.Api;

public static class Customer
{
    public static IEndpointRouteBuilder MapCustomerApi(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/customers");
        groupBuilder.MapGet("next", CustomerService.GetNextCustomer);
        return groupBuilder;
    }
}