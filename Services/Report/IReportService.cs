using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IReportService
    {
        Task<IEnumerable<ReportProduct>> GetProductsAsync();

        Task<string> GetHeadingsAsync(int productId);

        Task<IEnumerable<Report>> GetDataAsync(
            int productId,
            DateTime fromDate,
            DateTime toDate
        );
    }
}