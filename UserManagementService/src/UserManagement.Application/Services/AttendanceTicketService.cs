using System.Data;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Ports;
using UserManagement.Domain.Ports.TelemetryExtension;

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
                await unitOfWork.AttendanceTickets.GetWaitingTicketsCountByTypeAsync(ticket.Type);
                await unitOfWork.CommitAsync();
                
                MetricsExtension.IncrementAttendanceCount(ticket.Type.ToString());
                MetricsExtension.IncrementActiveAttendances(ticket.Type.ToString());
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
        var ticket = unitOfWork.AttendanceTickets.GetNextTicketInWaitAndUpdateToCallStatusAsync();
        MetricsExtension.DecrementActiveAttendances(ticket
                                                    .GetAwaiter()
                                                    .GetResult()
                                                    .Type
                                                    .ToString());
        return ticket;
    }
}
