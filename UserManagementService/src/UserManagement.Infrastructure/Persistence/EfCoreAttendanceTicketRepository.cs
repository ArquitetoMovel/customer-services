using UserManagement.Domain.Enums;

namespace UserManagement.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Ports;

public class EfCoreAttendanceTicketRepository(AttendanceTicketDbContext context) : IAttendanceTicketRepository
{
    public async Task<AttendanceTicket> CreateAsync(AttendanceTicket ticket)
    {
        context.AttendanceTickets.Add(ticket);
        await context.SaveChangesAsync();
        return ticket;
    }

    public async Task<AttendanceTicket> GetTicketByNumberAsync(int number)
    {
        return await context.AttendanceTickets.FindAsync(number) ?? 
               throw new InvalidOperationException("Ticket with id {id} not found");
    }

    public async Task<IEnumerable<AttendanceTicket>> GetAllAsync()
    {
        return await context.AttendanceTickets.ToListAsync();
    }

    public async Task UpdateAsync(AttendanceTicket ticket)
    {
        context.Entry(ticket).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var ticket = await context.AttendanceTickets.FindAsync(id);
        if (ticket != null)
        {
            context.AttendanceTickets.Remove(ticket);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<AttendanceTicket> GetLastTicketAsync()
    {
        return await context.AttendanceTickets
            .OrderByDescending(t => t.Number)
            .FirstOrDefaultAsync() ?? throw new InvalidOperationException();
    }

    public async Task<AttendanceTicket> GetNextTicketAsync()
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var nextTicket = await GetNextTicketOfTypeAsync(AttendanceType.Priority) ?? 
                             await GetNextTicketOfTypeAsync(AttendanceType.Normal);

            if (nextTicket != null)
            {
                nextTicket.CallTicket();
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return nextTicket;
            }

            await transaction.RollbackAsync();
            throw new InvalidOperationException("No ticket found");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<AttendanceTicket?> GetNextTicketOfTypeAsync(AttendanceType type)
    {
        return await context.AttendanceTickets
            .Where(t => t.Status == AttendanceStatus.Waiting && t.Type == type)
            .OrderBy(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }
}