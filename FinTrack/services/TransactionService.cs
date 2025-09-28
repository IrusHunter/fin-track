using FinTrack.Models;
using FinTrack.Repositories;
using System;
using System.Threading.Tasks;

namespace FinTrack.Services
{
    public interface ITransactionService
    {
        public Task<Transaction> Create(Transaction transaction);
        public Task<Transaction?> Find(int id);
        public Task<Transaction[]> FindAll();
        public Task Update(Transaction transaction);
        public Task Delete(int id);
        public Task<decimal> GetCompanyBalance();
        public Task<decimal> GetMonthProfit(int month, int year);
        public Task<Transaction[]> GetMonthTransactions(int month, int year);
        public Task<decimal> GetPeriodProfit(DateTime start, DateTime end);
        public Task<Transaction[]> GetPeriodTransactions(DateTime start, DateTime end);
    }

    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;

        public TransactionService(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<Transaction> Create(Transaction transaction)
        {
            await Validate(transaction);

            transaction.CreatedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;
            await CalculateTax(transaction);

            return await _transactionRepository.Create(transaction);
        }

        public async Task<Transaction?> Find(int id)
        {
            return await _transactionRepository.Find(id);
        }

        public async Task<Transaction[]> FindAll()
        {
            return await _transactionRepository.FindAll();
        }

        public async Task Update(Transaction transaction)
        {
            await Validate(transaction);
            transaction.UpdatedAt = DateTime.UtcNow;
            await CalculateTax(transaction);
            await _transactionRepository.Update(transaction);
        }

        public async Task Delete(int id)
        {
            var transaction = await _transactionRepository.Find(id) ?? throw new Exception($"Transaction {id} not found");
            if ((DateTime.UtcNow - transaction.CreatedAt).TotalDays <= 14)
            {
                throw new Exception("You can delete transaction only after 14 days from creation date.");
            }
            else
            {
                await _transactionRepository.HardDelete(id);
            }
        }

        private async Task Validate(Transaction transaction)
        {
            if (string.IsNullOrWhiteSpace(transaction.Name))
                throw new Exception("Transaction name is required.");

            if (transaction.Name.Length > Transaction.MaxNameLength)
                throw new Exception($"Transaction name ({transaction.Name.Length}) is longer than {Transaction.MaxNameLength}");

            if (await _categoryRepository.Find(transaction.CategoryId) == null)
                throw new Exception($"Category {transaction.CategoryId} not found.");
        }

        private async Task CalculateTax(Transaction transaction)
        {
            var category = await _categoryRepository.Find(transaction.CategoryId) ?? throw new Exception($"Category {transaction.CategoryId} not found.");

            if (transaction.Sum > 0) { transaction.SumAfterTax = transaction.Sum * (1 - category.TaxAmount); }
            else { transaction.SumAfterTax = transaction.Sum; }
        }

        public async Task<decimal> GetCompanyBalance()
        {
            return CalculateProfit(await _transactionRepository.FindAll());
        }

        public async Task<decimal> GetMonthProfit(int month, int year)
        {
            return CalculateProfit(await GetMonthTransactions(month, year));
        }

        public async Task<Transaction[]> GetMonthTransactions(int month, int year)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            return await _transactionRepository.SelectInPeriod(start, end);
        }

        public async Task<decimal> GetPeriodProfit(DateTime start, DateTime end)
        {
            return CalculateProfit(await GetPeriodTransactions(start, end));
        }

        public async Task<Transaction[]> GetPeriodTransactions(DateTime start, DateTime end)
        {
            return await _transactionRepository.SelectInPeriod(start, end);
        }

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