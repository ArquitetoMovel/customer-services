using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Ports;

public interface IAttendanceTicketRepository
{
    Task AddAsync(AttendanceTicket ticket);
    Task<IEnumerable<AttendanceTicket>> GetWaitingTicketsAsync();
}