using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IReportService
    {
        Task<IEnumerable<ReportCategory>> GetCategoryListAsync();

        Task<string> GetHeadingsAsync(int categoryId);

        Task<IEnumerable<Report>> GetDataAsync(
            int categoryId,
            DateTime fromDate,
            DateTime toDate
        );

        Task<IEnumerable<Report>> GetAllDataAsync(
            DateTime fromDate,
            DateTime toDate,
            int? categoryId = null
        );
    }
}