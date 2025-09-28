using System;
using System.Linq;
using System.Threading.Tasks;
using FinTrack.Models;
using FinTrack.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinTrack.Tests.Repositories
{
    public class TransactionRepositoryTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Create_ShouldAddTransaction()
        {
            using var db = GetDbContext(nameof(Create_ShouldAddTransaction));
            var repo = new TransactionRepository(db);

            var transaction = new Transaction { Name = "Coffee", SumAfterTax = 10, CreatedAt = DateTime.UtcNow };

            var result = await repo.Create(transaction);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Coffee", result.Name);
        }

        [Fact]
        public async Task Find_ShouldReturnTransaction_WhenExists()
        {
            using var db = GetDbContext(nameof(Find_ShouldReturnTransaction_WhenExists));
            var repo = new TransactionRepository(db);

            var t = new Transaction { Name = "Lunch", SumAfterTax = 20, CreatedAt = DateTime.UtcNow };
            db.Transactions.Add(t);
            await db.SaveChangesAsync();

            var result = await repo.Find(t.Id);

            Assert.NotNull(result);
            Assert.Equal("Lunch", result!.Name);
        }

        [Fact]
        public async Task FindByName_ShouldReturnTransaction_WhenExists()
        {
            using var db = GetDbContext(nameof(FindByName_ShouldReturnTransaction_WhenExists));
            var repo = new TransactionRepository(db);

            db.Transactions.Add(new Transaction { Name = "Book", SumAfterTax = 30, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var result = await repo.FindByName("Book");

            Assert.NotNull(result);
            Assert.Equal("Book", result!.Name);
        }

        [Fact]
        public async Task FindAll_ShouldReturnAllTransactions()
        {
            using var db = GetDbContext(nameof(FindAll_ShouldReturnAllTransactions));
            var repo = new TransactionRepository(db);

            db.Transactions.AddRange(
                new Transaction { Name = "A", SumAfterTax = 1, CreatedAt = DateTime.UtcNow },
                new Transaction { Name = "B", SumAfterTax = 2, CreatedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();

            var result = await repo.FindAll();

            Assert.Equal(2, result.Length);
        }

        [Fact]
        public async Task Update_ShouldModifyTransaction()
        {
            using var db = GetDbContext(nameof(Update_ShouldModifyTransaction));
            var repo = new TransactionRepository(db);

            var t = new Transaction { Name = "Old", SumAfterTax = 100, CreatedAt = DateTime.UtcNow };
            db.Transactions.Add(t);
            await db.SaveChangesAsync();

            t.Name = "New";
            await repo.Update(t);

            var updated = await repo.Find(t.Id);

            Assert.NotNull(updated);
            Assert.Equal("New", updated!.Name);
        }

        [Fact]
        public async Task HardDelete_ShouldRemoveTransaction()
        {
            using var db = GetDbContext(nameof(HardDelete_ShouldRemoveTransaction));
            var repo = new TransactionRepository(db);

            var t = new Transaction { Name = "ToDelete", SumAfterTax = 50, CreatedAt = DateTime.UtcNow };
            db.Transactions.Add(t);
            await db.SaveChangesAsync();

            await repo.HardDelete(t.Id);

            var result = await repo.Find(t.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task FillModel_ShouldLoadCategory()
        {
            using var db = GetDbContext(nameof(FillModel_ShouldLoadCategory));
            var repo = new TransactionRepository(db);

            var category = new Category { Name = "Food" };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            var t = new Transaction { Name = "Pizza", SumAfterTax = 25, CreatedAt = DateTime.UtcNow, CategoryId = category.Id };
            db.Transactions.Add(t);
            await db.SaveChangesAsync();

            var result = await repo.FillModel(t);

            Assert.NotNull(result.Category);
            Assert.Equal("Food", result.Category.Name);
        }

        [Fact]
        public async Task SelectInPeriod_ShouldReturnOnlyTransactionsWithinRange()
        {
            using var db = GetDbContext(nameof(SelectInPeriod_ShouldReturnOnlyTransactionsWithinRange));
            var repo = new TransactionRepository(db);

            var t1 = new Transaction { Name = "InRange1", SumAfterTax = 10, CreatedAt = new DateTime(2024, 01, 10) };
            var t2 = new Transaction { Name = "InRange2", SumAfterTax = 20, CreatedAt = new DateTime(2024, 01, 15) };
            var t3 = new Transaction { Name = "OutOfRange", SumAfterTax = 30, CreatedAt = new DateTime(2023, 12, 25) };
            db.Transactions.AddRange(t1, t2, t3);
            await db.SaveChangesAsync();

            var start = new DateTime(2024, 01, 01);
            var end = new DateTime(2024, 01, 31);

            var result = await repo.SelectInPeriod(start, end);

            Assert.Equal(2, result.Length);
            Assert.Contains(result, t => t.Name == "InRange1");
            Assert.Contains(result, t => t.Name == "InRange2");
            Assert.DoesNotContain(result, t => t.Name == "OutOfRange");
        }
    }
}
