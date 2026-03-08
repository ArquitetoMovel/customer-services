using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.UseCases;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/attendance-queue")]
public class AttendanceQueueController(GetNextAttendanceTicketUseCase getNextAttendanceTicketUseCase,
    ILogger<AttendanceQueueController> logger)
    : ControllerBase
{
    [HttpGet("next")]
    public async Task<IActionResult> GetNextTicket()
    {
        var ticket = await getNextAttendanceTicketUseCase.ExecuteAsync();
        logger.LogInformation("Selected attendance ticket {TicketNumber}", ticket.Number);

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
