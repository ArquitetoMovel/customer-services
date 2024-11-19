using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Ports;

public interface IMessageBrokerService
{
    Task PublishTicketAsync(AttendanceTicket ticket);
}