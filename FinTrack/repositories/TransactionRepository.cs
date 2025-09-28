using System;
using System.Threading.Tasks;
using FinTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Repositories
{
    public interface ITransactionRepository
    {
        public Task<Transaction> Create(Transaction transaction);
        public Task<Transaction?> Find(int id);
        public Task<Transaction?> FindByName(string name);
        public Task<Transaction[]> FindAll();
        public Task Update(Transaction transaction);
        public Task HardDelete(int id);
        public Task<Transaction> FillModel(Transaction transaction);
        public Task<Transaction[]> SelectInPeriod(DateTime start, DateTime end);

    }

    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _db;

        public TransactionRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Transaction> Create(Transaction transaction)
        {
            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();
            return transaction;
        }

        public async Task<Transaction?> Find(int id)
        {
            return await _db.Transactions.SingleOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Transaction?> FindByName(string name)
        {
            return await _db.Transactions.SingleOrDefaultAsync(t => t.Name == name);
        }

        public async Task<Transaction[]> FindAll()
        {
            return await _db.Transactions.ToArrayAsync();
        }

        public async Task Update(Transaction transaction)
        {
            _db.Transactions.Update(transaction);
            await _db.SaveChangesAsync();
        }

        public async Task HardDelete(int id)
        {
            var transaction = await Find(id) ?? throw new Exception($"Transaction {id} not found");
            _db.Transactions.Remove(transaction);
            await _db.SaveChangesAsync();
        }

        public async Task<Transaction> FillModel(Transaction transaction)
        {
            await _db.Entry(transaction).Reference(t => t.Category).LoadAsync();
            return transaction;
        }

        public async Task<Transaction[]> SelectInPeriod(DateTime start, DateTime end)
        {
            var transactions = await _db.Transactions.Where(t => t.CreatedAt > start && t.CreatedAt < end).ToArrayAsync();
            return transactions;
        }

    }
}
