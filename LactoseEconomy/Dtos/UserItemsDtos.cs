using Lactose.Economy.Models;

namespace Lactose.Economy.Dtos.UserItems;

public class QueryUserItemsRequest;

public class QueryUserItemsResponse
{
    public IList<string> UserIds { get; set; } = new List<string>();
}

public class GetUserItemsRequest
{
    public required string UserId { get; init; }
}

public class GetUserItemsResponse
{
    public IList<UserItem> Items { get; set; } = new List<UserItem>();
}