
using System.Data.Common;
using System.Threading.Tasks;
using FinTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Repositories
{
    public interface IReportRepository
    {
        public Task<IDictionary<string, decimal>> GetCategoryReport(DateTime start, DateTime end);
    }

    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportRepository"/> class with the specified database context.
        /// </summary>
        /// <param name="db">The <see cref="ApplicationDbContext"/> used for data access operations.</param>
        public ReportRepository(ApplicationDbContext db)
        {
            _db = db;
        }

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
