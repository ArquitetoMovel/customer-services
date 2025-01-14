using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Ports;
using UserManagement.Domain.Ports.TelemetryExtension;

namespace UserManagement.Application.UseCases;

public class GenerateAttendanceTicketUseCase(IAttendanceTicketService attendanceTicketService)
{
    public async Task<AttendanceTicket> ExecuteAsync(AttendanceType type)
    {
        var ticket = await attendanceTicketService.GenerateTicketAsync(type);
        return ticket;
    }
}