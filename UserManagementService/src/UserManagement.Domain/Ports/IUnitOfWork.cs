namespace UserManagement.Domain.Ports;

public interface IUnitOfWork : IAsyncDisposable
{
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    IAttendanceTicketRepository AttendanceTickets { get; }
}