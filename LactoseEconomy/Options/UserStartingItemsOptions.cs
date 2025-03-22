using Lactose.Economy.Models;
using LactoseWebApp.Options;

namespace Lactose.Economy.Options;

[Options(SectionName = "UserItems:StartingItems")]
public class UserStartingItemsOptions
{
    public List<UserItem> StartingUserItems { get; set; } = new();
}