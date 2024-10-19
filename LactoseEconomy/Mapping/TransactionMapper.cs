using Lactose.Economy.Dtos.Transactions;
using Lactose.Economy.Models;
using Riok.Mapperly.Abstractions;

namespace Lactose.Identity.Mapping;

[Mapper]
public partial class TransactionMapper
{
    public static partial GetTransactionResponse ToDto(Transaction model);
}