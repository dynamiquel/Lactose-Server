using Lactose.Economy.Models;
using Lactose.Economy.Transactions;
using Riok.Mapperly.Abstractions;

namespace Lactose.Economy.Mapping;

[Mapper]
public partial class TransactionMapper
{
    public static partial GetTransactionResponse ToDto(Transaction model);
}