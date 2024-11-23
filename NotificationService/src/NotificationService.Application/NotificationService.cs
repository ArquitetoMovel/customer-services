using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Ports;

namespace NotificationService.Application;

public class NotificationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageBroker _messageBroker;
    public NotificationService(IServiceProvider serviceProvider, IMessageBroker messageBroker)
    {
        _serviceProvider = serviceProvider;
        _messageBroker = messageBroker;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _messageBroker.ConsumeTicketsAsync(ProcessTicketAsync, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task ProcessTicketAsync(AttendanceTicket ticket)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAttendanceTicketRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificationService>>();
        await repository.AddAsync(ticket);
        var waitingTickets = await repository.GetWaitingTicketsAsync();
        logger.LogInformation($"Tickets em espera atualizados. Total: {waitingTickets.Count()}");
        // Adicione aqui lógica adicional para notificações ou atualizações de painel
    }
}
