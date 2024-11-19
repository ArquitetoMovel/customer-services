namespace UserManagement.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Domain.Entities;

public class AttendanceTicketDbContext(DbContextOptions<AttendanceTicketDbContext> options) : DbContext(options)
{
    public DbSet<AttendanceTicket> AttendanceTickets { get; set; }
}