using Microsoft.EntityFrameworkCore.Storage;
using UserManagement.Domain.Ports;

namespace UserManagement.Infrastructure.Persistence;

public class EfCoreUnitOfWork(AttendanceTicketDbContext context, IAttendanceTicketRepository attendanceTicketRepository)
    : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IAttendanceTicketRepository AttendanceTickets { get; } = attendanceTicketRepository;

    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            if (_transaction is not null)
                await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
        await context.DisposeAsync();
    }
}