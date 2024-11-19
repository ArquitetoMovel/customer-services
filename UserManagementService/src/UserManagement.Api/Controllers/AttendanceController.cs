using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.UseCases;
using UserManagement.Domain.Enums;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController(GenerateAttendanceTicketUseCase generateAttendanceTicketUseCase,
    GetNextAttendanceTicketUseCase getNextAttendanceTicketUseCase)
    : ControllerBase
{
    [HttpPost("generate-ticket")]
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
    
    [HttpGet("next-ticket")]
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