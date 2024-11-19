using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Ports;

namespace UserManagement.Application.UseCases;

public class GenerateAttendanceTicketUseCase(IAttendanceTicketService attendanceTicketService)
{
    public async Task<AttendanceTicket> ExecuteAsync(AttendanceType type)
    {
        return await attendanceTicketService.GenerateTicketAsync(type);
    }
}