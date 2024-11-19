using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;

namespace UserManagement.UnitTests;

public class AttendanceTicketTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        const int number = 1;
        const AttendanceType type = AttendanceType.Normal;

        // Act
        var ticket = new AttendanceTicket(number, type);

        // Assert
        Assert.Equal(number, ticket.Number);
        Assert.Equal(type, ticket.Type);
        Assert.Equal(AttendanceStatus.Waiting, ticket.Status);
        Assert.True(DateTime.UtcNow.Subtract(ticket.CreatedAt).TotalSeconds < 1);
    }

    [Fact]
    public void MarkAsServed_ShouldChangeStatusToServed()
    {
        // Arrange
        var ticket = new AttendanceTicket(1, AttendanceType.Normal);

        // Act
        ticket.CompleteTicket();

        // Assert
        Assert.Equal(AttendanceStatus.Completed, ticket.Status);
    }

    [Theory]
    [InlineData(AttendanceType.Normal)]
    [InlineData(AttendanceType.Priority)]
    public void Constructor_ShouldSetCorrectType(AttendanceType type)
    {
        // Arrange & Act
        var ticket = new AttendanceTicket(1, type);
            ticket.CancelTicket();
            
        // Assert
        Assert.Equal(type, ticket.Type);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenNumberIsZeroOrNegative()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new AttendanceTicket(0, AttendanceType.Normal));
        Assert.Throws<ArgumentException>(() => new AttendanceTicket(-1, AttendanceType.Priority));
    } 
}