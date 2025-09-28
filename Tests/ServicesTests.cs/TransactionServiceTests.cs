using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FinTrack.Models;
using FinTrack.Repositories;
using FinTrack.Services;
using Moq;
using Xunit;

namespace FinTrack.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly TransactionService _service;

        public TransactionServiceTests()
        {
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _categoryRepoMock = new Mock<ICategoryRepository>();

            _service = new TransactionService(_transactionRepoMock.Object, _categoryRepoMock.Object);
        }

        [Fact]
        public async Task Create_ShouldValidateAndCalculateTaxAndReturnCreatedTransaction()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Food", TaxAmount = 10m };
            var inputTransaction = new Transaction { Name = "Coffee", CategoryId = 1, Sum = 100m };

            _categoryRepoMock.Setup(c => c.Find(1)).ReturnsAsync(category);
            _transactionRepoMock.Setup(t => t.Create(It.IsAny<Transaction>()))
                                .ReturnsAsync((Transaction tr) => tr);

            // Act
            var result = await _service.Create(inputTransaction);

            // Assert
            Assert.Equal(90m, result.SumAfterTax); // 100 - 10%
            Assert.Equal("Coffee", result.Name);
            Assert.True(result.CreatedAt.Kind == DateTimeKind.Utc);
            _transactionRepoMock.Verify(t => t.Create(It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldValidateAndCalculateTaxAndCallRepositoryUpdate()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Food", TaxAmount = 20m };
            var transaction = new Transaction { Name = "Lunch", CategoryId = 1, Sum = 50m };

            _categoryRepoMock.Setup(c => c.Find(1)).ReturnsAsync(category);
            _transactionRepoMock.Setup(t => t.Update(It.IsAny<Transaction>()))
                                .Returns(Task.CompletedTask);

            // Act
            await _service.Update(transaction);

            // Assert
            Assert.Equal(40m, transaction.SumAfterTax); // 50 - 20%
            _transactionRepoMock.Verify(t => t.Update(It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldThrowException_WhenTransactionIsTooRecent()
        {
            // Arrange
            var t = new Transaction { Id = 1, Name = "X", CreatedAt = DateTime.UtcNow };
            _transactionRepoMock.Setup(r => r.Find(1)).ReturnsAsync(t);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.Delete(1));
            Assert.Contains("only after 14 days", ex.Message);
            _transactionRepoMock.Verify(r => r.HardDelete(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Delete_ShouldCallHardDelete_WhenTransactionIsOlderThan14Days()
        {
            // Arrange
            var t = new Transaction { Id = 1, Name = "X", CreatedAt = DateTime.UtcNow.AddDays(-15) };
            _transactionRepoMock.Setup(r => r.Find(1)).ReturnsAsync(t);

            // Act
            await _service.Delete(1);

            // Assert
            _transactionRepoMock.Verify(r => r.HardDelete(1), Times.Once);
        }

        [Fact]
        public async Task GetCompanyBalance_ShouldReturnSumOfAllTransactions()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction { SumAfterTax = 100m },
                new Transaction { SumAfterTax = 50m }
            };
            _transactionRepoMock.Setup(r => r.FindAll()).ReturnsAsync(transactions);

            // Act
            var balance = await _service.GetCompanyBalance();

            // Assert
            Assert.Equal(150m, balance);
        }

        [Fact]
        public async Task GetMonthProfit_ShouldFilterByMonthAndReturnSum()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction { SumAfterTax = 20m },
                new Transaction { SumAfterTax = 30m }
            };
            _transactionRepoMock.Setup(r => r.SelectInPeriod(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                                .ReturnsAsync(transactions);

            // Act
            var profit = await _service.GetMonthProfit(1, 2024);

            // Assert
            Assert.Equal(50m, profit);
            _transactionRepoMock.Verify(r => r.SelectInPeriod(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task GetPeriodTransactions_ShouldConvertDatesToUtcAndReturnTransactions()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Local);
            var end = new DateTime(2024, 1, 31, 12, 0, 0, DateTimeKind.Local);
            var transactions = new[] { new Transaction { SumAfterTax = 10m } };

            DateTime capturedStart = default, capturedEnd = default;
            _transactionRepoMock.Setup(r => r.SelectInPeriod(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((s, e) =>
                {
                    capturedStart = s;
                    capturedEnd = e;
                })
                .ReturnsAsync(transactions);

            // Act
            var result = await _service.GetPeriodTransactions(start, end);

            // Assert
            Assert.Same(transactions, result);
            Assert.Equal(DateTimeKind.Utc, capturedStart.Kind);
            Assert.Equal(DateTimeKind.Utc, capturedEnd.Kind);
        }
    }
}
