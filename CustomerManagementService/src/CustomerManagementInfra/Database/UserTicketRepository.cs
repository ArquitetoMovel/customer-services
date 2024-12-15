using CustomerManagementDomain;
using CustomerManagementDomain.Entity;
using CustomerManagementDomain.Ports;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementInfra.Database;

public class UserTicketRepository(UserTicketDbContext dbContext) : IUserTicketRepository
{
    public async Task AddAsync(UserTicket ticket)
    {
        await dbContext.UserTickets.AddAsync(ticket);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserTicket ticket)
    {
        dbContext.UserTickets.Update(ticket);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserTicket>> GetNextAsync()
    {
        return await dbContext.UserTickets
            .Where(t => t.Status == StatusTicket.Waiting)
            .OrderByDescending(t => t.Type)
            .ThenBy(t => t.Number) 
            .Take(1)
            .ToListAsync();
    }
}