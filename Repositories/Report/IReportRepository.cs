using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IReportRepository
    {
        Task<IEnumerable<ReportCategory>> GetCategoryListAsync();

        Task<string> GetHeadingsAsync(int categoryId);

        Task<IEnumerable<Report>> GetDataAsync(int categoryId, DateTime fromDate, DateTime toDate);
    }
}