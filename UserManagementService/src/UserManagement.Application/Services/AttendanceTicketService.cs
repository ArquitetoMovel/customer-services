using System.Data;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Ports;

namespace UserManagement.Application.Services;

public class AttendanceTicketService(IUnitOfWork unitOfWork,
                                     IMessageBrokerService messageBrokerService) : 
                IAttendanceTicketService
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private static int _lastTicketNumber = 0;
    
    public async Task<AttendanceTicket> GenerateTicketAsync(AttendanceType type)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            await Semaphore.WaitAsync();
        
            if (_lastTicketNumber == 0)
            {
                var lastTicket = await unitOfWork.AttendanceTickets.GetLastTicketAsync().ConfigureAwait(false);
                _lastTicketNumber = lastTicket?.Number ?? 0;
            }
        
            var ticketNumber = Interlocked.Increment(ref _lastTicketNumber);
            var ticket = new AttendanceTicket(ticketNumber, type);

            try
            {
                await unitOfWork.AttendanceTickets.CreateAsync(ticket).ConfigureAwait(false);
                await messageBrokerService.PublishTicketAsync(ticket).ConfigureAwait(false);
                await unitOfWork.CommitAsync();
            }
            catch
            {
                await unitOfWork.RollbackAsync();
                throw;
            }

            return ticket;
        }
        catch (Exception e)
        {
            // Use a logging framework instead of Console.WriteLine
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            Semaphore.Release();
        } 
    }
 

    public Task<AttendanceTicket> GetNextTicketAsync()
    {
        return unitOfWork.AttendanceTickets.GetNextTicketAsync();
    }
}
