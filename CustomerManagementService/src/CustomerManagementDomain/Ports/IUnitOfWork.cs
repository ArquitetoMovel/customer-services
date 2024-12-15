using Microsoft.EntityFrameworkCore.Storage;

namespace CustomerManagementDomain.Ports;

public interface IUnitOfWork : IDisposable
{
    IUserTicketRepository UserTickets { get; }
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task SaveChangesAsync();
    IExecutionStrategy CreateExecutionStrategy();
    Task ExecuteInTransactionAsync(Func<Task> action);
}