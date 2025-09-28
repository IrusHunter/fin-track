using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTrack.Models;
using FinTrack.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinTrack.Tests.Repositories
{
    public class ReportRepositoryTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetCategoryReport_ShouldReturnCorrectSumsWithinDateRange()
        {
            using var db = GetDbContext(nameof(GetCategoryReport_ShouldReturnCorrectSumsWithinDateRange));
            var repo = new ReportRepository(db);

            var food = new Category { Name = "Food" };
            var transport = new Category { Name = "Transport" };
            db.Categories.AddRange(food, transport);
            await db.SaveChangesAsync();

            db.Transactions.AddRange(
                new Transaction { CategoryId = food.Id, SumAfterTax = 100, CreatedAt = new DateTime(2024, 01, 05) },
                new Transaction { CategoryId = food.Id, SumAfterTax = 200, CreatedAt = new DateTime(2024, 01, 10) },
                new Transaction { CategoryId = transport.Id, SumAfterTax = 300, CreatedAt = new DateTime(2024, 01, 15) },
                new Transaction { CategoryId = transport.Id, SumAfterTax = 999, CreatedAt = new DateTime(2023, 12, 25) } // поза діапазоном
            );
            await db.SaveChangesAsync();

            var start = new DateTime(2024, 01, 01);
            var end = new DateTime(2024, 01, 31);

            var result = await repo.GetCategoryReport(start, end);

            Assert.Equal(2, result.Count);
            Assert.Equal(300, result["Food"]);
            Assert.Equal(300, result["Transport"]);
        }

        [Fact]
        public async Task GetCategoryReport_ShouldReturnEmpty_WhenNoTransactionsInRange()
        {
            using var db = GetDbContext(nameof(GetCategoryReport_ShouldReturnEmpty_WhenNoTransactionsInRange));
            var repo = new ReportRepository(db);

            var category = new Category { Name = "Misc" };
            db.Categories.Add(category);
            db.Transactions.Add(new Transaction
            {
                CategoryId = category.Id,
                SumAfterTax = 500,
                CreatedAt = new DateTime(2023, 01, 01) // поза діапазоном
            });
            await db.SaveChangesAsync();

            var start = new DateTime(2024, 01, 01);
            var end = new DateTime(2024, 01, 31);

            var result = await repo.GetCategoryReport(start, end);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCategoryReport_ShouldGroupByCategoryCorrectly()
        {
            using var db = GetDbContext(nameof(GetCategoryReport_ShouldGroupByCategoryCorrectly));
            var repo = new ReportRepository(db);

            var c1 = new Category { Name = "Health" };
            var c2 = new Category { Name = "Entertainment" };
            db.Categories.AddRange(c1, c2);
            await db.SaveChangesAsync();

            db.Transactions.AddRange(
                new Transaction { CategoryId = c1.Id, SumAfterTax = 50, CreatedAt = DateTime.UtcNow },
                new Transaction { CategoryId = c1.Id, SumAfterTax = 150, CreatedAt = DateTime.UtcNow },
                new Transaction { CategoryId = c2.Id, SumAfterTax = 200, CreatedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();

            var result = await repo.GetCategoryReport(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.Equal(2, result.Count);
            Assert.Equal(200, result["Health"]);
            Assert.Equal(200, result["Entertainment"]);
        }
    }
}
