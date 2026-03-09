using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.UseCases;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/attendance-queue")]
public class AttendanceQueueController(GetNextAttendanceTicketUseCase getNextAttendanceTicketUseCase)
    : ControllerBase
{
    [HttpGet("next")]
    public async Task<IActionResult> GetNextTicket()
    {
        var ticket = await getNextAttendanceTicketUseCase.ExecuteAsync();

        return Ok(new
        {
            TicketNumber = ticket.Number,
            ticket.Type,
            ticket.CreatedAt,
            ticket.Status,
            ticket.UpdatedAt
        });
    }
}
