using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.UseCases;
using UserManagement.Domain.Enums;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/attendance-tickets")]
public class AttendanceTicketsController(GenerateAttendanceTicketUseCase generateAttendanceTicketUseCase)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> GenerateTicket([FromQuery] AttendanceType type = AttendanceType.Normal)
    {
        var ticket = await generateAttendanceTicketUseCase.ExecuteAsync(type);
        return Ok(new
        {
            TicketNumber = ticket.Number,
            ticket.Type,
            ticket.CreatedAt,
            ticket.Status,
            ticket.UpdatedAt,
        });
    }
}
