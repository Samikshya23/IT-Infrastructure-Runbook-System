using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IReportRepository
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