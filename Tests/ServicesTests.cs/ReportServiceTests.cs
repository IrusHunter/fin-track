using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTrack.Repositories;
using FinTrack.Services;
using Moq;
using Xunit;

namespace FinTrack.Tests.Services
{
    public class ReportServiceTests
    {
        [Fact]
        public async Task GetCategoryReport_ShouldReturnRepositoryResult()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Local);
            var end = new DateTime(2024, 1, 31, 12, 0, 0, DateTimeKind.Local);

            var expectedResult = new Dictionary<string, decimal>
            {
                { "Food", 100m },
                { "Transport", 50m }
            };

            var repoMock = new Mock<IReportRepository>();
            repoMock
                .Setup(r => r.GetCategoryReport(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(expectedResult);

            var service = new ReportService(repoMock.Object);

            // Act
            var result = await service.GetCategoryReport(start, end);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetCategoryReport_ShouldConvertDatesToUtc()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Local);
            var end = new DateTime(2024, 1, 31, 12, 0, 0, DateTimeKind.Local);

            var repoMock = new Mock<IReportRepository>();
            DateTime? capturedStart = null;
            DateTime? capturedEnd = null;

            repoMock
                .Setup(r => r.GetCategoryReport(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((s, e) =>
                {
                    capturedStart = s;
                    capturedEnd = e;
                })
                .ReturnsAsync(new Dictionary<string, decimal>());

            var service = new ReportService(repoMock.Object);

            // Act
            await service.GetCategoryReport(start, end);

            // Assert: перевіряємо, що передані саме UTC-значення
            Assert.Equal(DateTimeKind.Utc, capturedStart!.Value.Kind);
            Assert.Equal(DateTimeKind.Utc, capturedEnd!.Value.Kind);
            Assert.Equal(start.ToUniversalTime(), capturedStart!.Value);
            Assert.Equal(end.ToUniversalTime(), capturedEnd!.Value);
        }
    }
}
