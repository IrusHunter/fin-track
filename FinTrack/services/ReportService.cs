using FinTrack.Models;
using FinTrack.Repositories;

namespace FinTrack.Services
{
    /// <summary>
    /// Interface defining service operations for generating financial reports.
    /// </summary>
    public interface IReportService
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
    /// Service class implementing business logic for generating financial reports.
    /// Uses <see cref="IReportRepository"/> for data access.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportService"/> class.
        /// </summary>
        /// <param name="reportRepository">The repository used for report data access.</param>
        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, decimal>> GetCategoryReport(DateTime start, DateTime end)
        {
            start = start.ToUniversalTime();
            end = end.ToUniversalTime();
            return await _reportRepository.GetCategoryReport(start, end);
        }
    }
}