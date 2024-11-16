namespace Lactose.Economy.Dtos.Items;

public class QueryItemsRequest;

public class QueryItemsResponse
{
    public IList<string> ItemIds { get; set; } = new List<string>();
}

public class GetItemsRequest
{
    public required IList<string> ItemIds { get; init; }
}

public class GetItemResponse
{
    public required string Id { get; set; }
    public required string Type { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class GetItemsResponse
{
    public IList<GetItemResponse> Items { get; set; } = new List<GetItemResponse>();
}

public class CreateItemRequest
{
    public required string Type { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public class UpdateItemRequest
{
    public required string ItemId { get; init; }
    public string? Type { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
}

public class DeleteItemsRequest
{
    public IList<string>? ItemIds { get; init; }
}

public class DeleteItemsResponse
{
    public IList<string> ItemIds { get; set; } = new List<string>();
}