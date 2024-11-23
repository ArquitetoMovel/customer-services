using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

public class AttendanceTicket
{
    public int Number { get; set; }
    public AttendanceType Type { get; set; }
    public AttendanceStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}