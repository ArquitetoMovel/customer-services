using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;

namespace UserManagement.Domain.Ports;

public interface IAttendanceTicketRepository
{
    Task<AttendanceTicket> CreateAsync(AttendanceTicket ticket);
    Task<AttendanceTicket> GetTicketByNumberAsync(int number);
    Task<IEnumerable<AttendanceTicket>> GetAllAsync();
    Task UpdateAsync(AttendanceTicket ticket);
    Task DeleteAsync(int id);
    Task<AttendanceTicket> GetNextTicketInWaitAndUpdateToCallStatusAsync();
    Task<AttendanceTicket> GetLastTicketAsync();
    Task<long> GetWaitingTicketsCountByTypeAsync(AttendanceType type);
}