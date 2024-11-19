using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using UserManagement.Domain.Enums;

namespace UserManagement.Domain.Entities;

public class AttendanceTicket(int number, AttendanceType type)
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    #pragma warning disable //CS8618 - gerenciado pelo mongo
    public string _id { get; set; }
    #pragma warning disable
    public int Number { get; private set; } = (number > 0) ? number : throw new ArgumentException(nameof(number));
    public AttendanceType Type { get; private set; } = type;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public AttendanceStatus Status { get; private set; } = AttendanceStatus.Waiting;

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public void CallTicket()
    {
        Status = AttendanceStatus.Called;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelTicket()
    {
        Status = AttendanceStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteTicket()
    {
        Status = AttendanceStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }
}