using MongoDB.Driver;
using UserManagement.Domain.Ports;

namespace UserManagement.Infrastructure.Persistence;

public class MongoUnitOfWork(IMongoClient client, IAttendanceTicketRepository attendanceTicketRepository)
    : IUnitOfWork
{
    private IClientSessionHandle? _session;

    public IAttendanceTicketRepository AttendanceTickets { get; } = attendanceTicketRepository;

    public async Task BeginTransactionAsync()
    {
        _session = await client.StartSessionAsync();
        _session.StartTransaction();
    }

    public async Task CommitAsync()
    {
        if (_session is not null && _session.IsInTransaction)
        {
            await _session.CommitTransactionAsync();
        }
    }

    public async Task RollbackAsync()
    {
        if (_session is not null && _session.IsInTransaction)
        {
            await _session.AbortTransactionAsync();
        }
    }
    /// <summary>
    /// No mongo o commit ou rollback já efetua o descarte do objeto
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;

    }
}
