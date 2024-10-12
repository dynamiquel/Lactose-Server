using Lactose.Economy.Dtos.Items;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Economy.Controllers;

public interface IItemsController
{
    Task<ActionResult<QueryItemsResponse>> QueryItems(QueryItemsRequest request);
    Task<ActionResult<GetItemsResponse>> GetItems(GetItemsRequest request);
    Task<ActionResult<GetItemResponse>> CreateItem(CreateItemRequest request);
    Task<ActionResult<GetItemResponse>> UpdateItem(UpdateItemRequest request);
    Task<ActionResult<DeleteItemsResponse>> DeleteItems(DeleteItemsRequest request);
}