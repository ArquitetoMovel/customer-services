using UserManagement.Domain.Entities;
using UserManagement.Domain.Ports;

namespace UserManagement.Application.UseCases;

public class GetNextAttendanceTicketUseCase(IAttendanceTicketService attendanceTicketService)
{
    public async Task<AttendanceTicket> ExecuteAsync()
    {
        return await attendanceTicketService.GetNextTicketAsync();
    } 
}