using CustomerManagementDomain.Ports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CustomerManagementInfra.Database;

public class UnitOfWork(UserTicketDbContext context, IUserTicketRepository userTicketRepository)
    : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    public IUserTicketRepository UserTickets { get; } = userTicketRepository;
    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            if (_transaction == null)
                throw new NullReferenceException("A transaction has not been started. Call BeginTransactionAsync() first.");
            
            await SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction == null)
                throw new NullReferenceException("A transaction has not been started. Call BeginTransactionAsync() first.");
        
        await _transaction.RollbackAsync();
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
    
    public IExecutionStrategy CreateExecutionStrategy()
    {
        return context.Database.CreateExecutionStrategy();
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        var strategy = CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await action();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }


    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }
}