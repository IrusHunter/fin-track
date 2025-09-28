using System;
using System.Linq;
using System.Threading.Tasks;
using FinTrack.Models;
using FinTrack.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinTrack.Tests.Repositories
{
    public class CategoryRepositoryTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Create_ShouldAddCategory()
        {
            using var db = GetDbContext(nameof(Create_ShouldAddCategory));
            var repo = new CategoryRepository(db);

            var category = new Category { Name = "Food" };
            var result = await repo.Create(category);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Food", result.Name);
        }

        [Fact]
        public async Task Find_ShouldReturnCategory_WhenExists()
        {
            using var db = GetDbContext(nameof(Find_ShouldReturnCategory_WhenExists));
            var repo = new CategoryRepository(db);

            var category = new Category { Name = "Transport" };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            var result = await repo.Find(category.Id);

            Assert.NotNull(result);
            Assert.Equal("Transport", result!.Name);
        }

        [Fact]
        public async Task FindByName_ShouldReturnCategory_WhenExists()
        {
            using var db = GetDbContext(nameof(FindByName_ShouldReturnCategory_WhenExists));
            var repo = new CategoryRepository(db);

            db.Categories.Add(new Category { Name = "Health" });
            await db.SaveChangesAsync();

            var result = await repo.FindByName("Health");

            Assert.NotNull(result);
            Assert.Equal("Health", result!.Name);
        }

        [Fact]
        public async Task FindAll_ShouldReturnOnlyActiveCategories()
        {
            using var db = GetDbContext(nameof(FindAll_ShouldReturnOnlyActiveCategories));
            var repo = new CategoryRepository(db);

            db.Categories.Add(new Category { Name = "Work" });
            db.Categories.Add(new Category { Name = "Old", DeletedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var result = await repo.FindAll();

            Assert.Single(result);
            Assert.Equal("Work", result[0].Name);
        }

        [Fact]
        public async Task Update_ShouldModifyCategory()
        {
            using var db = GetDbContext(nameof(Update_ShouldModifyCategory));
            var repo = new CategoryRepository(db);

            var category = new Category { Name = "OldName" };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            category.Name = "NewName";
            await repo.Update(category);

            var updated = await repo.Find(category.Id);

            Assert.NotNull(updated);
            Assert.Equal("NewName", updated!.Name);
        }

        [Fact]
        public async Task HardDelete_ShouldRemoveCategory()
        {
            using var db = GetDbContext(nameof(HardDelete_ShouldRemoveCategory));
            var repo = new CategoryRepository(db);

            var category = new Category { Name = "Temp" };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            await repo.HardDelete(category.Id);

            var result = await repo.Find(category.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task SoftDelete_ShouldMarkCategoryAsDeleted()
        {
            using var db = GetDbContext(nameof(SoftDelete_ShouldMarkCategoryAsDeleted));
            var repo = new CategoryRepository(db);

            var category = new Category { Name = "Temp" };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            await repo.SoftDelete(category.Id);

            var result = await repo.Find(category.Id);
            Assert.NotNull(result);
            Assert.NotNull(result!.DeletedAt);
        }

        [Fact]
        public async Task FillModel_ShouldLoadTransactions()
        {
            using var db = GetDbContext(nameof(FillModel_ShouldLoadTransactions));
            var repo = new CategoryRepository(db);

            var category = new Category { Name = "Entertainment" };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            // Додаємо транзакцію напряму
            db.Transactions.Add(new Transaction { Sum = 50, CategoryId = category.Id });
            await db.SaveChangesAsync();

            var result = await repo.FillModel(category);

            Assert.NotNull(result.Transactions);
            Assert.Single(result.Transactions);
            Assert.Equal(50, result.Transactions.First().Sum);
        }
    }
}
