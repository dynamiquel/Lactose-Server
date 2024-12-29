using Lactose.Economy.Dtos.Items;
using Lactose.Economy.Models;
using Riok.Mapperly.Abstractions;

namespace Lactose.Economy.Mapping;

[Mapper]
public partial class ItemMapper
{
    public static partial GetItemResponse ToDto(Item item);

    public static GetItemsResponse ToDto(ICollection<Item> items)
    {
        return new GetItemsResponse
        {
            Items = items.Select(ToDto).ToList()
        };
    }
}