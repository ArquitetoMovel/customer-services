using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CustomerManagementInfra.Database;

public class UserTicketDbContextFactory : IDesignTimeDbContextFactory<UserTicketDbContext>
{
    public UserTicketDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserTicketDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost:15432;Database=customerdb;Username=nuser;Password=npass1");

        return new UserTicketDbContext(optionsBuilder.Options);
    }
}