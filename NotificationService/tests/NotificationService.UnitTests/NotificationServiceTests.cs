using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Ports;

namespace NotificationService.UnitTests;

public class NotificationServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldPersistConsumedTicket()
    {
        var ticket = new AttendanceTicket { Number = 42 };
        var repository = new RecordingRepository();
        using var services = new ServiceCollection()
            .AddSingleton<IAttendanceTicketRepository>(repository)
            .AddSingleton<ILogger<NotificationService.Application.NotificationService>>(
                NullLogger<NotificationService.Application.NotificationService>.Instance)
            .BuildServiceProvider();
        using var service = new NotificationService.Application.NotificationService(
            services, new SingleTicketBroker(ticket));

        await service.StartAsync(CancellationToken.None);
        var savedTicket = await repository.SavedTicket.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await service.StopAsync(CancellationToken.None);

        Assert.Same(ticket, savedTicket);
    }

    private sealed class SingleTicketBroker(AttendanceTicket ticket) : IMessageBroker
    {
        public Task ConsumeTicketsAsync(Func<AttendanceTicket, Task> processTicket, CancellationToken cancellationToken)
            => processTicket(ticket);
    }

    private sealed class RecordingRepository : IAttendanceTicketRepository
    {
        public TaskCompletionSource<AttendanceTicket> SavedTicket { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task AddAsync(AttendanceTicket ticket)
        {
            SavedTicket.SetResult(ticket);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<AttendanceTicket>> GetWaitingTicketsAsync()
            => Task.FromResult<IEnumerable<AttendanceTicket>>([]);
    }
}
