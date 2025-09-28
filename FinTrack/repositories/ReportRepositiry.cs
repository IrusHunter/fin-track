
using System.Data.Common;
using System.Threading.Tasks;
using FinTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Repositories
{
    /// <summary>
    /// Interface defining methods for generating financial reports.
    /// </summary>
    public interface IReportRepository
    {
        /// <summary>
        /// Generates a category report showing total sum after tax for each category
        /// within the specified date range.
        /// </summary>
        /// <param name="start">Start date of the reporting period (inclusive).</param>
        /// <param name="end">End date of the reporting period (inclusive).</param>
        /// <returns>
        /// A dictionary where the key is the category name and the value is the total sum after tax for that category.
        /// </returns>
        public Task<IDictionary<string, decimal>> GetCategoryReport(DateTime start, DateTime end);
    }

    /// <summary>
    /// Repository class for generating financial reports.
    /// Implements <see cref="IReportRepository"/> using Entity Framework Core.
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportRepository"/> class
        /// with the specified database context.
        /// </summary>
        /// <param name="db">The <see cref="ApplicationDbContext"/> used for data access operations.</param>
        public ReportRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, decimal>> GetCategoryReport(DateTime start, DateTime end)
        {
            var result = await _db.Transactions
                .Where(t => t.CreatedAt >= start && t.CreatedAt <= end)
                .GroupBy(t => t.Category.Name)
                .Select(g => new { CategoryName = g.Key, TotalSum = g.Sum(t => t.SumAfterTax) })
                .ToDictionaryAsync(x => x.CategoryName, x => x.TotalSum);

            return result;
        }
    }

}
