using Lactose.Economy.Models;
using Lactose.Economy.Options;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;

namespace Lactose.Economy.Data.Repos;

public class MongoTransactionsRepo : MongoBasicKeyValueRepo<ITransactionsRepo, Transaction, TransactionsDatabaseOptions>, ITransactionsRepo
{
    public MongoTransactionsRepo(ILogger<ITransactionsRepo> logger, IOptions<TransactionsDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) { }
}