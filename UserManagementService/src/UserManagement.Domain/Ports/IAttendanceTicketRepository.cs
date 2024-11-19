using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Ports;

public interface IAttendanceTicketRepository
{
    Task<AttendanceTicket> CreateAsync(AttendanceTicket ticket);
    Task<AttendanceTicket> GetTicketByNumberAsync(int number);
    Task<IEnumerable<AttendanceTicket>> GetAllAsync();
    Task UpdateAsync(AttendanceTicket ticket);
    Task DeleteAsync(int id);
    Task<AttendanceTicket> GetNextTicketAsync();
    Task<AttendanceTicket> GetLastTicketAsync();
}