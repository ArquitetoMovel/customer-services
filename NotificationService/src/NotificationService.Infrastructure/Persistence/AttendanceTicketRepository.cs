using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Ports;

namespace NotificationService.Infrastructure.Persistence;

public class AttendanceTicketRepository(NotificationDbContext context) : IAttendanceTicketRepository
{
    public async Task AddAsync(AttendanceTicket ticket)
    {
        await context.AttendanceTickets.AddAsync(ticket);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AttendanceTicket>> GetWaitingTicketsAsync()
    {
        return await context.AttendanceTickets
            .Where(t => t.Status == AttendanceStatus.Waiting)
            .OrderBy(t => t.Type)
            .ThenBy(t => t.Number)
            .ToListAsync();
    }
}