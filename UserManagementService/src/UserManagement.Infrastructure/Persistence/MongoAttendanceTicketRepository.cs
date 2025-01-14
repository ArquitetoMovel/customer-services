using UserManagement.Domain.Enums;

namespace UserManagement.Infrastructure.Persistence;

using MongoDB.Driver;
using Domain.Entities;
using Domain.Ports;
using UserManagement.Domain.Ports.TelemetryExtension;

public class MongoAttendanceTicketRepository(IMongoDatabase database) : IAttendanceTicketRepository
{
    private readonly IMongoCollection<AttendanceTicket> _tickets =
        database.GetCollection<AttendanceTicket>("AttendanceTickets");

    public async Task<AttendanceTicket> CreateAsync(AttendanceTicket ticket)
    {
        using var activity = TracesExtension.StartActivity("Create Ticket");
        await _tickets.InsertOneAsync(ticket);
        return ticket;
    }

    public async Task<AttendanceTicket> GetTicketByNumberAsync(int number)
    {
        return await _tickets.Find(t => t.Number == number)
                    .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AttendanceTicket>> GetAllAsync()
    {
        return await _tickets.Find(_ => true).ToListAsync();
    }

    public async Task UpdateAsync(AttendanceTicket ticket)
    {
        await _tickets.ReplaceOneAsync(t => t.Number == ticket.Number, ticket);
        await GetWaitingTicketsCountByTypeAsync(ticket.Type);
    }

    public async Task DeleteAsync(int id)
    {
        await _tickets.DeleteOneAsync(t => t.Number == id);
    }
    
    public async Task<AttendanceTicket> GetNextTicketInWaitAndUpdateToCallStatusAsync()
    {
        using var activity = TracesExtension.StartActivity("Get Next Ticket");
        var nextTicket = (await GetNextTicketOfTypeAsync(AttendanceType.Priority) ??
                         await GetNextTicketOfTypeAsync(AttendanceType.Normal)) ?? 
                         throw new InvalidOperationException("No ticket found");
        nextTicket.CallTicket();
        activity?.AddTag("Ticket Number", nextTicket.Number);
        using var childActivity = activity?.StartChildActivity("Update Ticket");
        await UpdateAsync(nextTicket);
        
        return nextTicket; 
    }
    
    public async Task<AttendanceTicket> GetLastTicketAsync()
    {
        return await _tickets.Find(_ => true)
            .SortByDescending(t => t.Number)
            .FirstOrDefaultAsync();
    }
    
    private async Task<AttendanceTicket?> GetNextTicketOfTypeAsync(AttendanceType type)
    {

        var nextTicket = await _tickets.Find(t => t.Type == type && 
                                                  t.Status == AttendanceStatus.Waiting)
            .SortBy(t => t.Number)
            .FirstOrDefaultAsync();

            return nextTicket;
    }
    
    public async Task<long> GetWaitingTicketsCountByTypeAsync(AttendanceType type)
    {
        using var activity = TracesExtension.StartActivity("Get Waiting Tickets Count By Type");
        activity?.AddTag("attendance.type", type.ToString());
        
        var filter = Builders<AttendanceTicket>.Filter.And(
            Builders<AttendanceTicket>.Filter.Eq(t => t.Status, AttendanceStatus.Waiting),
            Builders<AttendanceTicket>.Filter.Eq(t => t.Type, type)
        );
        
        var count = await _tickets.CountDocumentsAsync(filter);
        MetricsExtension.SetWaitingAttendances(count, type.ToString().ToLower());
        
        return count;
    }

}