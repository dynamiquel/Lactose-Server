namespace Lactose.Identity.Dtos.Users;

public class QueryUsersRequest;

public class QueryUsersResponse
{
    public IList<string> UserIds { get; set; } = new List<string>();
}

public class UserRequest
{
    public required string UserId { get; set; }
}

public class UserResponse
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime TimeCreated { get; set; }
    public DateTime TimeLastLoggedIn { get; set; }
}

public class CreateUserRequest
{
    public required string DisplayName { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}