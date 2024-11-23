using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{

    public DbSet<AttendanceTicket> AttendanceTickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttendanceTicket>().HasKey(t => t.Number);
        modelBuilder.Entity<AttendanceTicket>().Property(t => t.Type).HasConversion<string>();
        modelBuilder.Entity<AttendanceTicket>().Property(t => t.Status).HasConversion<string>();
    }
}