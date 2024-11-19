using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;

namespace UserManagement.Domain.Ports;

public interface IAttendanceTicketService
{
    Task<AttendanceTicket> GenerateTicketAsync(AttendanceType type);
    Task<AttendanceTicket> GetNextTicketAsync();
}