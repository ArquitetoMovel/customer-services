using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;

namespace UserManagement.IntegrationTests;

public class AttendanceTicketIntegrationTests
{
    [Fact]
    public void AttendanceTicket_WhenCreatedWithValidNumber_ShouldHaveWaitingStatus()
    {
        // Arrange & Act
        var ticket = new AttendanceTicket(1, AttendanceType.Normal);

        // Assert
        Assert.Equal(AttendanceStatus.Waiting, ticket.Status);
    }

    [Fact]
    public void AttendanceTicket_WhenCallTicketInvoked_ShouldTransitionToCalledStatus()
    {
        // Arrange
        var ticket = new AttendanceTicket(2, AttendanceType.Priority);

        // Act
        ticket.CallTicket();

        // Assert
        Assert.Equal(AttendanceStatus.Called, ticket.Status);
    }

    [Fact]
    public void AttendanceTicket_WhenCreatedWithZeroNumber_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new AttendanceTicket(0, AttendanceType.Normal));
    }
}