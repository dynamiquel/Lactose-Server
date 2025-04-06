namespace Lactose.Events;

/// <summary>
/// Represents any event that has context of a user.
/// </summary>
public class UserEvent
{
    public required string UserId { get; set; }
}
