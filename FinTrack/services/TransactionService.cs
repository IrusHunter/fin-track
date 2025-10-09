using FinTrack.Models;
using FinTrack.Repositories;
using System;
using System.Threading.Tasks;

namespace FinTrack.Services
{
    /// <summary>
    /// Interface defining service operations for <see cref="Transaction"/> entities.
    /// Provides methods for creating, finding, updating, deleting transactions,
    /// and calculating company balance and profits.
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Creates a new transaction with validation and tax calculation.
        /// </summary>
        /// <param name="transaction">The transaction to create.</param>
        /// <returns>The created transaction.</returns>
        public Task<Transaction> Create(Transaction transaction);

        /// <summary>
        /// Finds a transaction by its Id.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <returns>The transaction if found, or null if not found.</returns>
        public Task<Transaction?> Find(int id);

        /// <summary>
        /// Retrieves all transactions.
        /// </summary>
        /// <returns>An array of all transactions.</returns>
        public Task<Transaction[]> FindAll();

        /// <summary>
        /// Updates an existing transaction with validation and tax recalculation.
        /// </summary>
        /// <param name="transaction">The transaction with updated values.</param>
        public Task Update(Transaction transaction);

        /// <summary>
        /// Deletes a transaction.
        /// A transaction can only be deleted if more than 14 days have passed since its creation.
        /// </summary>
        /// <param name="id">The Id of the transaction to delete.</param>
        public Task Delete(int id);

        /// <summary>
        /// Calculates the company's balance based on all transactions.
        /// </summary>
        /// <returns>The total balance after tax.</returns>
        public Task<decimal> GetCompanyBalance();

        /// <summary>
        /// Calculates the profit for a specific month and year.
        /// </summary>
        /// <param name="month">The month (1-12).</param>
        /// <param name="year">The year.</param>
        /// <returns>The profit for the specified month.</returns>
        public Task<decimal> GetMonthProfit(int month, int year);

        /// <summary>
        /// Retrieves all transactions for a specific month and year.
        /// </summary>
        /// <param name="month">The month (1-12).</param>
        /// <param name="year">The year.</param>
        /// <returns>An array of transactions for the specified month.</returns>
        public Task<Transaction[]> GetMonthTransactions(int month, int year);

        /// <summary>
        /// Calculates the profit for a specific date range.
        /// </summary>
        /// <param name="start">Start date of the period (inclusive).</param>
        /// <param name="end">End date of the period (inclusive).</param>
        /// <returns>The profit for the specified period.</returns>
        public Task<decimal> GetPeriodProfit(DateTime start, DateTime end);

        /// <summary>
        /// Retrieves all transactions within a specific date range.
        /// </summary>
        /// <param name="start">Start date of the period (inclusive).</param>
        /// <param name="end">End date of the period (inclusive).</param>
        /// <returns>An array of transactions within the specified period.</returns>
        public Task<Transaction[]> GetPeriodTransactions(DateTime start, DateTime end);
    }

    /// <summary>
    /// Service class implementing business logic for <see cref="Transaction"/> entities.
    /// Handles validation, tax calculation, and profit computations.
    /// Uses <see cref="ITransactionRepository"/> and <see cref="ICategoryRepository"/> for data access.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionService"/> class.
        /// </summary>
        /// <param name="transactionRepository">The repository used for transaction data access.</param>
        /// <param name="categoryRepository">The repository used for category data access.</param>
        public TransactionService(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
        }

        /// <inheritdoc/>
        public async Task<Transaction> Create(Transaction transaction)
        {
            await Validate(transaction);

            transaction.CreatedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;
            await CalculateTax(transaction);

            return await _transactionRepository.Create(transaction);
        }

        /// <inheritdoc/>
        public async Task<Transaction?> Find(int id)
        {
            return await _transactionRepository.Find(id);
        }

        /// <inheritdoc/>
        public async Task<Transaction[]> FindAll()
        {
            return await _transactionRepository.FindAll();
        }

        /// <inheritdoc/>
        public async Task Update(Transaction transaction)
        {
            await Validate(transaction);
            transaction.UpdatedAt = DateTime.UtcNow;
            await CalculateTax(transaction);
            await _transactionRepository.Update(transaction);
        }

        /// <inheritdoc/>
        public async Task Delete(int id)
        {
            var transaction = await _transactionRepository.Find(id) ?? throw new Exception($"Transaction {id} not found");
            if ((DateTime.UtcNow - transaction.CreatedAt).TotalDays >= 14)
            {
                throw new Exception("You can delete transaction only before 14 days from creation date.");
            }
            else
            {
                await _transactionRepository.HardDelete(id);
            }
        }

        /// <summary>
        /// Validates the transaction before creation or update.
        /// Checks for required fields, name length, and category existence.
        /// </summary>
        /// <param name="transaction">The transaction to validate.</param>
        private async Task Validate(Transaction transaction)
        {
            if (string.IsNullOrWhiteSpace(transaction.Name))
                throw new Exception("Transaction name is required.");

            if (transaction.Name.Length > Transaction.MaxNameLength)
                throw new Exception($"Transaction name ({transaction.Name.Length}) is longer than {Transaction.MaxNameLength}");

            if (await _categoryRepository.Find(transaction.CategoryId) == null)
                throw new Exception($"Category {transaction.CategoryId} not found.");
        }

        /// <summary>
        /// Calculates the after-tax amount for the transaction based on its category.
        /// </summary>
        /// <param name="transaction">The transaction for which to calculate tax.</param>
        private async Task CalculateTax(Transaction transaction)
        {
            var category = await _categoryRepository.Find(transaction.CategoryId) ?? throw new Exception($"Category {transaction.CategoryId} not found.");

            var sumAfterTax = transaction.Sum;
            switch (category.TaxType)
            {
                case TaxType.GeneralTax:
                    sumAfterTax = transaction.Sum - transaction.Sum * category.TaxAmount / 100;
                    break;
                case TaxType.ExpenseTax:
                    if (transaction.Sum < 0)
                    {
                        sumAfterTax = transaction.Sum + transaction.Sum * category.TaxAmount / 100;
                    }
                    break;
            }
            transaction.SumAfterTax = sumAfterTax;
        }

        /// <inheritdoc/>
        public async Task<decimal> GetCompanyBalance()
        {
            return CalculateProfit(await _transactionRepository.FindAll());
        }

        /// <inheritdoc/>
        public async Task<decimal> GetMonthProfit(int month, int year)
        {
            return CalculateProfit(await GetMonthTransactions(month, year));
        }

        /// <inheritdoc/>
        public async Task<Transaction[]> GetMonthTransactions(int month, int year)
        {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            return await _transactionRepository.SelectInPeriod(start, end);
        }

        /// <inheritdoc/>
        public async Task<decimal> GetPeriodProfit(DateTime start, DateTime end)
        {
            return CalculateProfit(await GetPeriodTransactions(start, end));
        }

        /// <inheritdoc/>
        public async Task<Transaction[]> GetPeriodTransactions(DateTime start, DateTime end)
        {
            start = start.ToUniversalTime();
            end = end.ToUniversalTime();

            return await _transactionRepository.SelectInPeriod(start, end);
        }

        /// <inheritdoc/>
        private static decimal CalculateProfit(Transaction[] transactions)
        {
            decimal result = 0M;
            foreach (var tr in transactions)
            {
                result += tr.SumAfterTax;
            }
            return result;
        }
    }
}