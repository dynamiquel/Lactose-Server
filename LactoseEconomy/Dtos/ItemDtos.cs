namespace Lactose.Economy.Dtos.Items;

public class QueryItemsRequest;

public class QueryItemsResponse
{
    public IList<string> ItemIds { get; set; } = new List<string>();
}

public class GetItemsRequest
{
    public required IList<string> ItemIds { get; set; }
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
    public required string Type { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class UpdateItemRequest
{
    public required string ItemId { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class DeleteItemsRequest
{
    public IList<string>? ItemIds { get; set; }
}

public class DeleteItemsResponse
{
    public IList<string> ItemIds { get; set; } = new List<string>();
}