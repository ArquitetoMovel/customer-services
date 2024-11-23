using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Ports;

public interface IMessageBroker
{
    Task ConsumeTicketsAsync(Func<AttendanceTicket, Task> processTicket, CancellationToken cancellationToken);
}