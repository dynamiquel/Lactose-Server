namespace Lactose.Identity.Dtos.Users;

public class QueryUsersResponse
{
    public IList<string> UserIds { get; set; } = new List<string>();
}