using System;
using System.Threading.Tasks;
using FinTrack.Models;
using Microsoft.EntityFrameworkCore;
using FinTrack.Models.ViewModels;
using System.Linq;

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

        public Task<Transaction[]> Search(TransactionSearchViewModel searchModel);

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
            return await _db.Transactions.Include(t => t.Category).ToArrayAsync();
        }

        /// <inheritdoc/>
        public async Task Update(Transaction transaction)
        {
            var existing = await _db.Transactions.FindAsync(transaction.Id);
            if (existing == null)
                throw new Exception("Transaction not found");

            existing.Name = transaction.Name;
            existing.Sum = transaction.Sum;
            existing.SumAfterTax = transaction.SumAfterTax;
            existing.CategoryId = transaction.CategoryId;
            existing.UpdatedAt = transaction.UpdatedAt;

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

        public async Task<Transaction[]> Search(TransactionSearchViewModel searchModel)
        {
            var query = _db.Transactions
                .Include(t => t.Category) // Перший JOIN (Category) (iv)
                .Include(t => t.User)     // Другий JOIN (ApplicationUser) (iv) - *ПЕРЕКОНАЙТЕСЯ, ЩО ВИ ДОДАЛИ ПОЛЕ UserId/User ДО МОДЕЛІ TRANSACTION*
                .AsQueryable();

            // Фільтрація за датою (i)
            DateTime? startDateUtc = null;
            if (searchModel.StartDate.HasValue)
            {
                var localDate = DateTime.SpecifyKind(searchModel.StartDate.Value, DateTimeKind.Local);
                startDateUtc = localDate.ToUniversalTime();
                query = query.Where(t => t.CreatedAt >= startDateUtc);
            }

            DateTime? endDateUtc = null;
            if (searchModel.EndDate.HasValue)
            {
                DateTime endDate = searchModel.EndDate.Value;
                if (endDate.Hour == 0 && endDate.Minute == 0 && endDate.Second == 0)
                {
                    endDate = endDate.AddDays(1);
                }

                var localDate = DateTime.SpecifyKind(endDate, DateTimeKind.Local);
                endDateUtc = localDate.ToUniversalTime();

                query = query.Where(t => t.CreatedAt <= endDateUtc);
            }

            // Фільтрація за списком CategoryIds (ii)
            if (searchModel.CategoryIds != null && searchModel.CategoryIds.Any())
            {
                query = query.Where(t => searchModel.CategoryIds.Contains(t.CategoryId));
            }

            // Фільтрація за початком/кінцем Name (iii)
            if (!string.IsNullOrEmpty(searchModel.NameStart))
            {
                query = query.Where(t => t.Name.StartsWith(searchModel.NameStart));
            }
            if (!string.IsNullOrEmpty(searchModel.NameEnd))
            {
                query = query.Where(t => t.Name.EndsWith(searchModel.NameEnd));
            }

            // Фільтрація у залежних таблицях (iv)
            if (searchModel.CategoryTaxType.HasValue)
            {
                query = query.Where(t => t.Category != null && t.Category.TaxType == searchModel.CategoryTaxType.Value);
            }
            if (!string.IsNullOrEmpty(searchModel.UserNameFilter))
            {
                query = query.Where(t => t.User != null && t.User.UserName.Contains(searchModel.UserNameFilter));
            }

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .ToArrayAsync();
        }

    }
}
