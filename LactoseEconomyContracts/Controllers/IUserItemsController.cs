using Lactose.Economy.Dtos.UserItems;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Economy.Controllers;

public interface IUserItemsController
{
    Task<ActionResult<QueryUserItemsResponse>> QueryUserItems(QueryUserItemsRequest request);
    Task<ActionResult<GetUserItemsResponse>> GetUserItems(GetUserItemsRequest request);
    Task<ActionResult<bool>> DeleteUserItems(GetUserItemsRequest request);
    Task<ActionResult<CreateVendorResponse>> CreateVendor(CreateVendorRequest request);
}