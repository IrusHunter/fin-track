using System;
using System.Linq;
using System.Threading.Tasks;
using FinTrack.Models;
using FinTrack.Repositories;
using FinTrack.Services;
using Moq;
using Xunit;

namespace FinTrack.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _repoMock;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _repoMock = new Mock<ICategoryRepository>();
            _service = new CategoryService(_repoMock.Object);
        }

        [Fact]
        public async Task Create_ValidCategory_CallsRepositoryAndSetsTimestamps()
        {
            // Arrange
            var cat = new Category { Name = "Food", TaxAmount = 5m };
            _repoMock.Setup(r => r.FindByName(cat.Name)).ReturnsAsync((Category?)null);
            _repoMock.Setup(r => r.Create(It.IsAny<Category>()))
                     .ReturnsAsync((Category c) => c);

            // Act
            var created = await _service.Create(cat);

            // Assert
            _repoMock.Verify(r => r.Create(It.Is<Category>(
                c => c.Name == "Food" && c.CreatedAt != default && c.UpdatedAt != default)), Times.Once);
            Assert.Equal("Food", created.Name);
        }

        [Fact]
        public async Task Create_WhenNameTooLong_ThrowsException()
        {
            var cat = new Category
            {
                Name = new string('x', Category.MaxNameLength + 1),
                TaxAmount = 5m
            };
            _repoMock.Setup(r => r.FindByName(It.IsAny<string>())).ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.Create(cat));
        }

        [Fact]
        public async Task Create_WhenDuplicateName_ThrowsException()
        {
            var cat = new Category { Id = 1, Name = "Food", TaxAmount = 5m };
            _repoMock.Setup(r => r.FindByName("Food")).ReturnsAsync(new Category { Id = 2, Name = "Food" });

            await Assert.ThrowsAsync<Exception>(() => _service.Create(cat));
        }

        [Fact]
        public async Task Update_ValidCategory_CallsRepositoryAndSetsUpdatedAt()
        {
            var cat = new Category { Id = 1, Name = "Food", TaxAmount = 5m };
            _repoMock.Setup(r => r.FindByName("Food")).ReturnsAsync(cat);

            await _service.Update(cat);

            _repoMock.Verify(r => r.Update(It.Is<Category>(
                c => c.Id == 1 && c.UpdatedAt != default)), Times.Once);
        }

        [Fact]
        public async Task Delete_NoTransactions_CallsHardDelete()
        {
            var cat = new Category { Id = 1, Name = "Food", Transactions = Array.Empty<Transaction>() };
            _repoMock.Setup(r => r.Find(1)).ReturnsAsync(cat);

            await _service.Delete(1);

            _repoMock.Verify(r => r.HardDelete(1), Times.Once);
            _repoMock.Verify(r => r.SoftDelete(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WithTransactions_CallsSoftDelete()
        {
            var cat = new Category
            {
                Id = 1,
                Name = "Food",
                Transactions = new[] { new Transaction() }
            };
            _repoMock.Setup(r => r.Find(1)).ReturnsAsync(cat);

            await _service.Delete(1);

            _repoMock.Verify(r => r.SoftDelete(1), Times.Once);
            _repoMock.Verify(r => r.HardDelete(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Delete_NotFound_ThrowsException()
        {
            _repoMock.Setup(r => r.Find(1)).ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<Exception>(() => _service.Delete(1));
        }

        [Fact]
        public async Task Find_ReturnsCategoryFromRepository()
        {
            var cat = new Category { Id = 1, Name = "Food" };
            _repoMock.Setup(r => r.Find(1)).ReturnsAsync(cat);

            var result = await _service.Find(1);

            Assert.Equal(cat, result);
        }

        [Fact]
        public async Task FindAll_ReturnsCategoriesFromRepository()
        {
            var cats = new[] { new Category { Id = 1, Name = "Food" } };
            _repoMock.Setup(r => r.FindAll()).ReturnsAsync(cats);

            var result = await _service.FindAll();

            Assert.Single(result);
            Assert.Equal("Food", result.First().Name);
        }
    }
}
