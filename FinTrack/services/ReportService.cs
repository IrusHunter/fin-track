using FinTrack.Models;
using FinTrack.Repositories;

namespace FinTrack.Services
{
    public interface IReportService
    {
        public Task<IDictionary<string, decimal>> GetCategoryReport(DateTime start, DateTime end);
    }
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<IDictionary<string, decimal>> GetCategoryReport(DateTime start, DateTime end)
        {
            start = start.ToUniversalTime();
            end = end.ToUniversalTime();
            return await _reportRepository.GetCategoryReport(start, end);
        }
    }
}