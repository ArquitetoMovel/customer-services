using CustomerManagementDomain.Entity;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementInfra.Database;

public class UserTicketDbContext(DbContextOptions<UserTicketDbContext> options) : DbContext(options)
{
    public DbSet<UserTicket> UserTickets { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserTicket>().HasKey(t => t.Number);
        modelBuilder.Entity<UserTicket>()
            .Property(u => u.Status)
            .HasConversion<string>();

        modelBuilder.Entity<UserTicket>()
            .Property(u => u.Type)
            .HasConversion<string>();
    }
}