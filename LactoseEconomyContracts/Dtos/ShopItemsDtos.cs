using Lactose.Economy.Models;

namespace LactoseEconomyContracts.Dtos.ShopItems;

public class ShopItemDto
{
    public required string Id { get; set; }
    
    public required string UserId { get; set; }
    public required string ItemId { get; set; }
    
    public required string TransactionType { get; set; }
    public IList<UserItem> TransactionItems { get; set; } = new List<UserItem>();

    public int Quantity { get; set; } = 0;
}

public class NewShopItemDto
{
    public required string ItemId { get; set; }
    public required string TransactionType { get; set; }
    public IList<UserItem> TransactionItems { get; set; } = new List<UserItem>();
}

public class GetShopItemsRequest
{
    public required IList<string> ShopItemIds { get; set; }
}

public class GetShopItemsResponse
{
    public IList<ShopItemDto> ShopItems { get; set; } = new List<ShopItemDto>();
}

public class GetUserShopItemsRequest
{
    public required string UserId { get; set; }
    public bool RetrieveUserQuantity { get; set; } = false;
}

public class GetUserShopItemsResponse
{
    public required IList<ShopItemDto> ShopItems { get; init; }
}

public class UpdateUserShopItemsRequest
{
    public required string UserId { get; set; }
    public IList<NewShopItemDto>? NewItems { get; set; }
    public IList<string>? ItemIdsToRemove { get; set; }
}

public class UpdateUserShopItemsResponse
{
    public IList<string> AddedItems { get; set; } = new List<string>();
    public IList<string> RemovedItems { get; set; } = new List<string>();
}

public class DeleteUserShopRequest
{
    public required string UserId { get; set; }
}

public class DeleteUserShopResponse
{
    
}