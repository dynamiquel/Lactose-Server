using LactoseWebApp.Repo;
using Transaction = Lactose.Economy.Models.Transaction;

namespace Lactose.Economy.Data.Repos;

public interface ITransactionsRepo : IBasicKeyValueRepo<Transaction>;