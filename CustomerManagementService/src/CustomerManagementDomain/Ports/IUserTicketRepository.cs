using CustomerManagementDomain.Entity;

namespace CustomerManagementDomain.Ports;

public interface IUserTicketRepository
{
    Task AddAsync(UserTicket ticket);
    Task UpdateAsync(UserTicket ticket);
    Task<IEnumerable<UserTicket>> GetNextAsync();
}