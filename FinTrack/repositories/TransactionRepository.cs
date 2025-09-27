using System;
using System.Threading.Tasks;
using FinTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Repositories
{
    public interface ITransactionRepository
    {
        Task<Transaction> Create(Transaction transaction);
        Task<Transaction?> Find(int id);
        Task<Transaction?> FindByName(string name);
        Task<Transaction[]> FindAll();
        Task Update(Transaction transaction);
        Task HardDelete(int id);
        Task<Transaction> FillModel(Transaction transaction);
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
    }
}
