using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CustomerManagementInfra.Database;

public class UserTicketDbContextFactory : IDesignTimeDbContextFactory<UserTicketDbContext>
{
    UserTicketDbContext IDesignTimeDbContextFactory<UserTicketDbContext>.CreateDbContext(string[] args)
        => CreateDbContext(args);

    public static UserTicketDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__customer_db")
            ?? "Host=localhost:15432;Database=customerdb;Username=nuser;Password=";

        var optionsBuilder = new DbContextOptionsBuilder<UserTicketDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new UserTicketDbContext(optionsBuilder.Options);
    }
}