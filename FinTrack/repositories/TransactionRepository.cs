using System;
using System.Threading.Tasks;
using FinTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Repositories
{
    /// <summary>
    /// Interface defining repository operations for <see cref="Transaction"/> entities.
    /// Provides methods for creating, finding, updating, deleting, and querying transactions.
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>
        /// Creates a new transaction in the database.
        /// </summary>
        /// <param name="transaction">The transaction to create.</param>
        /// <returns>The created transaction with its generated Id.</returns>
        public Task<Transaction> Create(Transaction transaction);

        /// <summary>
        /// Finds a transaction by its Id.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <returns>The transaction if found, or null if not found.</returns>
        public Task<Transaction?> Find(int id);

        /// <summary>
        /// Finds a transaction by its Name.
        /// </summary>
        /// <param name="name">The name of the transaction.</param>
        /// <returns>The transaction if found, or null if not found.</returns>
        public Task<Transaction?> FindByName(string name);

        /// <summary>
        /// Retrieves all transactions from the database.
        /// </summary>
        /// <returns>An array of all transactions.</returns>
        public Task<Transaction[]> FindAll();

        /// <summary>
        /// Updates an existing transaction in the database.
        /// </summary>
        /// <param name="transaction">The transaction with updated values.</param>
        public Task Update(Transaction transaction);

        /// <summary>
        /// Permanently deletes a transaction from the database.
        /// </summary>
        /// <param name="id">The Id of the transaction to delete.</param>
        public Task HardDelete(int id);

        /// <summary>
        /// Loads related data into the transaction model, such as its category.
        /// </summary>
        /// <param name="transaction">The transaction to fill with related data.</param>
        /// <returns>The transaction with its related data loaded.</returns>
        public Task<Transaction> FillModel(Transaction transaction);

        /// <summary>
        /// Selects all transactions created within a specified date range.
        /// </summary>
        /// <param name="start">Start date of the period (exclusive).</param>
        /// <param name="end">End date of the period (exclusive).</param>
        /// <returns>An array of transactions within the specified period.</returns>
        public Task<Transaction[]> SelectInPeriod(DateTime start, DateTime end);

    }

    /// <summary>
    /// Repository class for managing <see cref="Transaction"/> entities in the database.
    /// Implements <see cref="ITransactionRepository"/> using Entity Framework Core.
    /// </summary>
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRepository"/> class.
        /// </summary>
        /// <param name="db">The database context to use for operations.</param>
        public TransactionRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task<Transaction> Create(Transaction transaction)
        {
            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();
            return transaction;
        }

        /// <inheritdoc/>
        public async Task<Transaction?> Find(int id)
        {
            return await _db.Transactions.SingleOrDefaultAsync(t => t.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Transaction?> FindByName(string name)
        {
            return await _db.Transactions.SingleOrDefaultAsync(t => t.Name == name);
        }

        /// <inheritdoc/>
        public async Task<Transaction[]> FindAll()
        {
            return await _db.Transactions.ToArrayAsync();
        }

        /// <inheritdoc/>
        public async Task Update(Transaction transaction)
        {
            _db.Transactions.Update(transaction);
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task HardDelete(int id)
        {
            var transaction = await Find(id) ?? throw new Exception($"Transaction {id} not found");
            _db.Transactions.Remove(transaction);
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<Transaction> FillModel(Transaction transaction)
        {
            await _db.Entry(transaction).Reference(t => t.Category).LoadAsync();
            return transaction;
        }

        /// <inheritdoc/>
        public async Task<Transaction[]> SelectInPeriod(DateTime start, DateTime end)
        {
            var transactions = await _db.Transactions.Where(t => t.CreatedAt > start && t.CreatedAt < end).ToArrayAsync();
            return transactions;
        }

    }
}
