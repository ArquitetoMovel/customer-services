namespace CustomerManagementDomain.Entity;

public class UserTicket
{
    public int Number { get; init; }
    public StatusTicket Status { get; set; }
    public UserType Type { get; init; } = UserType.Normal;
    public DateTime WaitTime { get; set; }
    public DateTime? CallTime { get; private set; }
    public DateTime? CompleteTime { get; private set; }
    public DateTime? CancelTime { get; private set; }

    public void UpdateStatus()
    {
        switch (Status)
        { 
            case StatusTicket.Waiting:
                WaitTime = DateTime.UtcNow;
                break;
            case StatusTicket.Called:
                CallTime = DateTime.UtcNow;
                break;
            case StatusTicket.Completed:
                CompleteTime = DateTime.UtcNow;
                break;
            case StatusTicket.Cancelled:
                CancelTime = DateTime.UtcNow;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}